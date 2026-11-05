using MarioBros.Player;
using UnityEngine;
using UnityEngine.Events;

namespace MarioBros.World1_4
{
    public sealed class AdventureZone : MonoBehaviour
    {
        public AdventureEnvironment environment;
        public string title;
        [TextArea] public string hint;
        public float cameraLeft = 62, cameraRight = 346;
        public UnityEvent onEntered = new UnityEvent();
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerHealth>() != null && !AdventureLevel.Instance.Traveling)
                AdventureLevel.Instance.EnterZone(this);
        }
    }
}
