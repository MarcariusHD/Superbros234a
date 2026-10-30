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
                if(!MarioInput.Down||!movement.IsGrounded||Mathf.Abs(other.bounds.center.x-transform.position.x)>.55f||other.bounds.min.y<transform.position.y-.3f)return;
            }
            else if(!MarioInput.Right)return;
            busy=true;StartCoroutine(Travel(other.gameObject));
        }
        private IEnumerator Travel(GameObject player)
        {
            GameManager.Instance.SetState(GameState.Paused);
            var travel=new PipeTransit(player);yield return travel.Enter(transform.position,enterBonus);
            var camera=Camera.main;var follow=camera.GetComponent<CameraFollow>();
            if(enterBonus)
            {
                player.transform.position=new Vector3(243,10,0);
                camera.backgroundColor=Color.black;follow.SetRegion(248,248,248);
            }
            else
            {
                camera.backgroundColor=new Color32(92,148,252,255);follow.SetRegion(8,203,164);
                yield return travel.Emerge(new Vector3(164,4.5f,0));
            }
            travel.Restore();GameManager.Instance.SetState(GameState.Playing);
            yield return new WaitForSeconds(.5f);busy=false;
        }
    }
}
