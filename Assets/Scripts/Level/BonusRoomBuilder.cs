using UnityEngine;
using MarioBros.Items;

namespace MarioBros.Level
{
    public static class BonusRoomBuilder
    {
        private const string RoomName="Bonus room - original 1-1";
        public static bool IsCoin(int x,int y) => ((y==5||y==7)&&x>=4&&x<=10)||(y==9&&x>=5&&x<=9);
        public static bool IsSolid(int x,int y) => y<2||(x==0&&y>=2)||(y==12&&x>=4&&x<=10)||
            (x>=4&&x<=10&&y>=2&&y<=4)||x==15||(x>=13&&y>=2&&y<=3);
        public static void Build(Transform parent=null)
        {
            if(GameObject.Find(RoomName)!=null)return;
            var root=new GameObject(RoomName);root.transform.SetParent(parent);
            for(int y=0;y<13;y++)for(int x=0;x<16;x++)
            {
                if(!IsSolid(x,y)&&!IsCoin(x,y))continue;
                var sprite=Resources.Load<Sprite>("BonusRoom/Tile_"+x+"_"+y);
                if(sprite==null){Debug.LogError("Falta el recorte de la sala bonus: "+x+","+y);continue;}
                var go=new GameObject("Bonus tile "+x+","+y);go.transform.SetParent(root.transform);
                go.transform.position=new Vector3(240+x+.5f,y+.5f,0);
                var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sortingLayerName="Ground";
                if(IsCoin(x,y))
                {
                    go.AddComponent<CircleCollider2D>().radius=.35f;go.AddComponent<Coin>();
                    renderer.sortingOrder=1;
                }
                else
                {
                    go.layer=LayerMask.NameToLayer("Ground");go.AddComponent<BoxCollider2D>().size=Vector2.one;
                }
            }
            Portal(root.transform,"Bonus entry A - fourth pipe",new Vector2(58,6.1f),new Vector2(1.6f,.4f),true);
            Portal(root.transform,"Bonus exit B - horizontal pipe",new Vector2(252.8f,3),new Vector2(.5f,1.8f),false);
        }
        private static void Portal(Transform parent,string name,Vector2 position,Vector2 size,bool entry)
        {
            var go=new GameObject(name);go.transform.SetParent(parent);go.transform.position=position;
            var trigger=go.AddComponent<BoxCollider2D>();trigger.isTrigger=true;trigger.size=size;
            go.AddComponent<BonusPipe>().enterBonus=entry;
        }
    }
}
