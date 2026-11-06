using System.Collections;
using MarioBros.Player;
using MarioBros.UI;
using MarioBros.CameraControl;
using UnityEngine;

namespace MarioBros.Level
{
    public class BonusPipe : MonoBehaviour
    {
        public bool enterBonus;
        private bool busy;
        private void OnTriggerStay2D(Collider2D other)
        {
            var health=other.GetComponent<PlayerHealth>();var movement=other.GetComponent<PlayerMovement>();
            if(busy||health==null||health.IsDead||GameManager.CurrentState!=GameState.Playing)return;
            if(enterBonus)
            {
                if(!PlayerIdentity.InputFor(health).Down||!movement.IsGrounded||Mathf.Abs(other.bounds.center.x-transform.position.x)>.55f||other.bounds.min.y<transform.position.y-.3f)return;
            }
            else if(!PlayerIdentity.InputFor(health).Right)return;
            busy=true;StartCoroutine(Travel(other.gameObject));
        }
        private IEnumerator Travel(GameObject player)
        {
            GameManager.Instance.SetState(GameState.Paused);
            CoopSession.BeginTransition();
            var travel=new PipeTransit(player);yield return travel.Enter(transform.position,enterBonus);
            var camera=Camera.main;var follow=camera.GetComponent<CameraFollow>();
            follow.Target=player.transform;
            if(enterBonus)
            {
                player.transform.position=new Vector3(243,10,0);
                camera.backgroundColor=Color.black;follow.SetRegion(248,248,248);
                if(Sound.MusicManager.Instance!=null)Sound.MusicManager.Instance.PlayForScene("Underground");
            }
            else
            {
                camera.backgroundColor=new Color32(92,148,252,255);follow.SetRegion(8,203,164);
                if(Sound.MusicManager.Instance!=null)Sound.MusicManager.Instance.PlayForScene("SampleScene");
                yield return travel.Emerge(new Vector3(164,4.5f,0));
            }
            travel.Restore();CoopSession.Regroup(player);GameManager.Instance.SetState(GameState.Playing);
            yield return new WaitForSeconds(.5f);busy=false;
        }
    }
}
