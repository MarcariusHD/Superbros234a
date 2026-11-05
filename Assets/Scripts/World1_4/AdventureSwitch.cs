using System.Collections;
using MarioBros.Player;
using UnityEngine;

namespace MarioBros.World1_4
{
    /// <summary>El salto y golpe desde abajo del bloque de siempre accionan el puente del jefe.</summary>
    public sealed class AdventureSwitch : MonoBehaviour
    {
        public AdventureArena arena;
        public SpriteRenderer image;
        public Sprite readySprite, restingSprite;
        public GameObject signal;
        private bool ready;
        private Vector3 origin;
        private void Start() { origin = transform.position; SetReady(false); }
        public void SetReady(bool value) { ready = value; image.sprite = value ? readySprite : restingSprite; signal.SetActive(value); }
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!ready || !AdventureLevel.Live || collision.gameObject.GetComponent<PlayerHealth>() == null) return;
            bool below = collision.transform.position.y < transform.position.y - .2f;
            if (!below || !arena.Activate()) return;
            StartCoroutine(Bump());
        }
        private IEnumerator Bump()
        {
            float t = 0;
            while (t < .25f) { t += Time.deltaTime; transform.position = origin + Vector3.up * Mathf.Sin(t / .25f * Mathf.PI) * .2f; yield return null; }
            transform.position = origin;
        }
    }
}
