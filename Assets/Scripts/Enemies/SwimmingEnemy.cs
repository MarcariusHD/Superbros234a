using UnityEngine;
using MarioBros.Player;
using MarioBros.UI;

namespace MarioBros.Enemies
{
    [RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
    public class SwimmingEnemy : MonoBehaviour
    {
        public bool blooper;
        private Vector3 origin;
        private Transform player;
        private float phase;
        private SpriteRenderer spriteRenderer;
        private Sprite[] frames;
        private Rigidbody2D body;
        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var hitbox = GetComponent<BoxCollider2D>();
            hitbox.isTrigger = true;
            hitbox.size = new Vector2(.8f, blooper ? 1.3f : .8f);
        }
        private void Start()
        {
            origin = transform.position;
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            phase = origin.x;
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (!blooper) frames = new[] { Resources.Load<Sprite>("MarioFrames/CheepCheep1"), Resources.Load<Sprite>("MarioFrames/CheepCheep2") };
        }
        private void FixedUpdate()
        {
            if (player == null || GameManager.CurrentState != GameState.Playing) return;
            if (Mathf.Abs(player.position.x - transform.position.x) > 18) return;
            phase += Time.deltaTime;
            if (frames != null && spriteRenderer != null)
            {
                var frame = frames[(int)(phase * 5) % frames.Length];
                if (frame != null) spriteRenderer.sprite = frame;
            }
            Vector3 next = body.position;
            if (blooper)
            {
                float pulse = Mathf.Sin(phase * 2.5f);
                Vector3 target = player.position + Vector3.up * 1.5f;
                if (pulse > 0) next = Vector3.MoveTowards(next, target, Time.fixedDeltaTime * 1.6f);
                else next += Vector3.down * Time.fixedDeltaTime * .7f;
                next.y = Mathf.Clamp(next.y, 2.3f, 11.5f);
            }
            else
            {
                next += Vector3.left * Time.fixedDeltaTime * 1.7f;
                next.y = origin.y + Mathf.Sin(phase * 1.7f) * .7f;
            }
            body.MovePosition(next);
        }
        private void OnTriggerEnter2D(Collider2D other) => OnTriggerStay2D(other);
        private void OnTriggerStay2D(Collider2D other)
        {
            var health = other.GetComponentInParent<PlayerHealth>();
            if (health == null || health.IsDead || GameManager.CurrentState != GameState.Playing) return;
            if (health.IsInvincible) return;
            health.TakeDamage();
        }
    }
}
