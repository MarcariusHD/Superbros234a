using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace MarioBros.UI
{
    public class NesHud : MonoBehaviour
    {
        private NesText score,coins,world,time;
        private void Start()
        {
            var canvas = gameObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=gameObject.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(256,240);scaler.matchWidthOrHeight=1;
            var root=new GameObject("NES HUD",typeof(RectTransform)).GetComponent<RectTransform>();root.SetParent(transform,false);root.sizeDelta=new Vector2(256,240);
            score=NesText.Create(root,"",48,24);coins=NesText.Create(root,"",108,28);
            world=NesText.Create(root,"",164,24);time=NesText.Create(root,"",220,24);
            var icon=new GameObject("Coin",typeof(RectTransform),typeof(Image));icon.transform.SetParent(root,false);
            icon.GetComponent<Image>().sprite=Resources.Load<Sprite>("ClassicUI/Coin");
            var rt=icon.GetComponent<RectTransform>();rt.sizeDelta=new Vector2(8,8);rt.anchoredPosition=new Vector2(92-128,120-28);
            Refresh();
        }
        private void Update() => Refresh();
        private void Refresh()
        {
            if (score==null) return;
            string name=SceneManager.GetActiveScene().name;
            score.Text="MARIO\n"+GameManager.Score.ToString("D6");coins.Text="x"+GameManager.Coins.ToString("D2");
            world.Text="WORLD\n"+(name=="Underground"?"1-2":name=="Underwater"?"2-2":"1-1");time.Text="TIME\n"+GameManager.TimeRemaining.ToString("D3");
        }
    }
}
