using MarioBros.Player;
using MarioBros.UI;
using UnityEngine;

namespace MarioBros.Level
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class CoralHazard : MonoBehaviour
    {
        private void Awake()
        {
            var hitbox = GetComponent<BoxCollider2D>();
            hitbox.isTrigger = true;
            hitbox.size = new Vector2(.65f, .95f);
        }
        private void OnTriggerEnter2D(Collider2D other) => Hurt(other);
        private void OnTriggerStay2D(Collider2D other) => Hurt(other);
        private void Hurt(Collider2D other)
        {
            var health = other.GetComponentInParent<PlayerHealth>();
            if (health != null && !health.IsDead && GameManager.CurrentState == GameState.Playing)
                health.TakeDamage();
        }
    }
}
