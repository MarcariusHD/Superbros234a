using System.Collections;
using UnityEngine;
using MarioBros.Player;
using MarioBros.UI;

namespace MarioBros.Items
{
    /// <summary>
    /// Super Champiñón de mejora para Mario (Power-Up).
    /// Emerge lentamente del bloque interrogante, luego avanza horizontalmente
    /// cayendo por gravedad y rebotando en paredes. Al tocar a Mario, lo transforma a Super Mario (Big).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Mushroom : MonoBehaviour
    {
        [Header("Movimiento")]
        [Tooltip("Velocidad horizontal constante de avance.")]
        [SerializeField] private float speed = 3.0f;

        [Tooltip("Dirección horizontal inicial (1 = derecha, -1 = izquierda).")]
        [SerializeField] private float initialDirection = 1f;

        [Header("Puntuación y Sonido")]
        [Tooltip("Puntos otorgados al recolectar el champiñón.")]
        [SerializeField] private int scoreValue = 1000;

        [Tooltip("Sonido de power-up al ser recogido.")]
        [SerializeField] private AudioClip powerUpSound;

        [Tooltip("Sonido emitido al emerger del bloque.")]
        [SerializeField] private AudioClip spawnSound;

        [Header("Animación de Emergencia")]
        [Tooltip("Distancia que asciende el champiñón al salir del bloque.")]
        [SerializeField] private float riseDistance = 1.0f;

        [Tooltip("Tiempo en segundos que tarda en emerger del bloque.")]
        [SerializeField] private float riseDuration = 0.6f;

        // Componentes internos
        private Rigidbody2D rb;
        private Collider2D col;
        private float direction;
        private bool isSpawning = false;
        private bool isCollected = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            direction = initialDirection;

            // Bloquear rotación en el eje Z
            rb.freezeRotation = true;
        }

        /// <summary>
        /// Inicia la secuencia de emergencia desde un bloque de interrogación.
        /// </summary>
        public void SpawnFromBlock(Vector3 blockPosition)
        {
            StartCoroutine(RiseFromBlockRoutine(blockPosition));
        }

        private IEnumerator RiseFromBlockRoutine(Vector3 blockPosition)
        {
            isSpawning = true;
            rb.isKinematic = true;
            rb.linearVelocity = Vector2.zero;
            col.isTrigger = true; // No colisionar con sólidos mientras emerge

            transform.position = blockPosition;

            if (spawnSound != null)
            {
                AudioSource.PlayClipAtPoint(spawnSound, blockPosition);
            }

            Vector3 startPos = blockPosition;
            Vector3 endPos = blockPosition + new Vector3(0f, riseDistance, 0f);
            float elapsed = 0f;

            while (elapsed < riseDuration)
            {
                transform.position = Vector3.Lerp(startPos, endPos, elapsed / riseDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = endPos;

            // Activar física normal
            rb.isKinematic = false;
            col.isTrigger = false;
            isSpawning = false;
            rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
        }

        private void FixedUpdate()
        {
            if (isSpawning || isCollected) return;

            // Mantener velocidad horizontal constante respetando la gravedad vertical
            rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (isSpawning || isCollected) return;

            // Comprobar colisión con el jugador
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Collect(playerHealth);
                return;
            }

            // Detectar choque frontal contra paredes sólidas para invertir dirección
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // Si la normal apunta horizontalmente en contra de nuestra dirección de avance
                if (Mathf.Abs(contact.normal.x) > 0.5f)
                {
                    // Invertir dirección
                    direction = -direction;
                    rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
                    break;
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isSpawning || isCollected) return;

            // Permite recolectar también si el collider de Mario es Trigger o el champiñón lo es
            if (other.CompareTag("Player"))
            {
                PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    Collect(playerHealth);
                }
            }
        }

        /// <summary>
        /// Aplica la mejora al jugador, añade puntaje y destruye el champiñón.
        /// </summary>
        private void Collect(PlayerHealth playerHealth)
        {
            if (isCollected) return;
            isCollected = true;

            // Aplicar Power-up a Mario
            playerHealth.PowerUp(PowerState.Big);

            // Sumar puntos
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(scoreValue);
            }

            // Reproducir sonido
            if (powerUpSound != null)
            {
                AudioSource.PlayClipAtPoint(powerUpSound, transform.position);
            }

            Destroy(gameObject);
        }
    }
}
