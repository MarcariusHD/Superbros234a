using MarioBros.Items;
using MarioBros.Player;
using MarioBros.UI;
using UnityEngine;

namespace MarioBros.World1_4
{
    /// <summary>Efectos de esta aventura; ninguna fuente de música se modifica.</summary>
    public sealed class AdventureAudio : MonoBehaviour
    {
        public AudioSource source;
        public AudioClip coin, jumpSmall, jumpBig, stomp, powerup, fire, fireball, impact, mechanism, bowserFall, death, celebration, pipe;
        private PlayerMovement movement;
        private PlayerHealth health;
        private PowerState power;
        private bool grounded, deathHeard;
        private float lastStomp = -1, lastFire = -1;
        private void Awake() { movement = GetComponent<PlayerMovement>(); health = GetComponent<PlayerHealth>(); power = health.CurrentPowerState; }
        private void OnEnable() { Coin.OnCoinCollected += Collected; }
        private void OnDisable() { Coin.OnCoinCollected -= Collected; }
        public void Play(AudioClip clip, float volume = .35f) { if (clip != null && source != null) source.PlayOneShot(clip, Mathf.Clamp01(volume)); }
        private void Collected(Coin item) { if (GameManager.Instance == null || !GameManager.Instance.HasCoinAudio) Play(coin, .3f); }
        public void Fire(Vector3 position, bool projectile = false)
        {
            if (Camera.main != null && Mathf.Abs(Camera.main.transform.position.x - position.x) > 20 || Time.time - lastFire < .16f) return;
            lastFire = Time.time; Play(projectile ? fireball : fire, .28f);
        }
        private void LateUpdate()
        {
            if (health.IsDead)
            {
                if (!deathHeard) { deathHeard = true; Play(death, .5f); }
                return;
            }
            if (!AdventureLevel.Live) return;
            if (grounded && !movement.IsGrounded && movement.IsJumping) Play(health.IsBig ? jumpBig : jumpSmall, .26f);
            if (movement.IsUnderwater && MarioInput.JumpDown) Play(stomp, .18f);
            if ((int)health.CurrentPowerState > (int)power) Play(powerup, .4f);
            grounded = movement.IsGrounded; power = health.CurrentPowerState;
        }
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!AdventureLevel.Live || Time.time - lastStomp < .15f || collision.gameObject.GetComponent<MarioBros.Enemies.EnemyBase>() == null) return;
            foreach (var contact in collision.contacts)
                if (contact.normal.y > .5f) { lastStomp = Time.time; Play(stomp, .3f); break; }
        }
    }
}
