using UnityEngine;

namespace MarioBros.Level
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(Rigidbody2D))]
    public class HorizontalPlatform : MonoBehaviour
    {
        public float left=95.5f,right=107.5f,speed=2;
        public float DeltaX { get; private set; }
        private Rigidbody2D body;
        private int direction=1;
        private void Awake() { body=GetComponent<Rigidbody2D>(); }
        private void FixedUpdate()
        {
            float next=Mathf.MoveTowards(body.position.x,direction>0?right:left,speed*Time.fixedDeltaTime);
            DeltaX=next-body.position.x;
            body.MovePosition(new Vector2(next,body.position.y));
            if(next>=right||next<=left)direction=-direction;
        }
    }
}
