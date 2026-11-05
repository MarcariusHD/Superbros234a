using MarioBros.Player;
using UnityEngine;

namespace MarioBros.World1_4
{
    public sealed class AdventureHazard : MonoBehaviour
    {
        public bool armed = true, lava, disappearOnContact;
        public Vector2 safeShore;
        private void OnTriggerEnter2D(Collider2D other) { Touch(other); }
        private void OnTriggerStay2D(Collider2D other) { Touch(other); }
        private void Touch(Collider2D other)
        {
            if (!armed || !AdventureLevel.Live) return;
            var health = other.GetComponent<PlayerHealth>();
            if (health == null || health.IsDead || health.IsInvincible) return;
            health.TakeDamage();
            if (lava && !health.IsDead)
            {
                var body = other.attachedRigidbody; body.position = safeShore; body.linearVelocity = new Vector2(0, 10);
            }
            if (disappearOnContact) Destroy(gameObject);
        }
    }
}
