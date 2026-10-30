using UnityEngine;
using MarioBros.Player;

namespace MarioBros.Level
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(Rigidbody2D))]
    public class LoopingLift : MonoBehaviour
    {
        public float speed = 2;
        public float lowerY = -2;
        public float upperY = 16;
        public float VerticalSpeed => speed;
        private Rigidbody2D body;
        private void Awake() { body = GetComponent<Rigidbody2D>(); }
        private void FixedUpdate()
        {
            float y = body.position.y + speed * Time.fixedDeltaTime;
            if (y > upperY || y < lowerY)
            {
                // Teleport only the platform, never its former passengers.
                body.position = new Vector2(body.position.x, speed > 0 ? lowerY : upperY);
                return;
            }
            body.MovePosition(new Vector2(body.position.x, y));
        }
    }
}
