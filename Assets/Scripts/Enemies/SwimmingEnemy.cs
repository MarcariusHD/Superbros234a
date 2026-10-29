using UnityEngine;
using MarioBros.Player;
using MarioBros.UI;

namespace MarioBros.Enemies
{
    public class SwimmingEnemy : MonoBehaviour
    {
        public bool blooper;
        private Vector3 origin;
        private Transform player;
        private float phase;
        private SpriteRenderer spriteRenderer;
        private Sprite[] frames;
        private void Start()
        {
            origin = transform.position;
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            phase = origin.x;
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (!blooper) frames = new[] { Resources.Load<Sprite>("MarioFrames/CheepCheep1"), Resources.Load<Sprite>("MarioFrames/CheepCheep2") };
        }
        private void Update()
        {
            if (player == null || GameManager.CurrentState != GameState.Playing) return;
            if (Mathf.Abs(player.position.x - transform.position.x) > 18) return;
            phase += Time.deltaTime;
            if (frames != null && spriteRenderer != null)
            {
                var frame = frames[(int)(phase * 5) % frames.Length];
                if (frame != null) spriteRenderer.sprite = frame;
            }
            if (blooper)
            {
                float pulse = Mathf.Sin(phase * 2.5f);
                Vector3 target = player.position + Vector3.up * 1.5f;
                if (pulse > 0) transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * 1.6f);
                else transform.position += Vector3.down * Time.deltaTime * 0.7f;
                var pos = transform.position; pos.y = Mathf.Clamp(pos.y, 2.3f, 11.5f); transform.position = pos;
            }
            else
            {
                transform.position += Vector3.left * Time.deltaTime * 1.7f;
                var pos = transform.position; pos.y = origin.y + Mathf.Sin(phase * 1.7f) * 0.7f; transform.position = pos;
            }
        }
        private void OnTriggerStay2D(Collider2D other)
        {
            var health = other.GetComponent<PlayerHealth>();
            if (health == null || health.IsDead || GameManager.CurrentState != GameState.Playing) return;
            if (health.IsInvincible) return;
            health.TakeDamage();
        }
    }
}
