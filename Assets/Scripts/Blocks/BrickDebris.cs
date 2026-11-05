using UnityEngine;

namespace MarioBros.Blocks
{
    public class BrickDebris : MonoBehaviour
    {
        private Sprite[] frames;
        private int offset, phase;
        private SpriteRenderer sprite;
        public void Initialize(Sprite[] frames, int offset, int phase)
        {
            this.frames = frames;this.offset = offset;this.phase = phase;
            sprite = GetComponent<SpriteRenderer>();
            sprite.sprite = frames[offset + phase % 4];
        }
        private void Update()
        {
            sprite.sprite = frames[offset + (phase + Mathf.FloorToInt(Time.time * 12)) % 4];
        }
    }
}
