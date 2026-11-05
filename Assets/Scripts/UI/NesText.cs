using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace MarioBros.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class NesText : MonoBehaviour
    {
        public const string Characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-x!.©";
        private string value;
        private Texture atlas;
        private readonly List<RawImage> glyphs = new List<RawImage>();
        public float pixelSize = 8;
        public RectTransform rectTransform => (RectTransform)transform;
        public string Text { get => value; set { if(this.value==value)return;this.value=value??"";Refresh(); } }
        private void Awake()
        {
            var sprite=Resources.Load<Sprite>("ClassicUI/Font");
            atlas=sprite!=null?sprite.texture:Resources.Load<Texture2D>("ClassicUI/Font");
        }
        private void Refresh()
        {
            if(atlas==null){Debug.LogError("Missing NES font: Resources/ClassicUI/Font",this);return;}
            var lines=value.Split('\n');int used=0;
            for(int row=0;row<lines.Length;row++) for(int col=0;col<lines[row].Length;col++)
            {
                char c=lines[row][col];if(c!='x')c=char.ToUpperInvariant(c);
                int index=Characters.IndexOf(c);if(index<0)continue;
                if(used==glyphs.Count)
                {
                    var go=new GameObject("NES glyph",typeof(RectTransform),typeof(RawImage));go.transform.SetParent(transform,false);
                    var image=go.GetComponent<RawImage>();image.texture=atlas;image.color=Color.white;image.raycastTarget=false;glyphs.Add(image);
                }
                var glyph=glyphs[used++];glyph.gameObject.SetActive(true);
                glyph.uvRect=new Rect(index/(float)Characters.Length,0,1f/Characters.Length,1);
                glyph.rectTransform.sizeDelta=Vector2.one*pixelSize;
                glyph.rectTransform.anchoredPosition=new Vector2((col+.5f-lines[row].Length*.5f)*pixelSize,(lines.Length*.5f-row-.5f)*pixelSize);
            }
            for(int i=used;i<glyphs.Count;i++)glyphs[i].gameObject.SetActive(false);
        }
        public static NesText Create(Transform parent,string text,float x,float y,float size=8)
        {
            var go=new GameObject("NES "+text,typeof(RectTransform));go.transform.SetParent(parent,false);
            var label=go.AddComponent<NesText>();label.pixelSize=size;label.rectTransform.sizeDelta=new Vector2(256,40);
            label.rectTransform.anchoredPosition=new Vector2(x-128,120-y);label.Text=text;return label;
        }
    }
}
