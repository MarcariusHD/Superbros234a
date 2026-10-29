using System.Collections;
using UnityEngine;
using MarioBros.Items;
using MarioBros.Player;
using MarioBros.UI;

namespace MarioBros.Blocks
{
    /// <summary>
    /// Bloque de ladrillos rompible (Brick Block).
    /// - Al ser golpeado por Mario Pequeño: realiza solo animación de rebote (bump).
    /// - Al ser golpeado por Mario Grande / Fuego: se fragmenta en pedazos y se destruye.
    /// - Si contiene monedas ocultas (Multi-Coin Brick): expulsa monedas con rebote
    ///   hasta agotarse, convirtiéndose en un bloque sólido usado.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class BrickBlock : MonoBehaviour
    {
        [Header("Monedas Ocultas (Opcional)")]
        [Tooltip("Cantidad de monedas contenidas. Si es 0, es un bloque rompible normal.")]
        [SerializeField] private int coinCount = 0;

        [Tooltip("Prefab de la moneda a expulsar si contiene monedas.")]
        [SerializeField] private GameObject coinPrefab;

        [Tooltip("Sprite del bloque usado cuando se agotan las monedas.")]
        [SerializeField] private Sprite emptyBlockSprite;

        [Header("Efectos de Ruptura")]
        [Tooltip("Prefab de los fragmentos de ladrillo al romperse (4 piezas o partículas).")]
        [SerializeField] private GameObject brickPiecePrefab;

        [Tooltip("Puntos sumados al romper el ladrillo (50 puntos en NES).")]
        [SerializeField] private int breakScore = 50;

        [Header("Animación de Rebote (Bump)")]
        [Tooltip("Distancia vertical que sube el bloque al no romperse.")]
        [SerializeField] private float bumpHeight = 0.35f;

        [Tooltip("Velocidad de movimiento del rebote.")]
        [SerializeField] private float bumpSpeed = 8f;

        [Header("Sonidos")]
        [SerializeField] private AudioClip bumpSound;
        [SerializeField] private AudioClip breakSound;

        // Componentes internos
        private SpriteRenderer spriteRenderer;
        private Vector3 originalPosition;
        private bool isBumping = false;
        private bool isBroken = false;
        private bool isDepleted = false;

        public bool IsBroken => isBroken;
        public bool HasCoins => coinCount > 0;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            originalPosition = transform.localPosition;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (isBroken || isBumping) return;

            // Verificar si el impacto es con Mario
            if (!collision.gameObject.CompareTag("Player") && collision.gameObject.GetComponent<PlayerMovement>() == null)
            {
                return;
            }

            // Detectar impacto desde abajo
            bool hitFromBelow = false;
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    hitFromBelow = true;
                    break;
                }
            }

            if (!hitFromBelow && collision.transform.position.y < transform.position.y - 0.2f)
            {
                hitFromBelow = true;
            }

            if (hitFromBelow)
            {
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                Hit(playerHealth);
            }
        }

        /// <summary>
        /// Procesa el impacto recibido desde abajo.
        /// </summary>
        public void Hit(PlayerHealth playerHealth)
        {
            if (isBroken || isDepleted) return;

            // Caso 1: Bloque con monedas
            if (coinCount > 0)
            {
                DispenseCoin();
                return;
            }

            // Caso 2: Bloque normal sin monedas
            bool isBigMario = (playerHealth != null && playerHealth.IsBig);

            if (isBigMario)
            {
                // Mario Grande o Fuego rompe el bloque
                BreakBlock();
            }
            else
            {
                // Mario Pequeño solo hace rebotar el bloque
                Bump();
            }
        }

        /// <summary>
        /// Expulsa una moneda oculta del ladrillo.
        /// </summary>
        private void DispenseCoin()
        {
            coinCount--;

            if (coinPrefab != null)
            {
                GameObject coinObj = Instantiate(coinPrefab, transform.position, Quaternion.identity);
                Coin coinScript = coinObj.GetComponent<Coin>();
                if (coinScript != null)
                {
                    coinScript.LaunchFromBlock(transform.position);
                }
            }
            else
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddCoin();
                }
            }

            Bump();

            // Si se agotaron las monedas, se convierte en bloque usado
            if (coinCount <= 0)
            {
                isDepleted = true;
                if (emptyBlockSprite != null && spriteRenderer != null)
                {
                    spriteRenderer.sprite = emptyBlockSprite;
                }
            }
        }

        /// <summary>
        /// Realiza la animación de rebote hacia arriba y abajo.
        /// </summary>
        private void Bump()
        {
            if (bumpSound != null)
            {
                AudioSource.PlayClipAtPoint(bumpSound, transform.position);
            }

            if (!isBumping && gameObject.activeInHierarchy)
            {
                StartCoroutine(BumpRoutine());
            }
        }

        private IEnumerator BumpRoutine()
        {
            isBumping = true;
            Vector3 peakPosition = originalPosition + Vector3.up * bumpHeight;

            while (transform.localPosition.y < peakPosition.y - 0.01f)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, peakPosition, bumpSpeed * Time.deltaTime);
                yield return null;
            }

            transform.localPosition = peakPosition;

            while (transform.localPosition.y > originalPosition.y + 0.01f)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, originalPosition, bumpSpeed * Time.deltaTime);
                yield return null;
            }

            transform.localPosition = originalPosition;
            isBumping = false;
        }

        /// <summary>
        /// Rompe el bloque en 4 fragmentos, suma puntos y lo destruye.
        /// </summary>
        private void BreakBlock()
        {
            isBroken = true;

            if (breakSound != null)
            {
                AudioSource.PlayClipAtPoint(breakSound, transform.position);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(breakScore);
            }

            // Generar fragmentos
            SpawnDebris();

            // Destruir el bloque
            Destroy(gameObject);
        }

        /// <summary>
        /// Crea los 4 fragmentos característicos saliendo en arcos parabólicos.
        /// </summary>
        private void SpawnDebris()
        {
            if (brickPiecePrefab != null)
            {
                // Posiciones relativas de los 4 fragmentos
                Vector2[] velocities = new Vector2[]
                {
                    new Vector2(-3f, 8f),   // Arriba a la izquierda
                    new Vector2(3f, 8f),    // Arriba a la derecha
                    new Vector2(-2.5f, 5f), // Abajo a la izquierda
                    new Vector2(2.5f, 5f)   // Abajo a la derecha
                };

                Vector3[] offsets = new Vector3[]
                {
                    new Vector3(-0.25f, 0.25f, 0f),
                    new Vector3(0.25f, 0.25f, 0f),
                    new Vector3(-0.25f, -0.25f, 0f),
                    new Vector3(0.25f, -0.25f, 0f)
                };

                for (int i = 0; i < 4; i++)
                {
                    GameObject piece = Instantiate(brickPiecePrefab, transform.position + offsets[i], Quaternion.identity);
                    Rigidbody2D pieceRb = piece.GetComponent<Rigidbody2D>();
                    if (pieceRb != null)
                    {
                        pieceRb.linearVelocity = velocities[i];
                    }
                    Destroy(piece, 3f);
                }
            }
        }
    }
}
