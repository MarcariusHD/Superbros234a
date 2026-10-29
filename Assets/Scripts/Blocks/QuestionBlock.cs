using System.Collections;
using UnityEngine;
using MarioBros.Items;
using MarioBros.Player;
using MarioBros.UI;

namespace MarioBros.Blocks
{
    /// <summary>
    /// Tipo de contenido que puede albergar un Bloque de Interrogación.
    /// </summary>
    public enum QuestionBlockContent
    {
        Coin,
        Mushroom,
        FireFlower,
        Starman
    }

    /// <summary>
    /// Bloque de Interrogación (?) clásico de Super Mario Bros.
    /// Al ser golpeado desde abajo por Mario:
    /// - Ejecuta una pequeña animación de rebote (bump).
    /// - Expulsa el ítem configurado (Moneda, Champiñón, Flor o Estrella).
    /// - Cambia su apariencia al sprite de bloque vacío / usado.
    /// - Queda desactivado para golpes posteriores.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class QuestionBlock : MonoBehaviour
    {
        [Header("Contenido")]
        [Tooltip("Ítem que aparecerá al golpear el bloque.")]
        [SerializeField] private QuestionBlockContent content = QuestionBlockContent.Coin;

        [Tooltip("Si Mario es pequeño y el contenido es Flor de Fuego, expulsa Champiñón (comportamiento original de NES).")]
        [SerializeField] private bool adaptFlowerToMarioState = true;

        [Header("Prefabs de Contenido")]
        [SerializeField] private GameObject coinPrefab;
        [SerializeField] private GameObject mushroomPrefab;
        [SerializeField] private GameObject fireFlowerPrefab;
        [SerializeField] private GameObject starmanPrefab;

        [Header("Visuales y Sprite")]
        [Tooltip("Sprite del bloque una vez que ha sido golpeado y vaciado.")]
        [SerializeField] private Sprite emptyBlockSprite;

        [Header("Animación de Rebote (Bump)")]
        [Tooltip("Distancia que asciende el bloque al ser golpeado.")]
        [SerializeField] private float bumpHeight = 0.35f;

        [Tooltip("Velocidad del rebote.")]
        [SerializeField] private float bumpSpeed = 8f;

        [Header("Sonidos")]
        [SerializeField] private AudioClip bumpSound;
        [SerializeField] private AudioClip powerupSpawnSound;

        // Componentes internos
        private SpriteRenderer spriteRenderer;
        private Animator animator;
        private Vector3 originalPosition;
        private bool isHit = false;
        private bool isBumping = false;

        public bool IsHit => isHit;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            originalPosition = transform.localPosition;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (isHit) return;

            // Verificar si quien colisiona es Mario
            if (!collision.gameObject.CompareTag("Player") && collision.gameObject.GetComponent<PlayerMovement>() == null)
            {
                return;
            }

            // Comprobar si el impacto proviene desde abajo del bloque
            // El vector normal del contacto en el bloque apuntará hacia abajo respecto al bloque si Mario viene subiendo,
            // o el jugador se encuentra por debajo de la base del bloque
            bool hitFromBelow = false;
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // Normal apuntando hacia arriba significa que la superficie impactada es la parte inferior del bloque
                if (contact.normal.y > 0.5f)
                {
                    hitFromBelow = true;
                    break;
                }
            }

            // Chequeo de respaldo: posición en Y de Mario menor que el centro del bloque
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
        /// Ejecuta la activación del bloque: rebote, cambio de sprite y expulsión del ítem.
        /// </summary>
        public void Hit(PlayerHealth playerHealth)
        {
            if (isHit) return;
            isHit = true;

            // Cambiar sprite al estado vacío y desactivar animación de parpadeo de la interrogación
            if (animator != null)
            {
                animator.enabled = false;
            }

            if (emptyBlockSprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = emptyBlockSprite;
            }

            // Sonido de golpe
            if (bumpSound != null)
            {
                AudioSource.PlayClipAtPoint(bumpSound, transform.position);
            }

            // Iniciar animación de rebote
            if (!isBumping)
            {
                StartCoroutine(BumpRoutine());
            }

            // Expulsar el contenido correspondiente
            SpawnContent(playerHealth);
        }

        private IEnumerator BumpRoutine()
        {
            isBumping = true;
            Vector3 peakPosition = originalPosition + Vector3.up * bumpHeight;

            // Subir
            while (transform.localPosition.y < peakPosition.y - 0.01f)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, peakPosition, bumpSpeed * Time.deltaTime);
                yield return null;
            }

            transform.localPosition = peakPosition;

            // Bajar a posición original
            while (transform.localPosition.y > originalPosition.y + 0.01f)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, originalPosition, bumpSpeed * Time.deltaTime);
                yield return null;
            }

            transform.localPosition = originalPosition;
            isBumping = false;
        }

        private void SpawnContent(PlayerHealth playerHealth)
        {
            QuestionBlockContent spawnType = content;

            // Adaptar Flor de Fuego a Champiñón si Mario es pequeño
            if (spawnType == QuestionBlockContent.FireFlower && adaptFlowerToMarioState && playerHealth != null)
            {
                if (playerHealth.CurrentPowerState == PowerState.Small)
                {
                    spawnType = QuestionBlockContent.Mushroom;
                }
            }

            Vector3 spawnPos = transform.position + Vector3.up * 0.5f;

            switch (spawnType)
            {
                case QuestionBlockContent.Coin:
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
                        // Si no hay prefab asignado, otorgar moneda y puntos directamente al GameManager
                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.AddCoin();
                        }
                    }
                    break;

                case QuestionBlockContent.Mushroom:
                    if (mushroomPrefab != null)
                    {
                        GameObject mObj = Instantiate(mushroomPrefab, transform.position, Quaternion.identity);
                        Mushroom mushroomScript = mObj.GetComponent<Mushroom>();
                        if (mushroomScript != null)
                        {
                            mushroomScript.SpawnFromBlock(transform.position);
                        }
                    }
                    break;

                case QuestionBlockContent.FireFlower:
                    if (fireFlowerPrefab != null)
                    {
                        GameObject fObj = Instantiate(fireFlowerPrefab, transform.position, Quaternion.identity);
                        // Si la flor tiene animación de salida
                        Mushroom flowerAsItem = fObj.GetComponent<Mushroom>();
                        if (flowerAsItem != null)
                        {
                            flowerAsItem.SpawnFromBlock(transform.position);
                        }
                    }
                    break;

                case QuestionBlockContent.Starman:
                    if (starmanPrefab != null)
                    {
                        GameObject sObj = Instantiate(starmanPrefab, transform.position, Quaternion.identity);
                        Starman starScript = sObj.GetComponent<Starman>();
                        if (starScript != null)
                        {
                            starScript.SpawnFromBlock(transform.position);
                        }
                    }
                    break;
            }

            if (spawnType != QuestionBlockContent.Coin && powerupSpawnSound != null)
            {
                AudioSource.PlayClipAtPoint(powerupSpawnSound, transform.position);
            }
        }
    }
}
