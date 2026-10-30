using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MarioBros.UI
{
    /// <summary>
    /// Posibles estados del juego en Super Mario Bros.
    /// </summary>
    public enum GameState
    {
        Playing,
        Paused,
        GameOver,
        LevelComplete
    }

    /// <summary>
    /// Administrador global del estado del juego (Singleton).
    /// Controla la puntuación, monedas, vidas restantes, temporizador de nivel
    /// y notifica cambios a través de eventos en C# para la interfaz de usuario (HUD).
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Configuración Inicial")]
        [Tooltip("Cantidad de vidas iniciales de Mario.")]
        [SerializeField] private int startingLives = 3;

        [Tooltip("Tiempo inicial del nivel en segundos (clásico de NES: 400).")]
        [SerializeField] private int startingTime = 400;

        [Tooltip("Segundos reales por cada unidad del temporizador en el juego.")]
        [SerializeField] private float timeStepDuration = 0.6f;

        [Header("Audio Opcional")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip coinSound;
        [SerializeField] private AudioClip oneUpSound;
        [SerializeField] private AudioClip gameOverSound;

        // Variables de estado del juego
        public static int Score { get; private set; } = 0;
        public static int Coins { get; private set; } = 0;
        public static int Lives { get; private set; } = 3;
        public static int TimeRemaining { get; private set; } = 400;
        public static GameState CurrentState { get; private set; } = GameState.Playing;

        // Eventos para actualización de UI
        public static event Action<int> OnScoreChanged;
        public static event Action<int> OnCoinsChanged;
        public static event Action<int> OnLivesChanged;
        public static event Action<int> OnTimerChanged;
        public static event Action<GameState> OnGameStateChanged;

        private float timerAcc = 0f;
        private bool isTimerRunning = false;
        private bool isReloading;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            InitializeGame();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (Instance != this) return;
            isReloading = false;
            if (scene.name == "World2_3Pending")
            {
                isTimerRunning = false;
                SetState(GameState.LevelComplete);
                return;
            }
            if (scene.name == "GameOver")
            {
                isTimerRunning = false;
                SetState(GameState.GameOver);
                return;
            }
            // Reiniciar temporizador al cargar nuevo nivel si estamos jugando
            if (CurrentState != GameState.GameOver)
            {
                ResetTimer();
                SetState(GameState.Playing);
            }
        }

        private void Update()
        {
            // Manejo del temporizador durante el juego activo
            if (CurrentState == GameState.Playing && isTimerRunning)
            {
                timerAcc += Time.deltaTime;
                if (timerAcc >= timeStepDuration)
                {
                    timerAcc -= timeStepDuration;
                    DecrementTimer();
                }
            }
        }

        /// <summary>
        /// Inicializa todas las variables del juego para una nueva partida.
        /// </summary>
        public void InitializeGame()
        {
            Time.timeScale = 1f;
            isReloading = false;
            Score = 0;
            Coins = 0;
            Lives = startingLives;
            TimeRemaining = startingTime;
            timerAcc = 0f;
            isTimerRunning = true;
            CurrentState = GameState.Playing;

            NotifyAllEvents();
        }

        /// <summary>
        /// Dispara todos los eventos para sincronizar los HUDs recién creados.
        /// </summary>
        public void NotifyAllEvents()
        {
            OnScoreChanged?.Invoke(Score);
            OnCoinsChanged?.Invoke(Coins);
            OnLivesChanged?.Invoke(Lives);
            OnTimerChanged?.Invoke(TimeRemaining);
            OnGameStateChanged?.Invoke(CurrentState);
        }

        /// <summary>
        /// Añade puntos a la puntuación total.
        /// </summary>
        public void AddScore(int points)
        {
            if (points <= 0) return;

            Score += points;
            OnScoreChanged?.Invoke(Score);
        }

        /// <summary>
        /// Añade una moneda. Otorga 200 puntos y, al alcanzar 100 monedas, una vida extra (1-Up).
        /// </summary>
        public void AddCoin()
        {
            Coins++;
            AddScore(200);

            if (coinSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(coinSound);
            }

            if (Coins >= 100)
            {
                Coins -= 100;
                AddLife();
            }

            OnCoinsChanged?.Invoke(Coins);
        }

        /// <summary>
        /// Incrementa una vida al jugador.
        /// </summary>
        public void AddLife()
        {
            Lives++;
            if (oneUpSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(oneUpSound);
            }
            OnLivesChanged?.Invoke(Lives);
        }

        /// <summary>
        /// Resta una vida a Mario. Si no quedan vidas, activa GameOver.
        /// </summary>
        public void LoseLife()
        {
            if (isReloading || CurrentState != GameState.Playing) return;
            isReloading = true;
            isTimerRunning = false;
            Lives = Mathf.Max(0, Lives - 1);
            OnLivesChanged?.Invoke(Lives);

            if (Lives <= 0)
            {
                TriggerGameOver();
            }
            else
            {
                ResetLevel();
            }
        }

        /// <summary>
        /// Decrementa el temporizador del nivel.
        /// </summary>
        private void DecrementTimer()
        {
            if (TimeRemaining > 0)
            {
                TimeRemaining--;
                OnTimerChanged?.Invoke(TimeRemaining);

                if (TimeRemaining == 0)
                {
                    OnTimeUp();
                }
            }
        }

        /// <summary>
        /// Se ejecuta al agotarse el tiempo del nivel.
        /// </summary>
        private void OnTimeUp()
        {
            isTimerRunning = false;

            // Notificar a Mario para que muera si existe en la escena
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Player.PlayerHealth health = player.GetComponent<Player.PlayerHealth>();
                if (health != null && !health.IsDead)
                {
                    health.Die();
                    return;
                }
            }

            LoseLife();
        }

        /// <summary>
        /// Reinicia el tiempo al valor inicial del nivel.
        /// </summary>
        public void ResetTimer()
        {
            TimeRemaining = startingTime;
            timerAcc = 0f;
            isTimerRunning = true;
            OnTimerChanged?.Invoke(TimeRemaining);
        }

        /// <summary>
        /// Reinicia la escena del nivel actual manteniendo la puntuación y vidas.
        /// </summary>
        public void ResetLevel()
        {
            StartCoroutine(ReloadSceneRoutine());
        }

        private IEnumerator ReloadSceneRoutine()
        {
            yield return new WaitForSecondsRealtime(0.5f);
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }

        /// <summary>
        /// Completa el nivel actual.
        /// </summary>
        public void LevelCompleted()
        {
            SetState(GameState.LevelComplete);
            isTimerRunning = false;
        }

        /// <summary>
        /// Activa la pantalla de fin de juego (Game Over).
        /// </summary>
        private void TriggerGameOver()
        {
            SetState(GameState.GameOver);
            isTimerRunning = false;

            if (gameOverSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(gameOverSound);
            }
            Time.timeScale = 1f;
            if (Application.CanStreamedLevelBeLoaded("GameOver"))
                SceneManager.LoadScene("GameOver");
            else
                GameOverScreen.ShowRuntimeScene();
        }

        public void RestartGame()
        {
            StopAllCoroutines();
            InitializeGame();
            SceneManager.LoadScene("SampleScene");
        }

        /// <summary>
        /// Cambia el estado actual del juego y notifica a los suscriptores.
        /// </summary>
        public void SetState(GameState newState)
        {
            CurrentState = newState;
            OnGameStateChanged?.Invoke(CurrentState);

            if (newState == GameState.Paused)
            {
                Time.timeScale = 0f;
            }
            else if (newState == GameState.Playing)
            {
                Time.timeScale = 1f;
            }
        }
    }
}
