using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MarioBros.UI;

namespace MarioBros.Sound
{
    /// <summary>
    /// Administrador de música de fondo (Singleton, persiste entre escenas).
    /// - Reproduce la pista asignada a cada escena (por nombre) con loop y fade.
    /// - Usa su PROPIO AudioSource: los efectos de sonido (PlayClipAtPoint / PlayOneShot
    ///   del GameManager) no se cortan ni se pisan con la música.
    /// - Se instancia solo desde Resources/MusicManager.prefab, así no hay que tocar
    ///   ninguna escena (menos conflictos de merge en git).
    /// </summary>
    [DisallowMultipleComponent]
    public class MusicManager : MonoBehaviour
    {
        [Serializable]
        public class SceneTrack
        {
            [Tooltip("Nombre exacto de la escena (ej. SampleScene).")]
            public string sceneName;
            public AudioClip clip;
            public bool loop = true;
            [Range(0f, 1f)] public float volume = 1f;
            [Tooltip("Si es true, la música se detiene al terminar el nivel o en Game Over (la bandera ya suena su propio jingle).")]
            public bool stopWhenLevelEnds = true;
        }

        public static MusicManager Instance { get; private set; }

        [Header("Volumen")]
        [Tooltip("Volumen general de la música. Bajo a propósito para no tapar los SFX.")]
        [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.18f;
        [SerializeField] private float fadeInSeconds = 0.25f;
        [SerializeField] private float fadeOutSeconds = 0.35f;

        [Header("Pistas por escena")]
        [SerializeField] private List<SceneTrack> tracks = new List<SceneTrack>();

        private AudioSource source;
        private SceneTrack current;
        private Coroutine fadeRoutine;
        private bool muted;
        private bool pausedByGame;
        private Coroutine hurryRoutine;

        public float MusicVolume => musicVolume;
        public bool IsMuted => muted;

        // Crea el manager automáticamente antes de cargar la primera escena.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null) return;
            var prefab = Resources.Load<GameObject>("MusicManager");
            if (prefab == null)
            {
                Debug.LogWarning("[MusicManager] Falta Assets/Resources/MusicManager.prefab. Ejecuta Tools > Abril > Configurar Musica.");
                return;
            }
            Instantiate(prefab);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            source = GetComponent<AudioSource>();
            if (source == null) source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;      // 2D: no depende de la posición de la cámara
            source.priority = 200;         // prioridad baja: si faltan voces, caen primero la música
            source.ignoreListenerPause = true;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
            GameManager.OnGameStateChanged += HandleGameState;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            GameManager.OnGameStateChanged -= HandleGameState;
        }

        private void Start()
        {
            // Solo si el manager se puso a mano en una escena (sceneLoaded ya no llegará).
            if (current == null && !source.isPlaying) PlayForScene(SceneManager.GetActiveScene().name);
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single) PlayForScene(scene.name);
        }

        private void HandleGameState(GameState state)
        {
            if (source == null) return;
            switch (state)
            {
                case GameState.Paused:
                    pausedByGame = true;
                    source.Pause();
                    break;
                case GameState.Playing:
                    pausedByGame = false;
                    if (hurryRoutine == null) source.UnPause();
                    break;
                case GameState.LevelComplete:
                case GameState.GameOver:
                    pausedByGame = false;
                    if (current != null && current.stopWhenLevelEnds) Stop();
                    break;
            }
        }

        private SceneTrack FindTrack(string sceneName)
        {
            foreach (var t in tracks)
                if (t != null && t.clip != null && t.sceneName == sceneName) return t;
            return null;
        }

        /// <summary>Reproduce la pista configurada para esa escena (o silencio si no hay).</summary>
        public void PlayForScene(string sceneName)
        {
            var track = FindTrack(sceneName == "World2_3" ? "Underground" : sceneName);
            if (sceneName == "GameOver" && track != null)
                track.clip = SfxPlayer.GetClip("gameover") ?? track.clip;
            if (track == null) { Stop(); return; }
            Play(track);
        }

        public void Play(SceneTrack track)
        {
            CancelHurry();
            current = track;
            StartFade(StartTrack(track));
        }

        /// <summary>Detiene la música con fade-out.</summary>
        public void Stop()
        {
            CancelHurry();
            current = null;
            StartFade(StopRoutine());
        }

        public void StopImmediate()
        {
            CancelHurry();
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = null;current = null;
            source.Stop();
        }

        private void CancelHurry()
        {
            if (hurryRoutine != null) StopCoroutine(hurryRoutine);
            hurryRoutine = null;
        }

        public void HurryUp()
        {
            if (hurryRoutine != null) return;
            SfxPlayer.Play("hurryup");
            if (current != null) hurryRoutine = StartCoroutine(HurrySequence());
        }

        private IEnumerator HurrySequence()
        {
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = null;source.volume = TargetVolume;
            source.Pause();
            var warning = SfxPlayer.GetClip("hurryup");
            yield return new WaitForSeconds(warning != null ? warning.length : 3f);
            source.pitch = 1.25f;
            if (!pausedByGame) source.UnPause();
            hurryRoutine = null;
        }

        /// <summary>Cambia el volumen general (0-1), por ejemplo desde un menú de opciones.</summary>
        public void SetVolume(float value)
        {
            musicVolume = Mathf.Clamp01(value);
            ApplyVolume();
        }

        public void ToggleMute()
        {
            muted = !muted;
            ApplyVolume();
        }

        private float TargetVolume => muted ? 0f : musicVolume * (current != null ? current.volume : 1f);

        private void ApplyVolume()
        {
            if (source != null && fadeRoutine == null) source.volume = TargetVolume;
        }

        private void StartFade(IEnumerator routine)
        {
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(Wrap(routine));
        }

        private IEnumerator Wrap(IEnumerator inner)
        {
            yield return inner;
            fadeRoutine = null;
            ApplyVolume();
        }

        private IEnumerator StartTrack(SceneTrack track)
        {
            if (source.isPlaying) yield return FadeTo(0f, fadeOutSeconds);
            source.Stop();
            source.clip = track.clip;
            source.pitch = 1f;
            source.loop = track.loop;
            source.volume = 0f;
            source.Play();
            // sceneLoaded can request a track after the level intro has already paused.
            if (pausedByGame) source.Pause();
            yield return FadeTo(TargetVolume, fadeInSeconds);
        }

        private IEnumerator StopRoutine()
        {
            if (source.isPlaying) yield return FadeTo(0f, fadeOutSeconds);
            source.Stop();
        }

        private IEnumerator FadeTo(float target, float seconds)
        {
            float start = source.volume;
            if (seconds <= 0f) { source.volume = target; yield break; }
            for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
            {
                source.volume = Mathf.Lerp(start, target, t / seconds);
                yield return null;
            }
            source.volume = target;
        }
    }
}
