using UnityEngine;
using UnityEngine.UI;

namespace MarioBros.UI
{
    // Each glyph is an untouched 8x8 cell from the supplied NES sheet.
    public class NesText : MaskableGraphic
    {
        public const string Characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-x!.©";
        private string value = "";
        private Texture2D atlas;
        public float pixelSize = 8;
        public string Text { get => value; set { if (this.value == value) return; this.value = value; SetVerticesDirty(); } }
        public override Texture mainTexture => atlas != null ? atlas : Texture2D.whiteTexture;
        protected override void Awake() { base.Awake(); atlas = Resources.Load<Texture2D>("ClassicUI/Font");raycastTarget = false; }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();if (atlas == null) return;
            string[] lines = value.Split('\n');float lineHeight = pixelSize;
            for (int row = 0; row < lines.Length; row++)
            {
                float x = -lines[row].Length * pixelSize * .5f;
                float top = lines.Length * lineHeight * .5f - row * lineHeight;
                for (int col = 0; col < lines[row].Length; col++)
                {
                    char c = lines[row][col];if (c != 'x') c = char.ToUpperInvariant(c);
                    int glyph = Characters.IndexOf(c);if (glyph < 0) continue;
                    float left = x + col * pixelSize;float u0 = glyph / (float)Characters.Length;float u1 = (glyph + 1f) / Characters.Length;
                    int start = vh.currentVertCount;
                    vh.AddVert(new Vector3(left,top-pixelSize),color,new Vector2(u0,0));
                    vh.AddVert(new Vector3(left,top),color,new Vector2(u0,1));
                    vh.AddVert(new Vector3(left+pixelSize,top),color,new Vector2(u1,1));
                    vh.AddVert(new Vector3(left+pixelSize,top-pixelSize),color,new Vector2(u1,0));
                    vh.AddTriangle(start,start+1,start+2);vh.AddTriangle(start,start+2,start+3);
                }
            }
        }
        public static NesText Create(Transform parent,string text,float x,float y,float size=8)
        {
            var go = new GameObject("NES "+text,typeof(RectTransform));go.transform.SetParent(parent,false);
            var label=go.AddComponent<NesText>();label.pixelSize=size;label.Text=text;
            label.rectTransform.anchorMin=label.rectTransform.anchorMax=new Vector2(.5f,.5f);
            label.rectTransform.sizeDelta=new Vector2(256,40);label.rectTransform.anchoredPosition=new Vector2(x-128,120-y);
            return label;
        }
    }
}
