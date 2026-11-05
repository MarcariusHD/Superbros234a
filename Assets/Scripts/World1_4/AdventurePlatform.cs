using MarioBros.Player;
using UnityEngine;

namespace MarioBros.World1_4
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class AdventurePlatform : MonoBehaviour
    {
        public Vector2 travel;
        public float period = 4.5f, phase;
        private Rigidbody2D body;
        private Vector2 origin;
        private void Awake() { body = GetComponent<Rigidbody2D>(); origin = body.position; }
        private void FixedUpdate() { body.MovePosition(origin + travel * (.5f * Mathf.Sin(Time.fixedTime * Mathf.PI * 2 / period + phase))); }
        private void OnCollisionStay2D(Collision2D collision)
        {
            var mario = collision.gameObject.GetComponent<PlayerMovement>();
            if (mario == null || !mario.IsGrounded || collision.collider.bounds.min.y < GetComponent<Collider2D>().bounds.max.y - .2f) return;
            if (collision.rigidbody != null && collision.rigidbody.linearVelocity.y < body.linearVelocity.y + .6f)
                collision.rigidbody.position += new Vector2(body.linearVelocity.x * Time.fixedDeltaTime, 0);
        }
    }
}
