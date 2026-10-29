using UnityEngine;
using MarioBros.Player;

namespace MarioBros.Level
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class LoopingLift : MonoBehaviour
    {
        public float speed = 2;
        private Rigidbody2D body;
        private void Awake() { body = GetComponent<Rigidbody2D>(); }
        private void FixedUpdate()
        {
            float y = body.position.y + speed * Time.fixedDeltaTime;
            if (y > 13) y = -1;
            if (y < -1) y = 13;
            body.MovePosition(new Vector2(body.position.x, y));
        }
        private void OnCollisionStay2D(Collision2D collision)
        {
            var player = collision.gameObject.GetComponent<PlayerMovement>();
            if (player != null && collision.transform.position.y > transform.position.y)
                collision.transform.position += Vector3.up * (speed * Time.fixedDeltaTime);
        }
    }
}
