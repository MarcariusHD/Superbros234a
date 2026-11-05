using MarioBros.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MarioBros.World1_4
{
    public sealed class AdventureCheckpoint : MonoBehaviour
    {
        public static bool Reached { get; private set; }
        public SpriteRenderer flag;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void NewSession()
        {
            Reached = false; SceneManager.sceneLoaded -= Loaded; SceneManager.sceneLoaded += Loaded;
        }
        private static void Loaded(Scene scene, LoadSceneMode mode) { if (scene.name != "World1_4") Clear(); }
        public static void Clear() { Reached = false; }
        private void Start() { if (Reached) flag.color = new Color(1, 1, .5f); }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (Reached || !AdventureLevel.Live || other.GetComponent<PlayerHealth>() == null) return;
            Reached = true; flag.color = new Color(1, 1, .5f);
            AdventureLevel.Instance.sound.Play(AdventureLevel.Instance.sound.powerup, .4f);
            AdventureLevel.Instance.presentation.Announce("PUNTO DE CONTROL\n¡YA CASI ESTÁS!", 3);
        }
    }
}
