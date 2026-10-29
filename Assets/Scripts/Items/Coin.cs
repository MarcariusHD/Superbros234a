using System;
using System.Collections;
using UnityEngine;
using MarioBros.UI;

namespace MarioBros.Items
{
    /// <summary>
    /// Moneda coleccionable en el nivel o emergente desde un bloque de interrogación/ladrillo.
    /// Realiza animación visual (rotación de escala o intercambio de sprites).
    /// Al recolectarse, suma puntos y monedas a través del GameManager y dispara un evento estático.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Coin : MonoBehaviour
    {
        [Header("Puntuación")]
        [Tooltip("Puntos otorgados al recoger la moneda.")]
        [SerializeField] private int scoreValue = 200;

        [Header("Efectos Sonoros")]
        [Tooltip("Sonido reproducido al recolectar la moneda.")]
        [SerializeField] private AudioClip collectSound;

        [Header("Animación por Sprites (Opcional)")]
        [Tooltip("Fotogramas para animación de giro de la moneda.")]
        [SerializeField] private Sprite[] animationFrames;

        [Tooltip("Fotogramas por segundo para la animación.")]
        [SerializeField] private float frameRate = 8f;

        [Header("Animación Alternativa de Escala X (Si no hay sprites)")]
        [Tooltip("Velocidad de oscilación horizontal si se usa animación procedimental.")]
        [SerializeField] private float proceduralSpinSpeed = 6f;

        // Eventos
        public static event Action<Coin> OnCoinCollected;

        // Componentes internos
        private SpriteRenderer spriteRenderer;
        private Collider2D col;
        private bool isCollected = false;
        private float animationTimer = 0f;
        private int currentFrameIndex = 0;
        private Vector3 initialScale;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            col = GetComponent<Collider2D>();
            initialScale = transform.localScale;

            // Asegurar que el collider sea de tipo Trigger para recolección
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void Update()
        {
            if (isCollected) return;

            // Animación por cambio de sprites si están asignados
            if (animationFrames != null && animationFrames.Length > 0 && spriteRenderer != null)
            {
                animationTimer += Time.deltaTime;
                if (animationTimer >= 1f / frameRate)
                {
                    animationTimer = 0f;
                    currentFrameIndex = (currentFrameIndex + 1) % animationFrames.Length;
                    spriteRenderer.sprite = animationFrames[currentFrameIndex];
                }
            }
            else
            {
                // Animación procedimental simple oscilando la escala en X para simular rotación 2D
                float scaleX = Mathf.Abs(Mathf.Sin(Time.time * proceduralSpinSpeed));
                transform.localScale = new Vector3(scaleX * initialScale.x, initialScale.y, initialScale.z);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isCollected) return;

            // Comprobación de colisión con Mario
            if (other.CompareTag("Player") || other.GetComponent<Player.PlayerMovement>() != null)
            {
                Collect();
            }
        }

        /// <summary>
        /// Ejecuta la recolección de la moneda: añade puntos, reproduce sonido,
        /// dispara el evento estático y destruye el GameObject.
        /// </summary>
        public void Collect()
        {
            if (isCollected) return;
            isCollected = true;

            // Notificar a GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCoin();
            }

            // Reproducir sonido en la posición actual
            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position);
            }

            // Disparar evento para observadores
            OnCoinCollected?.Invoke(this);

            // Destruir la moneda
            Destroy(gameObject);
        }

        /// <summary>
        /// Animación de la moneda saltando desde un bloque golpeado hacia arriba y desapareciendo.
        /// </summary>
        public void LaunchFromBlock(Vector3 startPosition)
        {
            StartCoroutine(PopRoutine(startPosition));
        }

        private IEnumerator PopRoutine(Vector3 startPosition)
        {
            if (col != null) col.enabled = false;
            transform.position = startPosition;

            float duration = 0.45f;
            float height = 2.2f;
            float elapsed = 0f;

            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, startPosition);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCoin();
            }

            OnCoinCollected?.Invoke(this);

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                // Movimiento parabólico de subida y bajada
                float yOffset = Mathf.Sin(t * Mathf.PI) * height;
                transform.position = startPosition + new Vector3(0f, yOffset, 0f);

                // Rotación visual durante el salto
                float scaleX = Mathf.Abs(Mathf.Sin(Time.time * 15f));
                transform.localScale = new Vector3(scaleX * initialScale.x, initialScale.y, initialScale.z);

                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
