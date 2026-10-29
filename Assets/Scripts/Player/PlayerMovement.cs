using UnityEngine;

namespace MarioBros.Player
{
    /// <summary>
    /// Controlador principal de movimiento para Mario al estilo clásico de NES (Super Mario Bros).
    /// Maneja aceleración, desaceleración, derrape, salto variable, detección de suelo/paredes
    /// y caída al vacío.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Configuración de Velocidad")]
        [Tooltip("Velocidad al caminar (unidades por segundo).")]
        [SerializeField] private float walkSpeed = 5f;

        [Tooltip("Velocidad máxima al correr con Shift izquierdo.")]
        [SerializeField] private float runSpeed = 8f;

        [Tooltip("Aceleración horizontal para alcanzar la velocidad objetivo.")]
        [SerializeField] private float acceleration = 25f;

        [Tooltip("Desaceleración cuando no se presiona ninguna tecla de dirección.")]
        [SerializeField] private float deceleration = 20f;

        [Tooltip("Fuerza de desaceleración al cambiar de dirección abruptamente (derrape).")]
        [SerializeField] private float skidDeceleration = 35f;

        [Header("Configuración de Salto")]
        [Tooltip("Fuerza inicial del impulso de salto.")]
        [SerializeField] private float jumpForce = 12f;

        [Tooltip("Tiempo máximo en segundos que se puede mantener presionado el salto para ganar altura.")]
        [SerializeField] private float maxJumpTime = 0.3f;

        [Tooltip("Escala de gravedad para una física de salto responsiva y pesada estilo Mario.")]
        [SerializeField] private float gravityScale = 3f;

        [Tooltip("Factor de corte de velocidad vertical al soltar la tecla de salto prematuramente.")]
        [Range(0f, 1f)]
        [SerializeField] private float jumpCutMultiplier = 0.5f;

        [Header("Detección de Suelo")]
        [Tooltip("Transform que marca el punto de chequeo en los pies de Mario.")]
        [SerializeField] private Transform groundCheck;

        [Tooltip("Radio del círculo de detección de suelo.")]
        [SerializeField] private float groundCheckRadius = 0.2f;

        [Tooltip("Capas consideradas como suelo sólido.")]
        [SerializeField] private LayerMask groundLayer;

        [Header("Detección de Paredes")]
        [Tooltip("Distancia frontal del raycast para evitar atascos contra paredes.")]
        [SerializeField] private float wallCheckDistance = 0.5f;

        [Tooltip("Capas consideradas como paredes (por defecto toma groundLayer si no se asigna).")]
        [SerializeField] private LayerMask wallLayer;

        [Header("Límite de Caída")]
        [Tooltip("Posición Y por debajo de la cual Mario muere instantáneamente.")]
        [SerializeField] private float fallDeathY = -5f;

        [Header("Referencias Opcionales")]
        [Tooltip("SpriteRenderer de Mario para voltear dirección horizontal.")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Tooltip("Animator de Mario para actualizar parámetros de animación.")]
        [SerializeField] private Animator animator;

        // Componentes internos
        private Rigidbody2D rb;
        private Collider2D col;
        private PlayerHealth playerHealth;

        // Variables de estado
        private float horizontalInput;
        private bool isRunning;
        private bool isGrounded;
        private bool isJumping;
        private float jumpTimeCounter;
        private bool isFacingRight = true;
        private bool isDead = false;
        [SerializeField] private bool underwater;
        public bool IsUnderwater => underwater;

        // Propiedades públicas para PlayerAnimation y otros sistemas
        public float CurrentSpeed => Mathf.Abs(rb != null ? rb.linearVelocity.x : 0f);
        public bool IsGrounded => isGrounded;
        public bool IsJumping => !isGrounded && rb != null && rb.linearVelocity.y > 0.1f;
        public bool IsFalling => !isGrounded && rb != null && rb.linearVelocity.y < -0.1f;
        public bool IsFacingRight => isFacingRight;
        public bool IsDead => isDead;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            playerHealth = GetComponent<PlayerHealth>();

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            // Configurar Rigidbody2D
            rb.freezeRotation = true;
            rb.gravityScale = underwater ? 0.45f : gravityScale;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Capas por defecto si no están asignadas en Inspector
            if (groundLayer.value == 0)
            {
                int groundMask = LayerMask.GetMask("Ground");
                groundLayer = groundMask != 0 ? groundMask : LayerMask.GetMask("Default");
            }
            if (wallLayer.value == 0)
            {
                wallLayer = groundLayer;
            }

            // Crear y asignar material de física sin fricción para evitar que Mario se pegue a las paredes
            PhysicsMaterial2D frictionless = new PhysicsMaterial2D("MarioFrictionless")
            {
                friction = 0f,
                bounciness = 0f
            };
            col.sharedMaterial = frictionless;
        }

        private void Update()
        {
            if (isDead) return;

            // 1. Chequeo de caída al vacío
            CheckFallDeath();

            // 2. Procesar entrada del jugador (Old Input System con KeyCode)
            ProcessInput();

            // 3. Manejo de salto variable
            HandleJump();

            // 4. Voltear sprite según dirección
            HandleSpriteFlip();

            // 5. Actualizar parámetros en Animator si está presente
            UpdateAnimator();
        }

        private void FixedUpdate()
        {
            if (isDead) return;

            // Chequeo de suelo mediante Physics2D.OverlapCircle
            CheckGrounded();

            // Movimiento horizontal con aceleración y desaceleración
            HandleHorizontalMovement();
        }

        /// <summary>
        /// Captura la entrada del usuario usando KeyCode (A/D, flechas, Shift, Espacio).
        /// </summary>
        private void ProcessInput()
        {
            horizontalInput = 0f;

            if (MarioInput.Left)
            {
                horizontalInput -= 1f;
            }
            if (MarioInput.Right)
            {
                horizontalInput += 1f;
            }

            // Correr al mantener Shift o tecla de acción
            isRunning = MarioInput.Run;
        }

        /// <summary>
        /// Manejo del salto de altura variable:
        /// - Pulsación corta = salto pequeño.
        /// - Mantener presionado Espacio = salto completo hasta maxJumpTime.
        /// </summary>
        private void HandleJump()
        {
            if (underwater)
            {
                if (MarioInput.JumpDown)
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 4.5f);
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(-2.5f, rb.linearVelocity.y));
                return;
            }
            // Inicio del salto al presionar Espacio estando en el suelo
            if (MarioInput.JumpDown && isGrounded)
            {
                isJumping = true;
                jumpTimeCounter = maxJumpTime;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                isGrounded = false;
            }

            // Continuar impulsando mientras se mantenga presionada la tecla Espacio
            if (MarioInput.JumpHeld && isJumping)
            {
                if (jumpTimeCounter > 0f)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                    jumpTimeCounter -= Time.deltaTime;
                }
                else
                {
                    isJumping = false;
                }
            }

            // Al soltar la tecla de salto prematuramente, cortar la velocidad vertical ascendente
            if (MarioInput.JumpUp)
            {
                isJumping = false;
                if (rb.linearVelocity.y > 0f)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
                }
            }
        }

        /// <summary>
        /// Aplica la aceleración/desaceleración horizontal y previene empuje continuo contra paredes.
        /// </summary>
        private void HandleHorizontalMovement()
        {
            float targetSpeed = horizontalInput * (underwater ? 3.2f : (isRunning ? runSpeed : walkSpeed));

            // Detección de colisión con pared frontal para detener la velocidad contra la pared
            if (Mathf.Abs(horizontalInput) > 0.01f && IsTouchingWall(horizontalInput))
            {
                targetSpeed = 0f;
            }

            float currentSpeedX = rb.linearVelocity.x;
            float accelRate;

            if (Mathf.Abs(horizontalInput) > 0.01f)
            {
                // Si el jugador intenta moverse en dirección contraria a su velocidad actual (derrape)
                bool isSkidding = (Mathf.Sign(horizontalInput) != Mathf.Sign(currentSpeedX)) && Mathf.Abs(currentSpeedX) > 0.5f;
                accelRate = isSkidding ? skidDeceleration : acceleration;
            }
            else
            {
                // Desaceleración por inercia hasta detenerse
                accelRate = deceleration;
            }

            float newSpeedX = Mathf.MoveTowards(currentSpeedX, targetSpeed, accelRate * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(newSpeedX, rb.linearVelocity.y);
        }

        /// <summary>
        /// Detecta si Mario está tocando el suelo mediante un OverlapCircle.
        /// </summary>
        private void CheckGrounded()
        {
            Vector2 checkPosition = groundCheck != null 
                ? (Vector2)groundCheck.position 
                : (Vector2)transform.position + Vector2.down * 0.5f;

            bool wasGrounded = isGrounded;
            isGrounded = rb.linearVelocity.y <= 0.05f && Physics2D.OverlapCircle(checkPosition, groundCheckRadius, groundLayer);

            // Si aterriza en el suelo, reiniciar estado de salto
            if (isGrounded && rb.linearVelocity.y <= 0.05f)
            {
                isJumping = false;
            }
        }

        /// <summary>
        /// Comprueba si hay una pared directamente frente a Mario en la dirección de movimiento.
        /// </summary>
        private bool IsTouchingWall(float direction)
        {
            LayerMask mask = wallLayer != 0 ? wallLayer : groundLayer;
            Vector2 rayOrigin = (Vector2)transform.position + Vector2.up * 0.5f;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * Mathf.Sign(direction), wallCheckDistance, mask);

            return hit.collider != null && hit.collider.gameObject != gameObject;
        }

        /// <summary>
        /// Invierte la orientación horizontal del sprite según el movimiento.
        /// </summary>
        private void HandleSpriteFlip()
        {
            if (horizontalInput > 0.01f)
            {
                isFacingRight = true;
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = false;
                }
            }
            else if (horizontalInput < -0.01f)
            {
                isFacingRight = false;
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = true;
                }
            }
        }

        /// <summary>
        /// Actualiza los parámetros del Animator asociados al movimiento.
        /// </summary>
        private void UpdateAnimator()
        {
            if (animator == null || animator.runtimeAnimatorController == null) return;

            animator.SetFloat("speed", CurrentSpeed);
            animator.SetBool("isJumping", IsJumping);
            animator.SetBool("isFalling", IsFalling);
            animator.SetBool("isGrounded", isGrounded);
        }

        /// <summary>
        /// Verifica si Mario ha caído por debajo de la coordenada Y establecida como muerte.
        /// </summary>
        private void CheckFallDeath()
        {
            if (transform.position.y < fallDeathY)
            {
                TriggerDeath();
            }
        }

        /// <summary>
        /// Provoca la muerte de Mario desactivando el control y notificando a PlayerHealth.
        /// </summary>
        public void TriggerDeath()
        {
            if (isDead) return;

            isDead = true;

            if (playerHealth != null)
            {
                playerHealth.Die();
            }
            else
            {
                // Si no hay PlayerHealth, detener física y recargar escena tras un breve tiempo
                rb.linearVelocity = Vector2.zero;
                rb.isKinematic = true;
                col.enabled = false;
            }
        }

        /// <summary>
        /// Detiene completamente la velocidad del Rigidbody2D.
        /// </summary>
        public void StopMovement()
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Dibujar círculo de chequeo de suelo
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Vector2 checkPosition = groundCheck != null 
                ? (Vector2)groundCheck.position 
                : (Vector2)transform.position + Vector2.down * 0.5f;

            Gizmos.DrawWireSphere(checkPosition, groundCheckRadius);

            // Dibujar raycast de detección de pared
            Gizmos.color = Color.blue;
            Vector2 rayOrigin = (Vector2)transform.position + Vector2.up * 0.5f;
            Gizmos.DrawLine(rayOrigin, rayOrigin + Vector2.right * (isFacingRight ? 1f : -1f) * wallCheckDistance);

            // Dibujar línea de muerte por caída
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(new Vector3(-50f, fallDeathY, 0f), new Vector3(50f, fallDeathY, 0f));
        }
    }
}
