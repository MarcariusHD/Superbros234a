using System.Collections.Generic;
using UnityEngine;

namespace MarioBros.Sound
{
    // A persistent 2D source keeps effects audible regardless of camera position.
    public sealed class SfxPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip[] clips;
        private static SfxPlayer instance;
        private AudioSource source;
        private readonly Dictionary<string, AudioClip> sounds = new Dictionary<string, AudioClip>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap() { EnsureInstance(); }
        private static void EnsureInstance()
        {
            if (instance != null) return;
            var prefab = Resources.Load<GameObject>("SfxPlayer");
            if (prefab != null) Instantiate(prefab);
        }
        private void Awake()
        {
            if (instance != null && instance != this) { Destroy(gameObject); return; }
            instance = this;DontDestroyOnLoad(gameObject);
            source = gameObject.AddComponent<AudioSource>();source.playOnAwake = false;
            source.spatialBlend = 0;source.volume = .8f;source.priority = 32;
            foreach (var clip in clips) if (clip != null) sounds[clip.name] = clip;
        }
        public static AudioClip GetClip(string name)
        {
            EnsureInstance();
            return instance != null && instance.sounds.TryGetValue(name, out var clip) ? clip : null;
        }
        public static void Play(string name, AudioClip custom = null)
        {
            var clip = custom != null ? custom : GetClip(name);
            EnsureInstance();
            if (instance != null && clip != null) instance.source.PlayOneShot(clip);
        }
    }
}
