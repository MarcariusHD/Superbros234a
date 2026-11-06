using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MarioBros.Player
{
    public static class CoopSession
    {
        public static bool TwoPlayers { get; set; }
        public static readonly List<PlayerMovement> Players = new List<PlayerMovement>();
        public static bool Transitioning { get; private set; }
        private static readonly PowerState[] powers=new PowerState[2];
        public static void ResetPowers() { powers[0]=powers[1]=PowerState.Small; }
        public static bool CanSpawnFlower(PlayerHealth hitter)
        {
            if(!TwoPlayers)return hitter!=null&&hitter.IsBig;
            int count=0;
            foreach(var player in Players)
            {
                if(player==null)return false;
                var health=player.GetComponent<PlayerHealth>();
                if(health==null||health.IsDead||!health.IsBig)return false;
                count++;
            }
            return count==2;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { TwoPlayers=false;Players.Clear();Transitioning=false;ResetPowers(); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap() { SceneManager.sceneLoaded -= Loaded;SceneManager.sceneLoaded += Loaded; }
        private static void Loaded(Scene scene, LoadSceneMode mode)
        {
            if(mode != LoadSceneMode.Single)return;
            Players.Clear();Transitioning=false;
            if(scene.name=="SampleScene")Level.BonusRoomBuilder.Build();
            if(scene.name!="SampleScene"&&scene.name!="Underground"&&scene.name!="Underwater"&&scene.name!="World2_3")return;
            var mario=Object.FindFirstObjectByType<PlayerMovement>();if(mario==null)return;
            var id=mario.GetComponent<PlayerIdentity>()??mario.gameObject.AddComponent<PlayerIdentity>();id.slot=0;
            mario.name="Mario";Players.Add(mario);
            mario.GetComponent<PlayerHealth>().SetPowerState(powers[0]);
            if(mario.GetComponent<PlayerFire>()==null)mario.gameObject.AddComponent<PlayerFire>();
            if(!TwoPlayers)return;
            var luigi=Object.Instantiate(mario.gameObject,mario.transform.position+Vector3.right*1.2f,Quaternion.identity);
            luigi.name="Luigi";luigi.GetComponent<PlayerIdentity>().slot=1;
            luigi.GetComponent<PlayerHealth>().SetPowerState(powers[1]);
            Players.Add(luigi.GetComponent<PlayerMovement>());
            foreach(var a in mario.GetComponentsInChildren<Collider2D>())
            foreach(var b in luigi.GetComponentsInChildren<Collider2D>()) Physics2D.IgnoreCollision(a,b);
        }
        public static void BeginTransition()
        {
            Transitioning=true;
            foreach(var player in Players)if(player!=null)
                powers[player.GetComponent<PlayerIdentity>().slot]=player.GetComponent<PlayerHealth>().CurrentPowerState;
            foreach(var player in Players) if(player!=null) {player.StopMovement();player.enabled=false;}
        }
        public static void Regroup(GameObject leader)
        {
            foreach(var player in Players)
            {
                if(player==null)continue;
                if(player.gameObject!=leader)
                {
                    var body=player.GetComponent<Rigidbody2D>();body.position=(Vector2)leader.transform.position+Vector2.right*.8f;
                    body.linearVelocity=Vector2.zero;
                }
                player.enabled=true;
            }
            Transitioning=false;
        }
        public static float RearX(float fallback)
        {
            float result=float.PositiveInfinity;
            foreach(var p in Players) if(p!=null) result=Mathf.Min(result,p.transform.position.x);
            return float.IsPositiveInfinity(result)?fallback:result;
        }
    }
}
