using UnityEngine;

namespace MarioBros.World1_4
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class FinalBowserProjectile : MonoBehaviour
    {
        public Vector2 velocity;
        public float lifetime = 6;
        private Rigidbody2D body;
        private void Awake() { body = GetComponent<Rigidbody2D>(); }
        private void Start() { Destroy(gameObject, lifetime); }
        private void FixedUpdate()
        {
            var level = AdventureLevel.Instance;
            if (level == null || level.player.IsDead || level.Finished) { Destroy(gameObject); return; }
            body.MovePosition(body.position + velocity * Time.fixedDeltaTime);
        }
    }
}
