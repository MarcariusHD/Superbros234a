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
        private BoxCollider2D solid;
        private float swimDirection = -1;
        private readonly RaycastHit2D[] hits = new RaycastHit2D[8];
        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.freezeRotation = true;
            body.gravityScale = 0;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            solid = GetComponent<BoxCollider2D>();
            solid.isTrigger = false;
            solid.size = new Vector2(.8f, blooper ? 1.3f : .8f);
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
            if (player == null || GameManager.CurrentState != GameState.Playing || Mathf.Abs(player.position.x - transform.position.x) > 18)
            { body.linearVelocity = Vector2.zero; return; }
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
                next += Vector3.right * swimDirection * Time.fixedDeltaTime * 1.7f;
                next.y = origin.y + Mathf.Sin(phase * 1.7f) * .7f;
            }
            Vector2 delta = (Vector2)next - body.position;
            var filter = new ContactFilter2D();filter.SetLayerMask(LayerMask.GetMask("Ground"));filter.useTriggers = false;
            int count = solid.Cast(delta.normalized, filter, hits, delta.magnitude + .02f);
            float travel = delta.magnitude;
            bool turn = false;
            for (int i = 0; i < count; i++)
            {
                if (Vector2.Dot(hits[i].normal, delta) >= 0) continue;
                travel = Mathf.Min(travel, Mathf.Max(0, hits[i].distance - .02f));
                if (Mathf.Abs(hits[i].normal.x) > .5f) turn = true;
            }
            body.linearVelocity = delta.normalized * travel / Time.fixedDeltaTime;
            if (turn) swimDirection = -swimDirection;
            if (!blooper) spriteRenderer.flipX = swimDirection > 0;
        }
        private void OnCollisionEnter2D(Collision2D collision) => Hurt(collision.collider);
        private void OnCollisionStay2D(Collision2D collision) => Hurt(collision.collider);
        private void OnTriggerEnter2D(Collider2D other) => OnTriggerStay2D(other);
        private void OnTriggerStay2D(Collider2D other)
        { Hurt(other); }
        private void Hurt(Collider2D other)
        {
            var health = other.GetComponentInParent<PlayerHealth>();
            if (health == null || health.IsDead || GameManager.CurrentState != GameState.Playing) return;
            if (health.IsInvincible) return;
            health.TakeDamage();
        }
    }
}
