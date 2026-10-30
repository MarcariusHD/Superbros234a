using System.Collections;
using MarioBros.Player;
using UnityEngine;

namespace MarioBros.Level
{
    public sealed class PipeTransit
    {
        private readonly GameObject player;
        private readonly PlayerMovement movement;
        private readonly PlayerAnimation animation;
        private readonly Rigidbody2D body;
        private readonly SpriteRenderer sprite;
        private readonly int layer, order;
        public PipeTransit(GameObject player)
        {
            this.player=player;movement=player.GetComponent<PlayerMovement>();body=player.GetComponent<Rigidbody2D>();
            animation=player.GetComponent<PlayerAnimation>();sprite=player.GetComponent<SpriteRenderer>();
            layer=sprite.sortingLayerID;order=sprite.sortingOrder;
            movement.StopMovement();movement.enabled=false;body.simulated=false;
            sprite.sortingLayerName="Ground";sprite.sortingOrder=-1;
        }
        public IEnumerator Enter(Vector3 mouth,bool down)
        {
            if(down) player.transform.position=new Vector3(mouth.x,player.transform.position.y,0);
            animation.SetScriptedPose(down?"pipe":"walk");
            float distance=down?sprite.bounds.size.y+.25f:sprite.bounds.size.x+1;
            yield return Slide(player.transform.position,player.transform.position+(down?Vector3.down:Vector3.right)*distance);
        }
        public IEnumerator Emerge(Vector3 finalPosition)
        {
            animation.SetScriptedPose("pipe");
            yield return Slide(finalPosition-Vector3.up*(sprite.bounds.size.y+.25f),finalPosition);
        }
        private IEnumerator Slide(Vector3 start,Vector3 end)
        {
            for(float elapsed=0;elapsed<.8f;elapsed+=Time.unscaledDeltaTime)
            { player.transform.position=Vector3.Lerp(start,end,Mathf.Clamp01(elapsed/.8f));yield return null; }
            player.transform.position=end;
        }
        public void Restore()
        {
            sprite.sortingLayerID=layer;sprite.sortingOrder=order;animation.SetScriptedPose(null);
            body.simulated=true;body.linearVelocity=Vector2.zero;movement.enabled=true;
        }
    }
}
