using UnityEngine;
using MarioBros.Enemies;

namespace MarioBros.Player
{
    public class Fireball : MonoBehaviour
    {
        public GameObject Owner { get; private set; }
        private Rigidbody2D body;
        private float direction,clock;
        private SpriteRenderer sprite;
        private Sprite[] frames;
        private bool spent;
        public static void Launch(GameObject owner,int direction)
        {
            var go=new GameObject("Fireball");go.transform.position=owner.transform.position+new Vector3(direction*.6f,.6f,0);
            var shot=go.AddComponent<Fireball>();shot.Owner=owner;shot.direction=direction;
            shot.sprite=go.AddComponent<SpriteRenderer>();shot.sprite.sortingLayerName="Player";shot.sprite.sortingOrder=5;
            shot.frames=Resources.LoadAll<Sprite>("CoopFrames/Fireball");
            if(shot.frames.Length>0)shot.sprite.sprite=shot.frames[0];
            var col=go.AddComponent<CircleCollider2D>();col.radius=.2f;
            shot.body=go.AddComponent<Rigidbody2D>();shot.body.gravityScale=2.5f;shot.body.freezeRotation=true;
            shot.body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
            shot.body.linearVelocity=new Vector2(direction*9,-3);
            foreach(var p in CoopSession.Players)if(p!=null)
                foreach(var pc in p.GetComponentsInChildren<Collider2D>())Physics2D.IgnoreCollision(col,pc);
            foreach(var other in FindObjectsByType<Fireball>(FindObjectsSortMode.None))if(other!=shot)
                Physics2D.IgnoreCollision(col,other.GetComponent<Collider2D>());
            Destroy(go,3f);
        }
        private void Update()
        {
            clock+=Time.deltaTime*12;
            if(frames.Length>0)sprite.sprite=frames[(int)clock%frames.Length];
        }
        private void FixedUpdate()
        {
            body.linearVelocity=new Vector2(direction*9,body.linearVelocity.y);
        }
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if(Hit(collision.collider))return;
            if(collision.gameObject.layer!=LayerMask.NameToLayer("Ground"))return;
            foreach(var contact in collision.contacts)
            {
                if(contact.normal.y>.5f){body.linearVelocity=new Vector2(direction*9,5);return;}
            }
            Destroy(gameObject);
        }
        private void OnTriggerEnter2D(Collider2D other) { Hit(other); }
        private bool Hit(Collider2D other)
        {
            if(spent)return true;
            var enemy=other.GetComponentInParent<EnemyBase>();
            if(enemy!=null&&!enemy.IsAlive)return false;
            var plant=other.GetComponentInParent<PiranhaPlant>();
            var fish=other.GetComponentInParent<SwimmingEnemy>();
            if(enemy==null&&plant==null&&fish==null)return false;
            spent=true;
            if(enemy!=null&&enemy.IsAlive)enemy.Die(new Vector2(direction,0));
            if(plant!=null){plant.Die();Sound.SfxPlayer.Play("kickkill");}
            if(fish!=null){Destroy(fish.gameObject);Sound.SfxPlayer.Play("kickkill");}
            if(UI.GameManager.Instance!=null)UI.GameManager.Instance.AddScore(200);
            Destroy(gameObject);return true;
        }
    }
}
