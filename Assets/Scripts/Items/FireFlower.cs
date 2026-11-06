using System.Collections;
using UnityEngine;
using MarioBros.Player;

namespace MarioBros.Items
{
    public class FireFlower : MonoBehaviour
    {
        private bool ready,collected;
        private SpriteRenderer sprite;
        private Sprite[] frames;
        private float clock;
        public static void Spawn(Vector3 origin)
        {
            var go=new GameObject("Fire Flower");go.transform.position=origin;
            var flower=go.AddComponent<FireFlower>();
            flower.sprite=go.AddComponent<SpriteRenderer>();flower.sprite.sortingLayerName="Items";
            flower.frames=Resources.LoadAll<Sprite>("CoopFrames/Flower");
            if(flower.frames.Length>0)flower.sprite.sprite=flower.frames[0];
            go.AddComponent<BoxCollider2D>().isTrigger=true;
            flower.StartCoroutine(flower.Emerge(origin));
        }
        private IEnumerator Emerge(Vector3 origin)
        {
            Sound.SfxPlayer.Play("item");
            for(float t=0;t<.6f;t+=Time.deltaTime){transform.position=origin+Vector3.up*Mathf.Clamp01(t/.6f);yield return null;}
            transform.position=origin+Vector3.up;ready=true;
        }
        private void Update()
        {
            clock+=Time.deltaTime*8;
            if(frames.Length>0)sprite.sprite=frames[(int)clock%frames.Length];
        }
        private void OnTriggerStay2D(Collider2D other)
        {
            if(!ready||collected||UI.GameManager.CurrentState!=UI.GameState.Playing)return;
            var health=other.GetComponentInParent<PlayerHealth>();if(health==null||health.IsDead)return;
            collected=true;health.PowerUp(PowerState.Fire);
            if(UI.GameManager.Instance!=null)UI.GameManager.Instance.AddScore(1000);
            Destroy(gameObject);
        }
    }
}
