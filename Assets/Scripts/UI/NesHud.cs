using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace MarioBros.UI
{
    [DisallowMultipleComponent]
    public class NesHud : MonoBehaviour
    {
        private NesText score,coins,world,time,lives;
        private void Start()
        {
            var canvas = gameObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=gameObject.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(320,240);scaler.matchWidthOrHeight=1;canvas.pixelPerfect=true;
            var root=new GameObject("NES HUD",typeof(RectTransform)).GetComponent<RectTransform>();root.SetParent(transform,false);root.sizeDelta=new Vector2(320,240);
            score=NesText.Create(root,"",0,24);coins=NesText.Create(root,"",64,24);
            world=NesText.Create(root,"",128,24);time=NesText.Create(root,"",192,24);
            lives=NesText.Create(root,"",256,24);
            var icon=new GameObject("Coin",typeof(RectTransform),typeof(Image));icon.transform.SetParent(root,false);
            icon.GetComponent<Image>().sprite=Resources.Load<Sprite>("ClassicUI/Coin");
            var rt=icon.GetComponent<RectTransform>();rt.sizeDelta=new Vector2(8,8);rt.anchoredPosition=new Vector2(76-160,120-28);
            Refresh();
        }
        private void Update() => Refresh();
        private void Refresh()
        {
            if (score==null) return;
            string name=SceneManager.GetActiveScene().name;
            score.Text="MARIO\n"+GameManager.Score.ToString("D6");coins.Text="COINS\nx"+GameManager.Coins.ToString("D2");
            lives.Text="LIVES\nx "+GameManager.Lives;
            world.Text="WORLD\n"+(name=="Underground"?"1-2":name=="Underwater"?"2-2":name=="World2_3"?"2-3":"1-1");time.Text="TIME\n"+GameManager.TimeRemaining.ToString("D3");
        }
    }
}
