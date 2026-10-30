using System.Collections;
using UnityEngine;
using MarioBros.Player;

namespace MarioBros.Enemies
{
    /// <summary>
    /// Estados posibles del Koopa Troopa.
    /// </summary>
    public enum KoopaState
    {
        Walking,     // Patrullaje normal caminando
        Shell,       // Escondido dentro del caparazón (inmóvil)
        ShellMoving  // Caparazón deslizándose a alta velocidad tras ser pateado
    }

    /// <summary>
    /// Controlador del enemigo Koopa Troopa al estilo clásico de NES.
    /// Patrulla como Goomba; al ser pisado entra en su caparazón reduciendo su collider.
    /// Estando en caparazón, Mario puede patearlo. Al moverse a gran velocidad (10f),
    /// rebota contra paredes y elimina a otros enemigos en su camino.
    /// Si Mario lo pisa mientras se desplaza a alta velocidad, lo detiene.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class KoopaController : EnemyBase
    {
        [Header("Estados Koopa")]
        [Tooltip("Estado actual del Koopa.")]
        [SerializeField] private KoopaState currentState = KoopaState.Walking;

        [Tooltip("Velocidad normal de patrullaje caminando.")]
        [SerializeField] private float walkSpeed = 2f;

        [Tooltip("Velocidad del caparazón al ser pateado.")]
        [SerializeField] private float shellSpeed = 10f;

        [Header("Dimensiones del Collider (PPU = 16)")]
        [Tooltip("Tamaño del BoxCollider2D en estado caminando (aprox 16x24 px = 1x1.5 unidades).")]
        [SerializeField] private Vector2 walkingColliderSize = new Vector2(0.85f, 1.4f);

        [Tooltip("Offset del BoxCollider2D en estado caminando.")]
        [SerializeField] private Vector2 walkingColliderOffset = new Vector2(0f, 0.7f);

        [Tooltip("Tamaño del BoxCollider2D en estado de caparazón (aprox 16x16 px = 0.85x0.85 unidades).")]
        [SerializeField] private Vector2 shellColliderSize = new Vector2(0.85f, 0.85f);

        [Tooltip("Offset del BoxCollider2D en estado de caparazón.")]
        [SerializeField] private Vector2 shellColliderOffset = new Vector2(0f, 0.425f);

        [Header("Parámetros del Animator")]
        [Tooltip("Nombre del parámetro booleano para indicar si está en caparazón.")]
        [SerializeField] private string isShellBoolParam = "isShell";

        [Tooltip("Nombre del parámetro booleano para indicar si el caparazón está en movimiento.")]
        [SerializeField] private string isMovingBoolParam = "isMoving";

        [Header("Despertar del Caparazón")]
        [Tooltip("Tiempo en segundos que permanece en caparazón antes de volver a caminar (-1 para desactivar).")]
        [SerializeField] private float shellWakeUpTime = 5f;

        // Referencias internas y hashes
        private BoxCollider2D boxCol;
        private int isShellHash;
        private int isMovingHash;
        private Coroutine wakeUpCoroutine;
        private Sprite walkOne, walkTwo, shellSprite;
        private float kickGraceUntil;

        // Propiedad pública para consultar el estado
        public KoopaState CurrentState => currentState;

        protected override void Awake()
        {
            base.Awake();
            walkOne = Resources.Load<Sprite>("MarioFrames/Koopa_Walk1");
            walkTwo = Resources.Load<Sprite>("MarioFrames/Koopa_Walk2");
            shellSprite = Resources.Load<Sprite>("MarioFrames/Koopa_Shell");

            boxCol = GetComponent<BoxCollider2D>();
            speed = walkSpeed;

            isShellHash = Animator.StringToHash(isShellBoolParam);
            isMovingHash = Animator.StringToHash(isMovingBoolParam);
        }

        protected override void Start()
        {
            base.Start();

            // Configurar collider y animaciones según el estado inicial
            if (currentState == KoopaState.Walking)
            {
                ApplyWalkingCollider();
                UpdateAnimatorState(false, false);
            }
            else if (currentState == KoopaState.Shell)
            {
                ApplyShellCollider();
                UpdateAnimatorState(true, false);
            }
            else
            {
                ApplyShellCollider();
                UpdateAnimatorState(true, true);
                speed = shellSpeed;
            }
        }

        protected override void Update()
        {
            base.Update();
            if (spriteRenderer == null || (animator != null && animator.runtimeAnimatorController != null)) return;
            Sprite frame = currentState == KoopaState.Walking
                ? ((int)(Time.time * 6) % 2 == 0 ? walkOne : walkTwo)
                : shellSprite;
            if (frame != null) spriteRenderer.sprite = frame;
        }

        protected override void FixedUpdate()
        {
            if (!isAlive || !isActivated) return;

            CheckGrounded();

            if (currentState == KoopaState.Walking)
            {
                CheckWalls();
                Move();
            }
            else if (currentState == KoopaState.ShellMoving)
            {
                CheckWalls();
                MoveShell();
            }
            else // KoopaState.Shell (estacionario)
            {
                // Detener velocidad horizontal pero permitir gravedad vertical
                if (rb != null)
                {
                    rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                }
            }
        }

        /// <summary>
        /// Movimiento de traslación del caparazón a alta velocidad.
        /// </summary>
        private void MoveShell()
        {
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(direction.x * shellSpeed, rb.linearVelocity.y);
            }
        }

        /// <summary>
        /// Gestiona el pisotón de Mario sobre el Koopa según su estado actual.
        /// </summary>
        /// <param name="player">GameObject del jugador.</param>
        public override void OnPlayerStomp(GameObject player)
        {
            if (!isAlive) return;

            BouncePlayer(player);

            switch (currentState)
            {
                case KoopaState.Walking:
                    // De caminando pasa a caparazón inmóvil
                    EnterShellState();
                    break;

                case KoopaState.ShellMoving:
                    // Si el caparazón iba rápido, Mario lo frena al pisarlo
                    StopShell();
                    break;

                case KoopaState.Shell:
                    // Si ya estaba en caparazón inmóvil y Mario lo pisa, lo patea
                    Vector2 kickDir = player.transform.position.x < transform.position.x ? Vector2.right : Vector2.left;
                    KickShell(kickDir);
                    break;
            }
        }

        /// <summary>
        /// Contacto lateral o frontal con Mario.
        /// </summary>
        /// <param name="player">GameObject del jugador.</param>
        public override void OnPlayerContact(GameObject player)
        {
            if (!isAlive) return;

            switch (currentState)
            {
                case KoopaState.Walking:
                    // Tocar a un Koopa caminando hiere a Mario
                    DamagePlayer(player);
                    break;

                case KoopaState.Shell:
                    // Tocar un caparazón inmóvil lo patea en la dirección opuesta a Mario sin hacer daño
                    Vector2 kickDir = player.transform.position.x < transform.position.x ? Vector2.right : Vector2.left;
                    KickShell(kickDir);
                    break;

                case KoopaState.ShellMoving:
                    // Chocar con un caparazón en movimiento hiere a Mario
                    if (Time.time >= kickGraceUntil) DamagePlayer(player);
                    break;
            }
        }

        /// <summary>
        /// Transiciona al Koopa al estado de caparazón estacionario.
        /// </summary>
        public void EnterShellState()
        {
            currentState = KoopaState.Shell;
            speed = 0f;

            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }

            ApplyShellCollider();
            UpdateAnimatorState(true, false);

            // Programar despertar si está configurado
            if (shellWakeUpTime > 0f)
            {
                if (wakeUpCoroutine != null) StopCoroutine(wakeUpCoroutine);
                wakeUpCoroutine = StartCoroutine(WakeUpRoutine());
            }
        }

        /// <summary>
        /// Detiene el deslizamiento rápido del caparazón.
        /// </summary>
        public void StopShell()
        {
            currentState = KoopaState.Shell;
            speed = 0f;

            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }

            UpdateAnimatorState(true, false);

            if (shellWakeUpTime > 0f)
            {
                if (wakeUpCoroutine != null) StopCoroutine(wakeUpCoroutine);
                wakeUpCoroutine = StartCoroutine(WakeUpRoutine());
            }
        }

        /// <summary>
        /// Patea el caparazón en la dirección indicada a alta velocidad.
        /// </summary>
        /// <param name="kickDirection">Dirección horizontal del impulso.</param>
        public void KickShell(Vector2 kickDirection)
        {
            if (wakeUpCoroutine != null)
            {
                StopCoroutine(wakeUpCoroutine);
                wakeUpCoroutine = null;
            }

            currentState = KoopaState.ShellMoving;
            kickGraceUntil = Time.time + .15f;
            direction.x = kickDirection.x >= 0f ? 1f : -1f;
            speed = shellSpeed;

            UpdateAnimatorState(true, true);

            if (rb != null)
            {
                rb.linearVelocity = new Vector2(direction.x * shellSpeed, rb.linearVelocity.y);
            }
        }

        /// <summary>
        /// Corrutina para que el Koopa salga de su caparazón tras un periodo de inactividad.
        /// </summary>
        private IEnumerator WakeUpRoutine()
        {
            yield return new WaitForSeconds(shellWakeUpTime);

            // Do not expand into a ceiling while the shell is in a one-block tunnel.
            while (currentState == KoopaState.Shell && isAlive && Physics2D.OverlapBox(
                (Vector2)transform.position + walkingColliderOffset,
                walkingColliderSize - new Vector2(.08f,.08f), 0, groundLayer) != null)
                yield return new WaitForSeconds(.2f);

            if (currentState == KoopaState.Shell && isAlive)
            {
                currentState = KoopaState.Walking;
                speed = walkSpeed;
                ApplyWalkingCollider();
                UpdateAnimatorState(false, false);
                wakeUpCoroutine = null;
            }
        }

        /// <summary>
        /// Aplica dimensiones del BoxCollider2D para el cuerpo erguido del Koopa.
        /// </summary>
        private void ApplyWalkingCollider()
        {
            if (boxCol != null)
            {
                boxCol.size = walkingColliderSize;
                boxCol.offset = walkingColliderOffset;
            }
        }

        /// <summary>
        /// Aplica dimensiones reducidas del BoxCollider2D para la forma de caparazón.
        /// </summary>
        private void ApplyShellCollider()
        {
            if (boxCol != null)
            {
                boxCol.size = shellColliderSize;
                boxCol.offset = shellColliderOffset;
            }
        }

        /// <summary>
        /// Actualiza los parámetros del Animator asociados al Koopa.
        /// </summary>
        private void UpdateAnimatorState(bool isShell, bool isMoving)
        {
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetBool(isShellHash, isShell);
                animator.SetBool(isMovingHash, isMoving);
            }
        }

        /// <summary>
        /// Inflige daño al jugador llamando a PlayerHealth.TakeDamage().
        /// </summary>
        private void DamagePlayer(GameObject player)
        {
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null && !health.IsInvincible && !health.IsDead)
            {
                health.TakeDamage();
            }
        }

        /// <summary>
        /// Manejador extendido de colisiones para Koopa:
        /// - Caparazón en movimiento destruye a otros enemigos.
        /// - Caparazón en movimiento rebota en paredes sólidas.
        /// </summary>
        protected override void OnCollisionEnter2D(Collision2D collision)
        {
            if (!isAlive) return;

            // 1. Interacción especial con otros enemigos cuando el caparazón se desliza
            if (currentState == KoopaState.ShellMoving)
            {
                EnemyBase otherEnemy = collision.gameObject.GetComponent<EnemyBase>();
                if (otherEnemy != null && otherEnemy != this && otherEnemy.IsAlive)
                {
                    // Elimina al otro enemigo lanzándolo con impulso
                    otherEnemy.Die(direction);
                    return;
                }
            }

            // 2. Interacción con el jugador
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

            // 3. Rebote contra paredes al deslizarse
            if (currentState == KoopaState.ShellMoving)
            {
                foreach (ContactPoint2D contact in collision.contacts)
                {
                    // Si el impacto tiene componente horizontal y choca de frente
                    if (Mathf.Abs(contact.normal.x) > 0.5f && Mathf.Sign(contact.normal.x) != Mathf.Sign(direction.x))
                    {
                        FlipDirection();
                        if (rb != null)
                        {
                            rb.linearVelocity = new Vector2(direction.x * shellSpeed, rb.linearVelocity.y);
                        }
                        break;
                    }
                }
                return;
            }

            // 4. Comportamiento base para modo Walking
            base.OnCollisionEnter2D(collision);
        }

        /// <summary>
        /// Muerte estándar del Koopa (por ejemplo, al ser alcanzado por otro caparazón o fuego).
        /// </summary>
        public override void Die()
        {
            if (!isAlive) return;
            isAlive = false;

            if (wakeUpCoroutine != null)
            {
                StopCoroutine(wakeUpCoroutine);
            }

            if (boxCol != null) boxCol.enabled = false;
            if (col != null) col.enabled = false;

            if (rb != null)
            {
                rb.linearVelocity = new Vector2(direction.x * 2f, 6f);
                rb.gravityScale = 3f;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.flipY = true;
            }

            Destroy(gameObject, 2.5f);
        }
    }
}
