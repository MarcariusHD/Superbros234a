using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace MarioBros.UI
{
    public class ClassicFrontEnd : MonoBehaviour
    {
        private bool starting;
        private int selection;
        private RectTransform menuCursor;
        private void Start()
        {
            Time.timeScale = 1;
            if (GameManager.Instance == null) new GameObject("GameManager").AddComponent<GameManager>();
            GameManager.Instance.LevelCompleted();
            var root = Screen("Title",transform);
            var mario = new GameObject("Title Mario",typeof(RectTransform),typeof(Image));
            mario.transform.SetParent(root,false);
            mario.GetComponent<Image>().sprite=Resources.Load<Sprite>("MarioFrames/Small_Idle");
            Place(mario.GetComponent<RectTransform>(),48,200,16,16);
            Hud(root,"1-1",0,0,400);
            Label(root,"1 JUGADOR",124,148,112,8,8);
            Label(root,"2 JUGADORES",132,164,112,8,8);
            Label(root,"RECORD",96,188,48,8,8);
            var button = new GameObject("Start game",typeof(RectTransform),typeof(Image),typeof(Button));
            button.transform.SetParent(root,false);
            Place(button.GetComponent<RectTransform>(),128,148,192,16);
            button.GetComponent<Image>().color = new Color(0,0,0,0);
            button.GetComponent<Button>().navigation=new Navigation { mode=Navigation.Mode.None };
            button.GetComponent<Button>().onClick.AddListener(()=>{selection=0;Begin();});
            var second=Instantiate(button,root);second.name="Start cooperative game";
            Place(second.GetComponent<RectTransform>(),128,164,192,16);
            second.GetComponent<Button>().onClick.RemoveAllListeners();
            second.GetComponent<Button>().onClick.AddListener(()=>{selection=1;Begin();});
            Label(root,"000000",152,188,48,8,8);
            var cursor=new GameObject("Menu cursor",typeof(RectTransform),typeof(Image));cursor.transform.SetParent(root,false);
            cursor.GetComponent<Image>().sprite=Resources.Load<Sprite>("ClassicUI/Cursor");Place(cursor.GetComponent<RectTransform>(),76,148,8,8);
            menuCursor=cursor.GetComponent<RectTransform>();
            if (EventSystem.current == null)
                new GameObject("Title input",typeof(EventSystem),typeof(InputSystemUIInputModule));
        }
        private void Update()
        {
            var keyboard=Keyboard.current;
            if(keyboard!=null&&(keyboard.upArrowKey.wasPressedThisFrame||keyboard.downArrowKey.wasPressedThisFrame))
            {
                selection=1-selection;Place(menuCursor,76,148+selection*16,8,8);
                Sound.SfxPlayer.Play("beep");
            }
            if (MarioInput.JumpDown || (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)) Begin();
        }
        public void Begin()
        {
            if (starting) return;
            starting = true;
            Player.CoopSession.TwoPlayers=selection==1;
            GameManager.Instance.RestartGame();
        }
        public static GameObject ShowIntro(string world)
        {
            var holder = new GameObject("Level introduction");
            var root = Screen("LevelIntro",holder.transform);
            Hud(root,world,GameManager.Score,GameManager.Coins,GameManager.TimeRemaining);
            Label(root,"WORLD " + world,128,84,180,20,10);
            Label(root,"x " + GameManager.Lives,136,116,48,18,8);
            if(Player.CoopSession.TwoPlayers)
            {
                var luigi=new GameObject("Luigi introduction",typeof(RectTransform),typeof(Image));luigi.transform.SetParent(root,false);
                luigi.GetComponent<Image>().sprite=Resources.Load<Sprite>("CoopFrames/Luigi/Small_Idle");
                Place(luigi.GetComponent<RectTransform>(),88,116,16,16);
            }
            return holder;
        }
        public static GameObject ShowStatus(string name,string world)
        {
            var holder=new GameObject(name+" NES");var root=Screen(name,holder.transform);
            Hud(root,world,GameManager.Score,GameManager.Coins,GameManager.TimeRemaining);
            Label(root,name=="GameOver"?"FIN DEL JUEGO":"TIEMPO AGOTADO",128,132,200,16,8);
            if(name=="GameOver") Label(root,"PRESS START",128,184,180,16,8);
            return holder;
        }
        private static RectTransform Screen(string sprite,Transform parent)
        {
            var canvasObject = new GameObject("Classic screen",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(parent,false);
            var canvas = canvasObject.GetComponent<Canvas>();canvas.renderMode = RenderMode.ScreenSpaceOverlay;canvas.sortingOrder = 2000;
            canvas.pixelPerfect = true;
            var scaler = canvasObject.GetComponent<CanvasScaler>();scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(256,240);scaler.matchWidthOrHeight = 1;
            var background = new GameObject("Black borders",typeof(RectTransform),typeof(Image));background.transform.SetParent(canvas.transform,false);
            var br = background.GetComponent<RectTransform>();br.anchorMin=Vector2.zero;br.anchorMax=Vector2.one;br.sizeDelta=Vector2.zero;
            background.GetComponent<Image>().color = Color.black;
            var frame = new GameObject("NES frame",typeof(RectTransform),typeof(Image));frame.transform.SetParent(canvas.transform,false);
            var rt = frame.GetComponent<RectTransform>();rt.sizeDelta = new Vector2(256,240);
            frame.GetComponent<Image>().sprite = Resources.Load<Sprite>("ClassicUI/"+sprite);
            return rt;
        }
        private static void Hud(Transform root,string world,int score,int coins,int time)
        {
            Label(root,"MARIO\n"+score.ToString("D6"),48,24,88,28,8);
            Label(root,"x"+coins.ToString("D2"),108,28,40,18,8);
            Label(root,"WORLD\n"+world,164,24,60,28,8);
            Label(root,"TIME",220,20,60,8,8);
            var coinIcon=new GameObject("HUD coin",typeof(RectTransform),typeof(Image));coinIcon.transform.SetParent(root,false);
            coinIcon.GetComponent<Image>().sprite=Resources.Load<Sprite>("ClassicUI/Coin");Place(coinIcon.GetComponent<RectTransform>(),92,28,8,8);
        }
        private static void Label(Transform parent,string text,float x,float y,float width,float height,float size)
        {
            var label=NesText.Create(parent,text,x,y,8);
            label.rectTransform.sizeDelta=new Vector2(width,height);
        }
        private static void Place(RectTransform rect,float x,float y,float width,float height)
        {
            rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
            rect.anchoredPosition=new Vector2(x-128,120-y);rect.sizeDelta=new Vector2(width,height);
        }
    }
}
