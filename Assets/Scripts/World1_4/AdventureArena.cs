using System.Collections;
using MarioBros.Blocks;
using MarioBros.Player;
using UnityEngine;

namespace MarioBros.World1_4
{
    public sealed class AdventureArena : MonoBehaviour
    {
        public FinalBowser boss;
        public AdventureSwitch[] switches;
        public GameObject gate;
        public GameObject[] bridge;
        private bool begun;
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (begun || !AdventureLevel.Live || other.GetComponent<PlayerHealth>() == null) return;
            begun = true; gate.SetActive(true);
            AdventureLevel.Instance.view.FrameArena(); AdventureLevel.Instance.StartBattle(); boss.Begin();
        }
        public void SetSwitches(bool ready) { foreach (var block in switches) block.SetReady(ready); }
        public bool Activate()
        {
            if (!boss.ReceiveBridgeHit()) return false;
            var level = AdventureLevel.Instance; level.sound.Play(level.sound.mechanism, .3f);
            int index = Mathf.Clamp(6 - boss.Health, 0, bridge.Length - 1);
            bridge[index].GetComponent<SpriteRenderer>().color = new Color(1, .7f, .55f);
            Debris(boss.transform.position + Vector3.up);
            return true;
        }
        public void CollapseBridge() { StartCoroutine(Collapse()); }
        private IEnumerator Collapse()
        {
            foreach (var segment in bridge)
            {
                if (segment != null) { Debris(segment.transform.position); segment.SetActive(false); }
                yield return new WaitForSeconds(.075f);
            }
        }
        private static void Debris(Vector3 point)
        {
            var frames = Resources.LoadAll<Sprite>("ClassicEffects/Brick");
            if (frames.Length < 4) return;
            for (int i = 0; i < 4; i++)
            {
                var go = new GameObject("Fragmento del puente — efecto de Marco"); go.transform.position = point;
                go.AddComponent<SpriteRenderer>().sortingLayerName = "Items";
                go.AddComponent<BrickDebris>().Initialize(frames, 0, i);
                var rb = go.AddComponent<Rigidbody2D>(); rb.gravityScale = 2;
                rb.linearVelocity = new Vector2(i % 2 == 0 ? -2 : 2, 3 + i); Destroy(go, 2);
            }
        }
    }
}
