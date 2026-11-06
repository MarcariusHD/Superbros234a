using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using MarioBros.Player;
using MarioBros.UI;

namespace MarioBros.Level
{
    public class World23Finish : MonoBehaviour
    {
        private bool busy,finished;
        public float mouthY=9;
        private void OnTriggerStay2D(Collider2D other) { TryEnter(other,false); }
        public void TryEnter(Collider2D other,bool undersideContact)
        {
            var health=other.GetComponent<PlayerHealth>();
            var body=other.attachedRigidbody;
            bool rising=body!=null&&body.linearVelocity.y>.1f;
            if(busy||health==null||health.IsDead||GameManager.CurrentState!=GameState.Playing||
                Mathf.Abs(other.bounds.center.x-transform.position.x)>.65f||other.bounds.center.y>=mouthY||
                other.bounds.max.y<mouthY-.6f||
                !(undersideContact||rising||PlayerIdentity.InputFor(health).Up))return;
            busy=true;StartCoroutine(Finish(other.gameObject));
        }
        private IEnumerator Finish(GameObject leader)
        {
            CoopSession.BeginTransition();GameManager.Instance.SetState(GameState.Paused);
            if(Sound.MusicManager.Instance!=null)Sound.MusicManager.Instance.StopImmediate();
            var travel=new PipeTransit(leader);yield return travel.EnterUp(transform.position);
            FindFirstObjectByType<World23Builder>()?.RevealOutdoor();
            var camera=Camera.main;camera.backgroundColor=new Color32(92,148,252,255);
            var follow=camera.GetComponent<CameraControl.CameraFollow>();follow.Target=leader.transform;
            // Frame one tile of ground behind the exit pipe at the left edge.
            float initialCenter=152-1+camera.orthographicSize*camera.aspect;
            follow.SetRegion(initialCenter,Mathf.Max(initialCenter,168),initialCenter);
            yield return travel.Emerge(new Vector3(153,4.5f,0));travel.Restore();
            foreach(var player in CoopSession.Players)
            {
                if(player==null)continue;
                player.enabled=false;player.GetComponent<Rigidbody2D>().simulated=false;
                player.GetComponent<PlayerAnimation>().SetScriptedPose("walk");
            }
            GameManager.Instance.LevelCompleted();Time.timeScale=1;
            float start=leader.transform.position.x;
            for(float x=start;x<168.5f;x+=3*Time.deltaTime)
            {
                float y=WalkHeight(x);
                leader.transform.position=new Vector3(x,y,0);
                foreach(var player in CoopSession.Players)
                    if(player!=null&&player.gameObject!=leader)player.transform.position=new Vector3(x-.8f,WalkHeight(x-.8f),0);
                yield return null;
            }
            foreach(var player in CoopSession.Players)if(player!=null)player.GetComponent<SpriteRenderer>().enabled=false;
            for(int i=0;i<6;i++)
            {
                StartCoroutine(Burst(new Vector3(164+(i%3)*3,8+(i%2)*2,0)));
                yield return new WaitForSeconds(.55f);
            }
            yield return new WaitForSeconds(.7f);ShowComplete();finished=true;
        }
        private IEnumerator Burst(Vector3 position)
        {
            Sound.SfxPlayer.Play("billfirework");GameManager.Instance.AddScore(500);
            var go=new GameObject("Castle firework");go.transform.position=position;go.transform.localScale=Vector3.one*2;
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sortingLayerName="Player";renderer.sortingOrder=20;
            var frames=Resources.LoadAll<Sprite>("Castle23/Firework");
            foreach(var frame in frames){renderer.sprite=frame;yield return new WaitForSeconds(.12f);}
            Destroy(go);
        }
        private static float WalkHeight(float x) => x<154.5f?4.5f:Mathf.Max(2.5f,4.5f-(x-154.5f)*3);
        private static void ShowComplete()
        {
            var go=new GameObject("Victory",typeof(Canvas),typeof(CanvasScaler));
            var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=3000;
            var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(256,240);scaler.matchWidthOrHeight=1;
            var panel=new GameObject("Victory panel",typeof(RectTransform),typeof(Image));panel.transform.SetParent(go.transform,false);
            var rect=panel.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.sizeDelta=Vector2.zero;
            panel.GetComponent<Image>().color=new Color(0,0,0,.85f);
            var frame=new GameObject("NES victory",typeof(RectTransform)).GetComponent<RectTransform>();frame.SetParent(go.transform,false);frame.sizeDelta=new Vector2(256,240);
            NesText.Create(frame,"NIVEL 2-3 COMPLETADO",128,40);
            NesText.Create(frame,"INTEGRANTES",128,76);
            NesText.Create(frame,"Marco Calderon",128,100);
            NesText.Create(frame,"Abril Cayllagua",128,116);
            NesText.Create(frame,"Edson Chuquimia",128,132);
            NesText.Create(frame,"Limber Soto",128,148);
            var star=new GameObject("Estrella de Marco Calderon",typeof(RectTransform),typeof(Image));
            star.transform.SetParent(frame,false);star.GetComponent<Image>().sprite=Resources.Load<Sprite>("Castle23/CreditsStar");
            star.GetComponent<Image>().raycastTarget=false;
            var starRect=star.GetComponent<RectTransform>();starRect.sizeDelta=new Vector2(12,12);
            starRect.anchoredPosition=new Vector2(56-128,120-100);
            NesText.Create(frame,"PUNTOS "+GameManager.Score.ToString("D6"),128,180);
            NesText.Create(frame,"ENTER PARA VOLVER AL MENU",128,212,7);
        }
        private void Update()
        {
            if(finished&&Keyboard.current!=null&&Keyboard.current.enterKey.wasPressedThisFrame)SceneManager.LoadScene("TitleScreen");
        }
    }
    public class World23PipeContact : MonoBehaviour
    {
        public World23Finish entrance;
        private void OnCollisionEnter2D(Collision2D collision) { Check(collision); }
        private void OnCollisionStay2D(Collision2D collision) { Check(collision); }
        private void Check(Collision2D collision)
        {
            if(entrance!=null&&collision.collider.bounds.center.y<entrance.mouthY&&
                collision.collider.bounds.max.y<=entrance.mouthY+.15f)
                entrance.TryEnter(collision.collider,true);
        }
    }
}
