using TMPro;
using UnityEngine;

namespace MarioBros.UI
{
    /// <summary>
    /// Controlador de la interfaz de usuario en pantalla (HUD) estilo NES para Super Mario Bros.
    /// Muestra puntuación con 6 dígitos, monedas con 2 dígitos, nombre del mundo, tiempo y vidas.
    /// Se suscribe reactivamente a los eventos de GameManager.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Textos del HUD (TextMeshProUGUI)")]
        [Tooltip("Texto para la puntuación (formato 6 dígitos: 000000).")]
        [SerializeField] private TextMeshProUGUI scoreText;

        [Tooltip("Texto para el contador de monedas (formato 2 dígitos: x00).")]
        [SerializeField] private TextMeshProUGUI coinsText;

        [Tooltip("Texto para el nombre del mundo actual (ej: 1-1).")]
        [SerializeField] private TextMeshProUGUI worldText;

        [Tooltip("Texto para el temporizador regresivo del nivel (ej: 400).")]
        [SerializeField] private TextMeshProUGUI timeText;

        [Tooltip("Texto para la cantidad de vidas restantes de Mario (ej: x 3).")]
        [SerializeField] private TextMeshProUGUI livesText;

        [Header("Carteles de Estado del Juego")]
        [Tooltip("Elemento de texto o GameObject que muestra el cartel de GAME OVER.")]
        [SerializeField] private GameObject gameOverPanel;

        [Tooltip("Texto específico de Game Over.")]
        [SerializeField] private TextMeshProUGUI gameOverText;

        [Tooltip("Elemento de texto o GameObject que muestra TIME UP.")]
        [SerializeField] private GameObject timeUpPanel;

        [Tooltip("Texto específico de Time Up.")]
        [SerializeField] private TextMeshProUGUI timeUpText;

        [Header("Configuración de Nivel")]
        [Tooltip("Identificador o nombre del mundo mostrado en pantalla.")]
        [SerializeField] private string currentWorldName = "1-1";

        private void OnEnable()
        {
            // Suscribirse a los eventos de GameManager
            GameManager.OnScoreChanged += UpdateScore;
            GameManager.OnCoinsChanged += UpdateCoins;
            GameManager.OnLivesChanged += UpdateLives;
            GameManager.OnTimerChanged += UpdateTimer;
            GameManager.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            // Desuscribirse para evitar fugas de memoria
            GameManager.OnScoreChanged -= UpdateScore;
            GameManager.OnCoinsChanged -= UpdateCoins;
            GameManager.OnLivesChanged -= UpdateLives;
            GameManager.OnTimerChanged -= UpdateTimer;
            GameManager.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void Start()
        {
            // Configuración inicial de visuales
            if (worldText != null)
            {
                worldText.text = currentWorldName;
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }

            if (timeUpPanel != null)
            {
                timeUpPanel.SetActive(false);
            }

            // Sincronizar valores actuales desde GameManager si ya existe
            if (GameManager.Instance != null)
            {
                GameManager.Instance.NotifyAllEvents();
            }
            else
            {
                UpdateScore(GameManager.Score);
                UpdateCoins(GameManager.Coins);
                UpdateLives(GameManager.Lives);
                UpdateTimer(GameManager.TimeRemaining);
            }
        }

        /// <summary>
        /// Actualiza la visualización del puntaje con 6 dígitos enteros y ceros a la izquierda.
        /// </summary>
        public void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = score.ToString("D6");
            }
        }

        /// <summary>
        /// Actualiza la visualización de monedas en formato 'x00'.
        /// </summary>
        public void UpdateCoins(int coins)
        {
            if (coinsText != null)
            {
                coinsText.text = "x" + coins.ToString("D2");
            }
        }

        /// <summary>
        /// Actualiza el contador de vidas de Mario.
        /// </summary>
        public void UpdateLives(int lives)
        {
            if (livesText != null)
            {
                livesText.text = "x " + Mathf.Max(0, lives).ToString();
            }
        }

        /// <summary>
        /// Actualiza el temporizador del nivel con 3 dígitos.
        /// </summary>
        public void UpdateTimer(int timeRemaining)
        {
            if (timeText != null)
            {
                timeText.text = timeRemaining.ToString("D3");
            }

            // Mostrar aviso de TIME UP si el contador llega exactamente a cero
            if (timeRemaining <= 0)
            {
                ShowTimeUp();
            }
        }

        /// <summary>
        /// Responde a los cambios en el estado del juego.
        /// </summary>
        private void HandleGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.GameOver:
                    ShowGameOver();
                    break;
                case GameState.Playing:
                    HideGameOver();
                    HideTimeUp();
                    break;
            }
        }

        /// <summary>
        /// Muestra el texto/panel de GAME OVER en pantalla.
        /// </summary>
        public void ShowGameOver()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
            else if (gameOverText != null)
            {
                gameOverText.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Oculta el panel de GAME OVER.
        /// </summary>
        public void HideGameOver()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
            else if (gameOverText != null)
            {
                gameOverText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Muestra el cartel de TIME UP.
        /// </summary>
        public void ShowTimeUp()
        {
            if (timeUpPanel != null)
            {
                timeUpPanel.SetActive(true);
            }
            else if (timeUpText != null)
            {
                timeUpText.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Oculta el cartel de TIME UP.
        /// </summary>
        public void HideTimeUp()
        {
            if (timeUpPanel != null)
            {
                timeUpPanel.SetActive(false);
            }
            else if (timeUpText != null)
            {
                timeUpText.gameObject.SetActive(false);
            }
        }
    }
}
