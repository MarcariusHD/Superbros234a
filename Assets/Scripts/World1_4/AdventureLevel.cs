using System;
using System.Collections.Generic;
using MarioBros.Player;
using MarioBros.UI;
using UnityEngine;

namespace MarioBros.World1_4
{
    public enum AdventureEnvironment { Exterior, Underground, Underwater, Aqueduct, Lava, Bowser, Secret }

    /// <summary>Coordinación local y eventos opcionales para la música de Abril.</summary>
    public sealed class AdventureLevel : MonoBehaviour
    {
        public static AdventureLevel Instance { get; private set; }
        public static event Action<AdventureEnvironment> EnvironmentChanged;
        public static event Action BattleStarted;
        public static event Action AdventureCompleted;
        public PlayerHealth player;
        public AdventureCamera view;
        public AdventurePresentation presentation;
        public AdventureAudio sound;
        public AdventureArena arena;
        public Transform checkpointSpawn;
        public AdventureZone initialZone, checkpointZone;
        public AdventureEnvironment Environment { get; private set; }
        public bool Traveling { get; set; }
        public bool Finished { get; private set; }
        private readonly HashSet<AdventureWater> water = new HashSet<AdventureWater>();
        private AdventureZone zone;
        public static bool Live => Instance != null && !Instance.Traveling && !Instance.Finished &&
            Instance.player != null && !Instance.player.IsDead && GameManager.CurrentState == GameState.Playing;
        private void Awake() { Instance = this; }
        private void Start()
        {
            if (GameManager.Instance == null) new GameObject("Administrador de partida").AddComponent<GameManager>();
            if (AdventureCheckpoint.Reached)
            {
                player.transform.position = checkpointSpawn.position;
                EnterZone(checkpointZone, true);
            }
            else EnterZone(initialZone, true);
            presentation.Announce("MUNDO 1-4\nLA AVENTURA FINAL", 4);
            presentation.Hint("FLECHAS/A-D: MOVER\nESPACIO/Z: SALTAR   X/MAYÚS: CORRER", 8);
        }
        public void EnterZone(AdventureZone next, bool snap = false)
        {
            if (next == null) return;
            bool changed = zone != next;
            zone = next;
            Environment = next.environment;
            view.SetRegion(next.cameraLeft, next.cameraRight, snap);
            if (!changed) return;
            presentation.SetZone(next.title);
            EnvironmentChanged?.Invoke(Environment);
            next.onEntered.Invoke();
            if (!string.IsNullOrEmpty(next.hint)) presentation.Hint(next.hint, 5);
        }
        public void SetWater(AdventureWater source, bool inside)
        {
            if (inside) water.Add(source); else water.Remove(source);
            player.GetComponent<PlayerMovement>().SetUnderwater(water.Count > 0);
        }
        public void ResetWater(bool underwater)
        {
            water.Clear(); player.GetComponent<PlayerMovement>().SetUnderwater(underwater);
        }
        public void StartBattle() { BattleStarted?.Invoke(); }
        public void Complete()
        {
            if (Finished || player.IsDead) return;
            Finished = true; AdventureCheckpoint.Clear();
            GameManager.Instance.AddScore(10000); GameManager.Instance.LevelCompleted();
            var movement = player.GetComponent<PlayerMovement>(); movement.StopMovement(); movement.enabled = false;
            player.GetComponent<Rigidbody2D>().simulated = false;
            sound.Play(sound.celebration, .5f); presentation.Victory(); AdventureCompleted?.Invoke();
        }
        private void OnDestroy() { if (Instance == this) Instance = null; }
    }
}
