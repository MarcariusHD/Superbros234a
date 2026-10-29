using System.Collections;
using UnityEngine;
using MarioBros.Player;
using MarioBros.UI;

namespace MarioBros.Items
{
    /// <summary>
    /// Estrella de invencibilidad (Starman / Super Star).
    /// Emerge de los bloques interrogantes, rebota constantemente sobre el suelo y avanza
    /// rebotando contra obstáculos. Al recogerla, otorga invencibilidad temporal (~10s) a Mario.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Starman : MonoBehaviour
    {
        [Header("Movimiento y Rebote")]
        [Tooltip("Velocidad horizontal de avance.")]
        [SerializeField] private float speed = 3.5f;

        [Tooltip("Fuerza vertical de rebote constante al tocar el suelo.")]
        [SerializeField] private float bounceForce = 9.0f;

        [Tooltip("Dirección horizontal inicial (1 = derecha, -1 = izquierda).")]
        [SerializeField] private float initialDirection = 1f;

        [Header("Invencibilidad")]
        [Tooltip("Duración en segundos de la invencibilidad otorgada a Mario.")]
        [SerializeField] private float invincibilityDuration = 10.0f;

        [Tooltip("Puntos otorgados al recoger la estrella.")]
        [SerializeField] private int scoreValue = 1000;

        [Header("Sonidos")]
        [Tooltip("Sonido o música de estrella al ser recogida.")]
        [SerializeField] private AudioClip starSound;

        [Tooltip("Sonido al salir del bloque.")]
        [SerializeField] private AudioClip spawnSound;

        [Header("Animación de Emergencia")]
        [Tooltip("Distancia que sube del bloque antes de iniciar los rebotes.")]
        [SerializeField] private float riseDistance = 1.0f;

        [Tooltip("Duración de la animación de salida del bloque.")]
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

            rb.freezeRotation = true;
        }

        /// <summary>
        /// Inicia la emergencia de la estrella desde un bloque interrogante.
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
            col.isTrigger = true;

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

            // Activar física y aplicar primer salto
            rb.isKinematic = false;
            col.isTrigger = false;
            isSpawning = false;
            rb.linearVelocity = new Vector2(direction * speed, bounceForce);
        }

        private void FixedUpdate()
        {
            if (isSpawning || isCollected) return;

            // Mantener velocidad horizontal constante
            rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (isSpawning || isCollected) return;

            // Comprobar contacto con Mario
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Collect(playerHealth);
                return;
            }

            foreach (ContactPoint2D contact in collision.contacts)
            {
                // Si colisiona con el suelo (normal hacia arriba), rebotar hacia arriba
                if (contact.normal.y > 0.6f)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceForce);
                }
                // Si colisiona con una pared lateral, rebotar e invertir dirección
                else if (Mathf.Abs(contact.normal.x) > 0.5f)
                {
                    direction = -direction;
                    rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isSpawning || isCollected) return;

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
        /// Otorga invencibilidad a Mario, suma puntos y destruye la estrella.
        /// </summary>
        private void Collect(PlayerHealth playerHealth)
        {
            if (isCollected) return;
            isCollected = true;

            // Activar invencibilidad por ~10 segundos en Mario
            playerHealth.StartInvincibility(invincibilityDuration);

            // Sumar puntos
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(scoreValue);
            }

            // Reproducir sonido
            if (starSound != null)
            {
                AudioSource.PlayClipAtPoint(starSound, transform.position);
            }

            Destroy(gameObject);
        }
    }
}
