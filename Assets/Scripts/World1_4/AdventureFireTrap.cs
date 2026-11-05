using UnityEngine;

namespace MarioBros.World1_4
{
    public sealed class AdventureFireTrap : MonoBehaviour
    {
        public bool rotating;
        public float phase, period = 5, speed = 38;
        public GameObject flame;
        public SpriteRenderer warning;
        private Rigidbody2D body;
        private bool hot;
        private void Awake() { body = GetComponent<Rigidbody2D>(); }
        private void FixedUpdate() { if (rotating) body.MoveRotation(phase + Time.fixedTime * speed); }
        private void Update()
        {
            if (rotating) return;
            float t = Mathf.Repeat(Time.time + phase, period);
            bool active = t > period - 1.25f;
            flame.SetActive(active);
            warning.enabled = t > period - 2.4f;
            if (active && !hot && AdventureLevel.Instance != null) AdventureLevel.Instance.sound.Fire(transform.position);
            hot = active;
        }
    }
}
