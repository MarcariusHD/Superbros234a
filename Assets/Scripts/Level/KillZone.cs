using UnityEngine;
using MarioBros.Player;

namespace MarioBros.Level
{
    /// <summary>
    /// Zona de muerte al fondo del nivel (precipicio / vacío).
    /// Posee un BoxCollider2D trigger que abarca el límite inferior del mapa.
    /// - Al entrar el jugador (Player), activa su secuencia de muerte (PlayerHealth.Die).
    /// - Al entrar un enemigo o proyectil/ítem, lo destruye de inmediato.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class KillZone : MonoBehaviour
    {
        [Header("Tags Reconocidos")]
        [Tooltip("Tag utilizado para identificar a Mario.")]
        [SerializeField] private string playerTag = "Player";

        [Tooltip("Tag utilizado para identificar enemigos.")]
        [SerializeField] private string enemyTag = "Enemy";

        private void Awake()
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Caso 1: Mario cae al vacío
            if (other.CompareTag(playerTag))
            {
                PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth == null)
                {
                    playerHealth = other.GetComponentInParent<PlayerHealth>();
                }

                if (playerHealth != null && !playerHealth.IsDead)
                {
                    playerHealth.Die();
                }
                return;
            }

            // Caso 2: Enemigo cae al vacío
            if (other.CompareTag(enemyTag))
            {
                Destroy(other.gameObject);
                return;
            }

            // Comprobación por componentes si no se asignaron tags
            PlayerHealth healthCheck = other.GetComponent<PlayerHealth>();
            if (healthCheck != null && !healthCheck.IsDead)
            {
                healthCheck.Die();
                return;
            }

            // Destruir cualquier otro ítem o escombro que caiga fuera del mundo
            Destroy(other.gameObject);
        }

        private void OnDrawGizmos()
        {
            // Dibujar la zona de muerte en color rojo en la vista de escena
            Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                Gizmos.DrawCube(col.bounds.center, col.bounds.size);
            }
        }
    }
}
