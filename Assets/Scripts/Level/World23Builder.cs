using UnityEngine;
using UnityEngine.UI;
using MarioBros.Player;
using MarioBros.Blocks;
using MarioBros.CameraControl;
using MarioBros.UI;

namespace MarioBros.Level
{
    // Castle route assembled from the supplied stage sheets.
    [DefaultExecutionOrder(-200)]
    public class World23Builder : MonoBehaviour
    {
        public GameObject goombaPrefab,koopaPrefab,coinPrefab,mushroomPrefab;
        public Sprite groundSprite,brickSprite,questionSprite,emptySprite,pipeSprite,liftSprite;
        public const int Width=192;
        public const int CeilingRow=11;
        public const float PipeMouthY=9;
        private Material material;
        private GameObject outdoorRoot;
        public void RevealOutdoor() { if(outdoorRoot!=null)outdoorRoot.SetActive(true); }
        public static bool IsPit(int x) => (x>=28&&x<=31)||(x>=60&&x<=63)||(x>=94&&x<=108);
        private void Awake()
        {
            var shader=Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")??Shader.Find("Sprites/Default");
            material=new Material(shader);
            groundSprite=Resources.Load<Sprite>("Castle23/Floor");brickSprite=groundSprite;
            var ceiling=Resources.Load<Sprite>("Castle23/Ceiling");
            var column=Resources.Load<Sprite>("Castle23/Column");
            var outdoor=Resources.Load<Sprite>("Castle23/OutdoorGround");
            outdoorRoot=new GameObject("Castle exterior - revealed after pipe");outdoorRoot.transform.SetParent(transform);
            for(int x=0;x<Width;x++)
            {
                if(!IsPit(x))for(int y=0;y<2;y++)
                {
                    var floor=Tile(x>=148?outdoor:groundSprite,x,y,true);
                    if(x>=148)floor.transform.SetParent(outdoorRoot.transform,true);
                }
                if(x<145)Tile(ceiling,x,CeilingRow,true);
            }
            for(int x=15;x<23;x++)Tile(brickSprite,x,6,true,true);
            for(int x=40;x<49;x++)Tile(brickSprite,x,6,true,true);
            for(int x=79;x<89;x++)Tile(brickSprite,x,7,true,true);
            // Four upgrade blocks for two players; the remaining question blocks hold coins.
            foreach(int x in new[]{7,9,113,116})Question(x,5,true);
            foreach(int x in new[]{14,22,40,47,79,85,89,123,128})Question(x,5,false);
            Question(45,9,false);
            foreach(int start in new[]{17,42,81,119})
                for(int x=start;x<start+4;x++)CoinAt(x,3);
            foreach(int start in new[]{34,110})
                for(int step=0;step<3;step++)
                    for(int y=2;y<(step==1?4:3);y++)Tile(column,start+step,y,true);
            for(int x=119;x<123;x++)Tile(brickSprite,x,6,true,true);
            for(int i=0;i<4;i++)for(int y=2;y<3+i;y++)Tile(column,132+i,y,true);
            PipeAt(73);
            // One horizontal ferry, with a continuous lava lake underneath.
            LiftAt(95.5f,3.5f,2);
            Lava(28,31);Lava(60,63);Lava(94,108);
            Enemy(goombaPrefab,14);Enemy(goombaPrefab,17);
            Enemy(koopaPrefab,43);Enemy(goombaPrefab,82);Enemy(koopaPrefab,126);
            PlantAt(74);BubbleAt(30);
            Boundary(-.5f);Boundary(145);Boundary(Width+.5f);
            // The ceiling pipe is entered from underneath while holding Up.
            var inverted=Visual(pipeSprite,new Vector2(138,CeilingRow-1));
            var pipeBounds=pipeSprite.bounds.size;
            inverted.transform.localScale=new Vector3(2/pipeBounds.x,2/pipeBounds.y,1);
            inverted.GetComponent<SpriteRenderer>().flipY=true;
            inverted.layer=LayerMask.NameToLayer("Ground");inverted.AddComponent<BoxCollider2D>().size=pipeBounds;
            var exit=new GameObject("Inverted pipe - up to castle");exit.transform.SetParent(transform);
            exit.transform.position=new Vector3(138,PipeMouthY-.2f,0);
            var trigger=exit.AddComponent<BoxCollider2D>();trigger.isTrigger=true;trigger.size=new Vector2(1.8f,.8f);
            var finish=exit.AddComponent<World23Finish>();finish.mouthY=PipeMouthY;
            inverted.AddComponent<World23PipeContact>().entrance=finish;
            PipeAt(152).transform.SetParent(outdoorRoot.transform,true);
            var castle=Visual(Resources.Load<Sprite>("Castle23/Castle"),new Vector2(168.5f,2));
            castle.GetComponent<SpriteRenderer>().sortingOrder=1;
            castle.transform.SetParent(outdoorRoot.transform,true);outdoorRoot.SetActive(false);
            var mario=CreatePlayer();
            var cameraObject=new GameObject("Main Camera");cameraObject.tag="MainCamera";
            cameraObject.transform.position=new Vector3(8,7.5f,-10);
            var camera=cameraObject.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=7.5f;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;
            cameraObject.AddComponent<AudioListener>();
            var follow=cameraObject.AddComponent<CameraFollow>();follow.Target=mario.transform;
            follow.SetRegion(8,148-camera.orthographicSize*camera.aspect,8);
            var hud=new GameObject("HUD Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(NesHud));
            if(GameManager.Instance==null)new GameObject("GameManager").AddComponent<GameManager>();
        }
        private GameObject Tile(Sprite sprite,int x,int y,bool solid,bool breakable=false)
        {
            var go=Visual(sprite,new Vector2(x+.5f,y+.5f));
            if(solid){go.layer=LayerMask.NameToLayer("Ground");go.AddComponent<BoxCollider2D>().size=Vector2.one;}
            if(breakable)go.AddComponent<BrickBlock>();
            return go;
        }
        private GameObject Visual(Sprite sprite,Vector2 position)
        {
            var go=new GameObject(sprite.name);go.transform.SetParent(transform);go.transform.position=position;
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sortingLayerName="Ground";renderer.sharedMaterial=material;
            return go;
        }
        private void Question(int x,int y,bool powerup)
        {
            var go=Tile(questionSprite,x,y,true);
            go.AddComponent<QuestionBlock>().Configure(coinPrefab,mushroomPrefab,emptySprite,powerup);
        }
        private void CoinAt(int x,int y)
        {
            var coin=Instantiate(coinPrefab,new Vector3(x+.5f,y+.5f,0),Quaternion.identity,transform);
            coin.GetComponent<SpriteRenderer>().sharedMaterial=material;
        }
        private GameObject PipeAt(int x)
        {
            var pipe=Visual(pipeSprite,new Vector2(x+1,3));
            var bounds=pipeSprite.bounds.size;pipe.transform.localScale=new Vector3(2/bounds.x,2/bounds.y,1);
            pipe.layer=LayerMask.NameToLayer("Ground");
            pipe.AddComponent<BoxCollider2D>().size=new Vector2(bounds.x,bounds.y);
            return pipe;
        }
        private void LiftAt(float x,float y,float speed)
        {
            var lift=Visual(liftSprite,new Vector2(x,y));lift.layer=LayerMask.NameToLayer("Ground");
            lift.AddComponent<BoxCollider2D>().size=new Vector2(3,.5f);
            var body=lift.AddComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Kinematic;
            var motion=lift.AddComponent<HorizontalPlatform>();motion.speed=speed;motion.left=95.5f;motion.right=107.5f;
        }
        private void Lava(int left,int right)
        {
            var frames=Resources.LoadAll<Sprite>("Castle23/Lava");
            for(int x=left;x<=right;x++)
            {
                var tile=Visual(frames[0],new Vector2(x+.5f,1.5f));
                tile.AddComponent<CastleLava>().frames=frames;
                Visual(Resources.Load<Sprite>("Castle23/LavaFill"),new Vector2(x+.5f,.5f));
            }
            var go=new GameObject("Lava lake");go.transform.SetParent(transform);
            go.transform.position=new Vector3((left+right+1)*.5f,.8f,0);
            var hitbox=go.AddComponent<BoxCollider2D>();hitbox.isTrigger=true;hitbox.size=new Vector2(right-left+1,2);
            go.AddComponent<CastleHazard>().lava=true;
        }
        private void PlantAt(float x)
        {
            var frames=Resources.LoadAll<Sprite>("Castle23/Plant");
            var plant=Visual(frames[0],new Vector2(x,3));
            plant.GetComponent<SpriteRenderer>().sortingOrder=-1;
            var box=plant.AddComponent<BoxCollider2D>();box.size=new Vector2(.7f,1.1f);box.isTrigger=true;
            plant.AddComponent<Enemies.PiranhaPlant>();plant.AddComponent<CastleLava>().frames=frames;
        }
        private void BubbleAt(float x)
        {
            var bubble=Visual(Resources.Load<Sprite>("Castle23/Bubble"),new Vector2(x,1));
            bubble.GetComponent<SpriteRenderer>().sortingOrder=-2;
            bubble.AddComponent<CircleCollider2D>().radius=.4f;
            bubble.GetComponent<CircleCollider2D>().isTrigger=true;
            bubble.AddComponent<CastleHazard>();bubble.AddComponent<LavaBubble>();
        }
        private void Enemy(GameObject prefab,int x)
        {
            var enemy=Instantiate(prefab,new Vector3(x+.5f,2.5f,0),Quaternion.identity,transform);
            enemy.GetComponent<SpriteRenderer>().sharedMaterial=material;
        }
        private void Boundary(float x)
        {
            var go=new GameObject("Level boundary");go.transform.SetParent(transform);go.transform.position=new Vector3(x,7,0);
            go.layer=LayerMask.NameToLayer("Ground");go.AddComponent<BoxCollider2D>().size=new Vector2(1,30);
        }
        private GameObject CreatePlayer()
        {
            var go=new GameObject("Mario");go.SetActive(false);go.tag="Player";go.layer=LayerMask.NameToLayer("PlayerLayer");
            go.transform.position=new Vector3(3,2.5f,0);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=Resources.Load<Sprite>("MarioFrames/Small_Idle");
            renderer.sortingLayerName="Player";renderer.sharedMaterial=material;
            go.AddComponent<Rigidbody2D>().interpolation=RigidbodyInterpolation2D.Interpolate;
            go.AddComponent<BoxCollider2D>();go.AddComponent<PlayerHealth>().UseCenteredSprites();
            go.AddComponent<PlayerMovement>();go.AddComponent<PlayerAnimation>();go.SetActive(true);return go;
        }
        private void OnDestroy() { if(material!=null)Destroy(material); }
    }
}
