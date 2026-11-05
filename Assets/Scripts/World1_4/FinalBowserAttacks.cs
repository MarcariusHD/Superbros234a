using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarioBros.World1_4
{
    public sealed class FinalBowserAttacks : MonoBehaviour
    {
        public GameObject firePrefab, wavePrefab, pillarPrefab;
        public SpriteRenderer landingMarker;
        public SpriteRenderer[] pillarMarkers = new SpriteRenderer[0];
        private readonly List<GameObject> active = new List<GameObject>();
        private AdventureLevel Level => AdventureLevel.Instance;
        public IEnumerator Pattern(FinalBowser boss, int turn)
        {
            bool jump = boss.Phase >= 2 && turn % 2 == 1;
            if (jump) yield return Jump(boss);
            else yield return Fire(boss);
            if (boss.Phase == 3 && AdventureLevel.Live)
            {
                Level.presentation.Hint("¡EL SUELO ARDE!\nLOS EXTREMOS SON SEGUROS", 2.5f);
                foreach (var marker in pillarMarkers) marker.enabled = true;
                yield return new WaitForSeconds(1);
                foreach (var marker in pillarMarkers) marker.enabled = false;
                Spawn(pillarPrefab, new Vector3(328, 3.2f, 0), Vector2.zero, 1.5f);
                Spawn(pillarPrefab, new Vector3(336, 3.2f, 0), Vector2.zero, 1.5f);
                Level.sound.Fire(transform.position); yield return new WaitForSeconds(1.6f);
            }
        }
        private IEnumerator Fire(FinalBowser boss)
        {
            Level.presentation.Boss(boss.Health, boss.Phase, "¡ALIENTO DE FUEGO!\nOBSERVA LA ALTURA Y SALTA");
            boss.BreathingFire = true;
            float direction = Level.player.transform.position.x < transform.position.x ? -1 : 1; boss.Face(direction);
            Level.sound.Play(Level.sound.fire, .25f);
            yield return new WaitForSeconds(boss.Phase == 3 ? .8f : 1.1f);
            int count = boss.Phase == 1 ? 2 : boss.Phase == 2 ? 3 : 4;
            for (int i = 0; i < count && AdventureLevel.Live; i++)
            {
                Vector3 p = transform.position + new Vector3(direction * 1.2f, 0, 0);
                p.y = i % 2 == 0 ? 3.15f : 4.65f;
                Spawn(firePrefab, p, new Vector2(direction * (4.2f + boss.Phase * .45f), 0), 6);
                Level.sound.Fire(p, true); yield return new WaitForSeconds(boss.Phase == 3 ? .45f : .7f);
            }
            // Se deja recorrer la arena a los proyectiles antes de abrir los interruptores.
            yield return new WaitForSeconds(2.5f);
        }
        private IEnumerator Jump(FinalBowser boss)
        {
            Level.presentation.Boss(boss.Health, boss.Phase, "¡VA A SALTAR!\nALÉJATE DE LA MARCA"); boss.BreathingFire = false;
            Vector3 start = transform.position, end = start;
            end.x = Mathf.Clamp(Level.player.transform.position.x, 327, 335);
            landingMarker.transform.position = new Vector3(end.x, 2.7f, 0); landingMarker.enabled = true;
            yield return new WaitForSeconds(1.1f);
            float t = 0;
            while (t < 1.25f && AdventureLevel.Live)
            {
                t += Time.deltaTime; float f = Mathf.Clamp01(t / 1.25f);
                transform.position = Vector3.Lerp(start, end, f) + Vector3.up * Mathf.Sin(f * Mathf.PI) * 3.2f; yield return null;
            }
            landingMarker.enabled = false;
            if (!AdventureLevel.Live) yield break;
            transform.position = end; Level.view.Shake(.1f); Level.sound.Play(Level.sound.impact, .45f);
            Level.presentation.Hint("¡ONDA DE FUEGO!\nSALTA SOBRE ELLA", 2.5f);
            Spawn(wavePrefab, end + new Vector3(-1, .25f, 0), new Vector2(-4.6f, 0), 5);
            Spawn(wavePrefab, end + new Vector3(1, .25f, 0), new Vector2(4.6f, 0), 5);
            yield return new WaitForSeconds(3.5f);
        }
        private void Spawn(GameObject prefab, Vector3 position, Vector2 velocity, float lifetime)
        {
            var go = Instantiate(prefab, position, Quaternion.identity); go.name = "Ataque de Bowser";
            var projectile = go.GetComponent<FinalBowserProjectile>(); projectile.velocity = velocity; projectile.lifetime = lifetime;
            go.GetComponent<SpriteRenderer>().flipX = velocity.x < 0;
            active.RemoveAll(x => x == null); active.Add(go);
        }
        public void Clear()
        {
            landingMarker.enabled = false;
            foreach (var marker in pillarMarkers) marker.enabled = false;
            foreach (var go in active) if (go != null) Destroy(go);
            active.Clear();
        }
    }
}
