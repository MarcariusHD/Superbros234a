using UnityEngine;
using MarioBros.Player;

namespace MarioBros.Enemies
{
    /// <summary>
    /// Clase base abstracta para todos los enemigos del juego (Goomba, Koopa, etc.).
    /// Proporciona la funcionalidad común de patrullaje, detección de colisiones,
    /// chequeo de suelo y paredes, orientación de sprite y optimización de visibilidad.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public abstract class EnemyBase : MonoBehaviour
    {
        [Header("Propiedades Base")]
        [Tooltip("Velocidad horizontal de patrullaje del enemigo.")]
        [SerializeField] protected float speed = 2f;

        [Tooltip("Dirección inicial del movimiento (en Mario NES los enemigos suelen comenzar hacia la izquierda).")]
        [SerializeField] protected Vector2 direction = Vector2.left;

        [Tooltip("Indica si el enemigo se encuentra vivo y activo.")]
        [SerializeField] protected bool isAlive = true;

        [Header("Física y Rebote al ser Pisado")]
        [Tooltip("Fuerza de impulso que recibe Mario al pisar al enemigo.")]
        [SerializeField] protected float stompBounceForce = 8f;

        [Tooltip("Fuerza de impulso si Mario mantiene presionado el botón de salto al pisar.")]
        [SerializeField] protected float stompBounceForceHigh = 12f;

        [Header("Detección de Suelo")]
        [Tooltip("Transform opcional en la base para chequear el suelo.")]
        [SerializeField] protected Transform groundCheck;

        [Tooltip("Radio del círculo de detección de suelo.")]
        [SerializeField] protected float groundCheckRadius = 0.15f;

        [Tooltip("Capa que representa el suelo sólido.")]
        [SerializeField] protected LayerMask groundLayer;

        [Header("Detección de Paredes")]
        [Tooltip("Distancia del raycast frontal para detectar muros y cambiar de dirección.")]
        [SerializeField] protected float wallCheckDistance = 0.3f;

        [Tooltip("Capa de las paredes y obstáculos.")]
        [SerializeField] protected LayerMask wallLayer;

        [Header("Optimización")]
        [Tooltip("Desactiva el enemigo cuando sale completamente de la pantalla.")]
        [SerializeField] protected bool deactivateWhenOffscreen = true;

        [Tooltip("Distancia horizontal máxima respecto a la cámara principal para activarse.")]
        [SerializeField] protected float activationDistance = 16f;

        // Referencias a componentes protegidos
        protected Rigidbody2D rb;
        protected Collider2D col;
        protected SpriteRenderer spriteRenderer;
        protected Animator animator;

        // Variables de estado
        protected bool isGrounded = false;
        protected bool isActivated = false;
        private readonly RaycastHit2D[] wallHits = new RaycastHit2D[8];
        private float lastTurnTime = float.NegativeInfinity;
        private static PhysicsMaterial2D slideMaterial;

        // Propiedades públicas
        public bool IsAlive => isAlive;
        public float Speed { get => speed; set => speed = value; }
        public Vector2 Direction => direction;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            animator = GetComponentInChildren<Animator>();
            if (slideMaterial == null)
                slideMaterial = new PhysicsMaterial2D("Enemy frictionless") { friction = 0, bounciness = 0 };
            col.sharedMaterial = slideMaterial;

            // Configurar Rigidbody2D para física 2D estándar
            if (rb != null)
            {
                rb.freezeRotation = true;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }

            // Capas por defecto si no están asignadas
            if (groundLayer.value == 0)
            {
                int groundMask = LayerMask.GetMask("Ground");
                groundLayer = groundMask != 0 ? groundMask : LayerMask.GetMask("Default");
            }
            if (wallLayer.value == 0)
            {
                wallLayer = groundLayer;
            }
        }

        protected virtual void Start()
        {
            // Normalizar dirección horizontal inicial (-1 o 1)
            direction.x = direction.x >= 0f ? 1f : -1f;
            direction.y = 0f;

            UpdateSpriteDirection();
        }

        protected virtual void Update()
        {
            if (!isAlive) return;

            // En Mario NES, los enemigos despiertan cuando la cámara se acerca a ellos
            if (!isActivated)
            {
                CheckCameraActivation();
                return;
            }

            // Actualizar orientación del sprite
            UpdateSpriteDirection();
        }

        protected virtual void FixedUpdate()
        {
            if (!isAlive || !isActivated) return;

            CheckGrounded();
            CheckWalls();
            Move();
        }

        /// <summary>
        /// Comprueba si la cámara principal está lo suficientemente cerca para activar el enemigo.
        /// </summary>
        protected virtual void CheckCameraActivation()
        {
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                isActivated = true;
                return;
            }

            float dist = Mathf.Abs(mainCam.transform.position.x - transform.position.x);
            if (dist <= activationDistance)
            {
                isActivated = true;
            }
        }

        /// <summary>
        /// Aplica la velocidad horizontal al Rigidbody2D manteniendo la velocidad vertical por gravedad.
        /// </summary>
        protected virtual void Move()
        {
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y);
            }
        }

        /// <summary>
        /// Invierte la dirección horizontal del enemigo (e.g. al chocar contra un muro u obstáculo).
        /// </summary>
        public virtual void FlipDirection()
        {
            if (lastTurnTime == Time.fixedTime) return;
            lastTurnTime = Time.fixedTime;
            direction.x = -direction.x;
            if (rb != null && isAlive && isActivated)
                rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y);
            UpdateSpriteDirection();
        }

        /// <summary>
        /// Actualiza el volteo horizontal del SpriteRenderer según la dirección de movimiento.
        /// </summary>
        protected virtual void UpdateSpriteDirection()
        {
            if (spriteRenderer != null)
            {
                // En sprites que miran a la izquierda por defecto (como Koopa):
                // Si va a la derecha (direction.x > 0), flipX = true.
                spriteRenderer.flipX = direction.x > 0f;
            }
        }

        /// <summary>
        /// Verifica si el enemigo está en contacto con el suelo mediante OverlapCircle.
        /// </summary>
        protected virtual void CheckGrounded()
        {
            Vector2 checkPos = groundCheck != null
                ? (Vector2)groundCheck.position
                : (Vector2)transform.position + Vector2.down * (col.bounds.extents.y + 0.05f);

            isGrounded = Physics2D.OverlapCircle(checkPos, groundCheckRadius, groundLayer);
        }

        /// <summary>
        /// Comprueba con un raycast frontal si hay una pared sólida en el camino.
        /// </summary>
        protected virtual void CheckWalls()
        {
            if (col == null || !col.enabled) return;
            LayerMask mask = wallLayer != 0 ? wallLayer : groundLayer;
            var filter = new ContactFilter2D();
            filter.SetLayerMask(mask);
            filter.useTriggers = false;
            Vector2 forward = Vector2.right * Mathf.Sign(direction.x);
            // Barrer desde el borde del collider; el antiguo rayo de 0.3 unidades
            // terminaba dentro del propio Goomba (semiancho 0.425).
            Bounds bounds = col.bounds;
            // Leave clearance above and below the probe so a low ceiling or floor
            // is not interpreted as a wall through an initial-overlap cast hit.
            Vector2 probeSize = new Vector2(Mathf.Max(.05f, bounds.size.x - .08f), Mathf.Max(.05f, bounds.size.y - .18f));
            int count = Physics2D.BoxCast(bounds.center, probeSize, 0, forward, filter, wallHits,
                Mathf.Max(.06f, speed * Time.fixedDeltaTime + .04f));
            for (int i = 0; i < count; i++)
            {
                if (wallHits[i].collider != col && wallHits[i].distance > .001f && Vector2.Dot(wallHits[i].normal, forward) < -.7f)
                {
                    FlipDirection();
                    return;
                }
            }
        }

        protected virtual void OnCollisionStay2D(Collision2D collision)
        {
            if (!isAlive || !isActivated || collision.gameObject.GetComponent<PlayerHealth>() != null) return;
            // Recuperar también contactos persistentes, no solo el primer impacto.
            // Una normal detrás del enemigo no vuelve a invertir su dirección.
            foreach (var contact in collision.contacts)
            {
                if (contact.normal.x * direction.x < -.7f && Mathf.Abs(contact.normal.y) < .3f)
                {
                    FlipDirection();
                    break;
                }
            }
        }

        /// <summary>
        /// Comprueba si el jugador impactó al enemigo cayendo desde arriba (stomp).
        /// Revisa que la velocidad vertical de Mario sea menor a cero y que sus pies estén por encima del centro.
        /// </summary>
        /// <param name="collision">Información de la colisión física.</param>
        /// <returns>True si el jugador realizó un pisotón válido desde arriba.</returns>
        protected virtual bool IsPlayerStomping(Collision2D collision)
        {
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            Collider2D playerCol = collision.collider;

            if (playerRb == null || playerCol == null) return false;

            // El jugador debe estar cayendo (velocidad vertical negativa)
            bool isFalling = playerRb.linearVelocity.y < 0.1f;

            // La base del collider del jugador debe estar por encima del centro del enemigo
            float playerFeetY = playerCol.bounds.min.y;
            float enemyCenterY = col != null ? col.bounds.center.y : transform.position.y;
            bool isAboveCenter = playerFeetY >= enemyCenterY - 0.1f;

            // También comprobar los puntos de contacto de la colisión: la normal debe apuntar hacia arriba
            bool normalIsUp = false;
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f) // Normal relativa apunta hacia abajo desde el jugador
                {
                    normalIsUp = true;
                    break;
                }
            }

            return isFalling && (isAboveCenter || normalIsUp);
        }

        /// <summary>
        /// Hace rebotar al jugador verticalmente al realizar un pisotón exitoso.
        /// </summary>
        /// <param name="player">GameObject del jugador.</param>
        protected virtual void BouncePlayer(GameObject player)
        {
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                // Si mantiene presionada la tecla de salto (Espacio), rebota más alto estilo Mario NES
                float force = MarioInput.JumpHeld ? stompBounceForceHigh : stompBounceForce;
                playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, force);
            }
        }

        /// <summary>
        /// Método invocado al ser aplastado por el jugador desde arriba.
        /// </summary>
        /// <param name="player">GameObject del jugador.</param>
        public abstract void OnPlayerStomp(GameObject player);

        /// <summary>
        /// Método invocado al tocar al jugador desde un lateral o por debajo.
        /// </summary>
        /// <param name="player">GameObject del jugador.</param>
        public virtual void OnPlayerContact(GameObject player)
        {
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null && !health.IsInvincible && !health.IsDead)
            {
                health.TakeDamage();
            }
        }

        /// <summary>
        /// Método abstracto para la muerte general del enemigo.
        /// </summary>
        public abstract void Die();

        /// <summary>
        /// Muerte lanzada con impulso (e.g. cuando es golpeado por un caparazón de Koopa o estrella).
        /// Voltea el sprite boca abajo y lo hace caer fuera de la pantalla sin colisiones.
        /// </summary>
        /// <param name="hitDirection">Dirección del impacto horizontal.</param>
        public virtual void Die(Vector2 hitDirection)
        {
            if (!isAlive) return;
            isAlive = false;

            if (col != null) col.enabled = false;

            if (rb != null)
            {
                rb.linearVelocity = new Vector2(Mathf.Sign(hitDirection.x) * 3f, 7f);
                rb.gravityScale = 3f;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.flipY = true;
            }

            Destroy(gameObject, 2.5f);
        }

        /// <summary>
        /// Manejador de colisiones para detectar jugador, paredes u otros enemigos.
        /// </summary>
        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            if (!isAlive) return;

            // Colisión con el jugador
            if (collision.gameObject.CompareTag("Player") || collision.gameObject.GetComponent<PlayerHealth>() != null)
            {
                if (IsPlayerStomping(collision))
                {
                    OnPlayerStomp(collision.gameObject);
                }
                else
                {
                    OnPlayerContact(collision.gameObject);
                }
                return;
            }

            // Colisión frontal con otros enemigos: ambos cambian de dirección
            EnemyBase otherEnemy = collision.gameObject.GetComponent<EnemyBase>();
            if (otherEnemy is KoopaController shell && shell.CurrentState == KoopaState.ShellMoving)
            {
                Die(shell.Direction);
                return;
            }
            if (otherEnemy != null && otherEnemy.isAlive)
            {
                // Solo si el contacto es predominantemente horizontal
                foreach (ContactPoint2D contact in collision.contacts)
                {
                    if (Mathf.Abs(contact.normal.x) > 0.5f)
                    {
                        FlipDirection();
                        break;
                    }
                }
                return;
            }

            // Colisión contra muros mediante puntos de contacto
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (Mathf.Abs(contact.normal.x) > 0.5f && Mathf.Sign(contact.normal.x) != Mathf.Sign(direction.x))
                {
                    FlipDirection();
                    break;
                }
            }
        }

        /// <summary>
        /// Optimización: Desactiva o destruye el enemigo al salir del campo visual de la cámara.
        /// </summary>
        protected virtual void OnBecameInvisible()
        {
            if (!deactivateWhenOffscreen) return;

            // Si ya fue activado previamente y cae fuera de cámara
            if (isActivated)
            {
                // Si cayó al vacío o quedó muy atrás a la izquierda de la cámara
                Camera cam = Camera.main;
                if (cam != null && transform.position.x < cam.transform.position.x - 4f)
                {
                    gameObject.SetActive(false);
                }
            }
        }

        protected virtual void OnDrawGizmosSelected()
        {
            // Chequeo de suelo
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Vector2 checkPos = groundCheck != null
                ? (Vector2)groundCheck.position
                : (Vector2)transform.position + Vector2.down * (col != null ? col.bounds.extents.y + 0.05f : 0.5f);
            Gizmos.DrawWireSphere(checkPos, groundCheckRadius);

            // Chequeo de pared
            Gizmos.color = Color.cyan;
            Vector2 rayOrigin = (Vector2)transform.position + Vector2.up * 0.1f;
            Vector2 dir = direction.x >= 0 ? Vector2.right : Vector2.left;
            Gizmos.DrawLine(rayOrigin, rayOrigin + dir * wallCheckDistance);
        }
    }
}
