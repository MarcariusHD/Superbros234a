using UnityEngine;

namespace MarioBros.World1_4
{
    public sealed class AdventureVisual : MonoBehaviour
    {
        public Sprite[] frames;
        public float frameRate = 8, parallax, drift, bob;
        private SpriteRenderer image;
        private Vector3 origin;
        private void Start() { image = GetComponent<SpriteRenderer>(); origin = transform.position; }
        private void LateUpdate()
        {
            if (frames != null && frames.Length > 0 && image != null) image.sprite = frames[(int)(Time.time * frameRate) % frames.Length];
            if (parallax == 0 && drift == 0 && bob == 0) return;
            Vector3 next = origin;
            if (Camera.main != null) next.x += (Camera.main.transform.position.x - origin.x) * parallax;
            next.x += Mathf.Sin(Time.time * .06f + origin.x) * drift;
            next.y += Mathf.Sin(Time.time * 2 + origin.x) * bob;
            transform.position = new Vector3(Mathf.Round(next.x * 16) / 16, Mathf.Round(next.y * 16) / 16, next.z);
        }
    }
}
