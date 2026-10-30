using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MarioBros.Player
{
    /// <summary>
    /// Estados posibles de potenciación de Mario.
    /// </summary>
    public enum PowerState
    {
        Small,  // Mario pequeño (16x16)
        Big,    // Super Mario grande (16x32)
        Fire    // Mario de fuego (16x32 con bolas de fuego)
    }

    /// <summary>
    /// Sistema de salud y power-ups de Mario.
    /// Gestiona estados (Small -> Big -> Fire), daño con degradación escalonada,
    /// invencibilidad con parpadeo visual, y animación de muerte con reinicio de escena tras 2 segundos.
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Estado de Poder")]
        [Tooltip("Estado inicial de Mario.")]
        [SerializeField] private PowerState currentPowerState = PowerState.Small;

        [Header("Invencibilidad")]
        [Tooltip("Duración en segundos del periodo de invulnerabilidad tras recibir daño.")]
        [SerializeField] private float invincibilityDuration = 2.0f;

        [Tooltip("Intervalo de parpadeo del sprite durante la invencibilidad.")]
        [SerializeField] private float blinkInterval = 0.1f;

        [Header("Muerte y Reinicio")]
        [Tooltip("Tiempo de espera en segundos antes de recargar la escena tras morir.")]
        [SerializeField] private float deathDelay = 2.0f;

        [Tooltip("Fuerza del salto característico de Mario hacia arriba al morir.")]
        [SerializeField] private float deathBounceForce = 10f;

        [Header("Dimensiones de Collider (PPU = 16)")]
        [Tooltip("Tamaño de BoxCollider2D para Mario Pequeño (16x16 px = ~1 unidad).")]
        [SerializeField] private Vector2 smallColliderSize = new Vector2(0.75f, 0.95f);

        [Tooltip("Offset de BoxCollider2D para Mario Pequeño.")]
        [SerializeField] private Vector2 smallColliderOffset = new Vector2(0f, 0.475f);

        [Tooltip("Tamaño de BoxCollider2D para Mario Grande o Fuego (16x32 px = ~2 unidades).")]
        [SerializeField] private Vector2 bigColliderSize = new Vector2(0.75f, 1.95f);

        [Tooltip("Offset de BoxCollider2D para Mario Grande o Fuego.")]
        [SerializeField] private Vector2 bigColliderOffset = new Vector2(0f, 0.975f);

        [Header("Referencias Opcionales (auto-detectadas si están vacías)")]
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerAnimation playerAnimation;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private BoxCollider2D boxCollider;
        [SerializeField] private Rigidbody2D rb;

        // Variables de estado interno
        private bool isInvincible = false;
        private bool isDead = false;
        private Coroutine invincibilityCoroutine;

        // Propiedades públicas
        public PowerState CurrentPowerState => currentPowerState;
        public bool IsInvincible => isInvincible;
        public bool IsDead => isDead;
        public bool IsBig => currentPowerState == PowerState.Big || currentPowerState == PowerState.Fire;

        private void Awake()
        {
            if (playerMovement == null)
            {
                playerMovement = GetComponent<PlayerMovement>();
            }

            if (playerAnimation == null)
            {
                playerAnimation = GetComponent<PlayerAnimation>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (boxCollider == null)
            {
                boxCollider = GetComponent<BoxCollider2D>();
            }

            if (rb == null)
            {
                rb = GetComponent<Rigidbody2D>();
            }
        }

        private void Start()
        {
            // Aplicar configuración inicial del collider según el estado de partida
            ApplyStateColliders(IsBig);

            if (playerAnimation != null)
            {
                playerAnimation.SetBig(IsBig);
            }
        }

        /// <summary>
        /// Aplica daño a Mario siguiendo la progresión: Fire -> Big -> Small -> Muerte.
        /// Al perder un power-up, se otorga un periodo de invencibilidad con parpadeo.
        /// </summary>
        public void TakeDamage()
        {
            // Si ya está muerto o es invencible, ignorar daño
            if (isDead || isInvincible) return;

            switch (currentPowerState)
            {
                case PowerState.Fire:
                    // De Fuego pasa a Grande
                    SetPowerState(PowerState.Big);
                    if (playerAnimation != null)
                    {
                        playerAnimation.TriggerShrink();
                    }
                    StartInvincibility();
                    break;

                case PowerState.Big:
                    // De Grande pasa a Pequeño
                    SetPowerState(PowerState.Small);
                    if (playerAnimation != null)
                    {
                        playerAnimation.TriggerShrink();
                    }
                    StartInvincibility();
                    break;

                case PowerState.Small:
                    // De Pequeño pasa a Muerte
                    Die();
                    break;
            }
        }

        /// <summary>
        /// Recolecta un Super Champiñón, haciendo crecer a Mario de Pequeño a Grande.
        /// </summary>
        public void CollectMushroom()
        {
            if (isDead) return;

            if (currentPowerState == PowerState.Small)
            {
                SetPowerState(PowerState.Big);
                if (playerAnimation != null)
                {
                    playerAnimation.TriggerGrow();
                }
            }
        }

        /// <summary>
        /// Recolecta una Flor de Fuego, otorgando el poder de fuego a Mario.
        /// </summary>
        public void CollectFireFlower()
        {
            if (isDead) return;

            SetPowerState(PowerState.Fire);

            if (playerAnimation != null)
            {
                playerAnimation.TriggerGrow();
            }
        }

        /// <summary>
        /// Cambia el estado actual de Mario y ajusta las dimensiones del collider y animación.
        /// </summary>
        /// <param name="newState">Nuevo estado de poder.</param>
        public void SetPowerState(PowerState newState)
        {
            currentPowerState = newState;
            bool big = (newState == PowerState.Big || newState == PowerState.Fire);

            ApplyStateColliders(big);

            if (playerAnimation != null)
            {
                playerAnimation.SetBig(big);
            }
        }

        /// <summary>
        /// Ajusta el tamaño y el offset del BoxCollider2D según si Mario es Pequeño o Grande.
        /// </summary>
        /// <param name="isBigState">True si es grande/fuego, False si es pequeño.</param>
        private void ApplyStateColliders(bool isBigState)
        {
            IsCrouching = false;
            if (boxCollider == null) return;

            if (isBigState)
            {
                boxCollider.size = bigColliderSize;
                boxCollider.offset = bigColliderOffset;
            }
            else
            {
                boxCollider.size = smallColliderSize;
                boxCollider.offset = smallColliderOffset;
            }
        }

        public bool IsCrouching { get; private set; }
        public void SetCrouching(bool crouch)
        {
            if (boxCollider == null || isDead) return;
            crouch = crouch && IsBig;
            if (crouch == IsCrouching) return;
            if (!crouch && IsBig)
            {
                Vector2 standingCenter=(Vector2)transform.position+bigColliderOffset;
                if (Physics2D.OverlapBox(standingCenter,bigColliderSize-new Vector2(.08f,.08f),0,LayerMask.GetMask("Ground")) != null) return;
            }
            if (crouch)
            {
                float bottom=bigColliderOffset.y-bigColliderSize.y*.5f;
                boxCollider.size=new Vector2(bigColliderSize.x,1.35f);
                boxCollider.offset=new Vector2(0,bottom+.675f);
            }
            else ApplyStateColliders(IsBig);
            IsCrouching=crouch;
        }

        /// <summary>
        /// <summary>
        /// Aplica una mejora de estado a Mario (Small -> Big, etc.).
        /// </summary>
        /// <param name="newState">Nuevo estado de poder.</param>
        public void PowerUp(PowerState newState)
        {
            SetPowerState(newState);
        }

        /// <summary>
        /// Activa la rutina de invencibilidad temporal con efecto visual de parpadeo usando la duración por defecto.
        /// </summary>
        public void StartInvincibility()
        {
            StartInvincibility(invincibilityDuration);
        }

        /// <summary>
        /// Activa la rutina de invencibilidad temporal con una duración personalizada (por ejemplo Starman).
        /// </summary>
        /// <param name="duration">Duración en segundos de la invencibilidad.</param>
        public void StartInvincibility(float duration)
        {
            if (invincibilityCoroutine != null)
            {
                StopCoroutine(invincibilityCoroutine);
            }

            invincibilityCoroutine = StartCoroutine(InvincibilityRoutine(duration));
        }

        private IEnumerator InvincibilityRoutine(float duration)
        {
            isInvincible = true;
            float elapsed = 0f;
            bool visible = true;

            while (elapsed < duration)
            {
                visible = !visible;
                if (spriteRenderer != null)
                {
                    Color color = spriteRenderer.color;
                    color.a = visible ? 1f : 0.25f;
                    spriteRenderer.color = color;
                }

                yield return new WaitForSeconds(blinkInterval);
                elapsed += blinkInterval;
            }

            // Restaurar visibilidad total al finalizar
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 1f;
                spriteRenderer.color = color;
            }

            isInvincible = false;
            invincibilityCoroutine = null;
        }

        /// <summary>
        /// Ejecuta la secuencia de muerte estilo Mario:
        /// Desactiva controles, deshabilita colisiones para caer a través del escenario,
        /// realiza el salto de muerte hacia arriba y recarga la escena tras 2 segundos.
        /// </summary>
        public void Die()
        {
            if (isDead) return;

            isDead = true;

            // Detener cualquier efecto de invencibilidad en curso y restaurar opacidad
            if (invincibilityCoroutine != null)
            {
                StopCoroutine(invincibilityCoroutine);
                invincibilityCoroutine = null;
            }

            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = 1f;
                spriteRenderer.color = c;
            }

            // Desactivar movimiento y controles
            if (playerMovement != null)
            {
                playerMovement.StopMovement();
                playerMovement.enabled = false;
            }

            // Desactivar colliders para que Mario no interactúe con el entorno durante la muerte
            Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
            foreach (Collider2D col in colliders)
            {
                col.enabled = false;
            }

            // Salto característico de muerte de Mario
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0f, deathBounceForce);
                rb.gravityScale = 3f; // Asegurar gravedad activa para la caída
            }

            // Disparar animación de muerte
            if (playerAnimation != null)
            {
                playerAnimation.TriggerDeath();
            }

            // Programar recarga de escena tras 2 segundos
            StartCoroutine(DeathReloadRoutine());
        }

        private IEnumerator DeathReloadRoutine()
        {
            yield return new WaitForSeconds(deathDelay);
            if (UI.GameManager.Instance == null)
                new GameObject("GameManager").AddComponent<UI.GameManager>();
            UI.GameManager.Instance.LoseLife();
        }
    }
}
