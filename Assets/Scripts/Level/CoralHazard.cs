using UnityEngine;

namespace MarioBros.Level
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class CoralHazard : MonoBehaviour
    {
        private void Awake()
        {
            var hitbox = GetComponent<BoxCollider2D>();
            hitbox.isTrigger = false;
            hitbox.size = Vector2.one;
            gameObject.layer = LayerMask.NameToLayer("Ground");
        }
    }
}
