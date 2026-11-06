using UnityEngine;
using MarioBros.Player;
using MarioBros.UI;

namespace MarioBros.Level
{
    public class CastleHazard : MonoBehaviour
    {
        public bool lava;
        private void OnTriggerEnter2D(Collider2D other) { Hurt(other); }
        private void OnTriggerStay2D(Collider2D other) { Hurt(other); }
        private void Hurt(Collider2D other)
        {
            if(GameManager.CurrentState!=GameState.Playing||CoopSession.Transitioning)return;
            var health=other.GetComponentInParent<PlayerHealth>();if(health==null||health.IsDead)return;
            if(lava)health.Die();else health.TakeDamage();
        }
    }
    public class CastleLava : MonoBehaviour
    {
        public Sprite[] frames;
        private SpriteRenderer sprite;
        private void Awake() { sprite=GetComponent<SpriteRenderer>(); }
        private void Update() { if(frames!=null&&frames.Length>0)sprite.sprite=frames[(int)(Time.time*4)%frames.Length]; }
    }
    public class LavaBubble : MonoBehaviour
    {
        private Vector3 origin;
        private Collider2D hitbox;
        private SpriteRenderer sprite;
        private float phase;
        private void Start()
        {
            origin=transform.position;hitbox=GetComponent<Collider2D>();sprite=GetComponent<SpriteRenderer>();
            sprite.sortingOrder=-2;sprite.enabled=false;hitbox.enabled=false;phase=-Mathf.PI*.5f;
        }
        private void Update()
        {
            if(GameManager.CurrentState!=GameState.Playing)return;
            phase+=Time.deltaTime*1.6f;float jump=Mathf.Sin(phase);
            transform.position=origin+Vector3.up*Mathf.Max(0,jump)*4.5f;
            sprite.enabled=transform.position.y+sprite.bounds.extents.y>2;
            hitbox.enabled=sprite.enabled&&jump>0;
            sprite.flipY=Mathf.Cos(phase)<0;
        }
    }
}
