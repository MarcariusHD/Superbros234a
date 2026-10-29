using UnityEngine;

namespace MarioBros.Player
{
    /// <summary>
    /// Administrador de estados de animación para Mario.
    /// Sincroniza parámetros del Animator ('speed', 'isJumping', 'isFalling', 'isGrounded', 'isBig')
    /// a partir del estado de PlayerMovement y gestiona los triggers de crecimiento, encogimiento y muerte.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerAnimation : MonoBehaviour
    {
        [Header("Referencias de Componentes")]
        [Tooltip("Componente Animator encargado de las transiciones de animación.")]
        [SerializeField] private Animator animator;

        [Tooltip("Componente SpriteRenderer para renderizado y volteo de sprites.")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Tooltip("Referencia al controlador de movimiento de Mario.")]
        [SerializeField] private PlayerMovement playerMovement;

        [Header("Estado de Mario")]
        [Tooltip("Indica si Mario se encuentra actualmente en estado Grande (Super Mario / Fire Mario).")]
        [SerializeField] private bool isBig = false;
        private Sprite[] smallRun, bigRun, smallSwim, bigSwim;
        private Sprite smallIdle, bigIdle, smallJump, bigJump, deathSprite;
        private bool dead;
        private float frameClock;
        private bool HasAnimator => animator != null && animator.runtimeAnimatorController != null;

        // Hashes optimizados para los parámetros del Animator
        private static readonly int SpeedHash = Animator.StringToHash("speed");
        private static readonly int IsJumpingHash = Animator.StringToHash("isJumping");
        private static readonly int IsFallingHash = Animator.StringToHash("isFalling");
        private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");
        private static readonly int IsBigHash = Animator.StringToHash("isBig");
        private static readonly int DieTriggerHash = Animator.StringToHash("die");
        private static readonly int DeathTriggerHash = Animator.StringToHash("death");
        private static readonly int GrowTriggerHash = Animator.StringToHash("grow");
        private static readonly int ShrinkTriggerHash = Animator.StringToHash("shrink");

        /// <summary>
        /// Propiedad para consultar o modificar el estado 'isBig'.
        /// </summary>
        public bool IsBig
        {
            get => isBig;
            set => SetBig(value);
        }

        private void Awake()
        {
            smallIdle = Frame("Small_Idle"); bigIdle = Frame("Big_Idle");
            smallJump = Frame("Small_Jump"); bigJump = Frame("Big_Jump");
            deathSprite = Frame("Small_Die");
            smallRun = new[] { Frame("Small_Run1"), Frame("Small_Run2"), Frame("Small_Run3") };
            bigRun = new[] { Frame("Big_Run1"), Frame("Big_Run2"), Frame("Big_Run3") };
            smallSwim = new[] { Frame("Small_Swim1"), Frame("Small_Swim2") };
            bigSwim = new[] { Frame("Big_Swim1"), Frame("Big_Swim2") };
            if (animator == null)
            {
                animator = GetComponent<Animator>();
                if (animator == null)
                {
                    animator = GetComponentInChildren<Animator>();
                }
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                }
            }

            if (playerMovement == null)
            {
                playerMovement = GetComponent<PlayerMovement>();
            }
        }

        private void Start()
        {
            // Sincronizar estado inicial en el Animator
            if (HasAnimator)
            {
                animator.SetBool(IsBigHash, isBig);
            }
        }

        private void Update()
        {
            UpdateAnimationParameters();
        }

        /// <summary>
        /// Actualiza continuamente los parámetros del Animator a partir del estado de PlayerMovement.
        /// </summary>
        private void UpdateAnimationParameters()
        {
            // Voltear sprite según la dirección en que mira Mario
            if (playerMovement != null && spriteRenderer != null)
            {
                spriteRenderer.flipX = !playerMovement.IsFacingRight;
            }

            if (!HasAnimator)
            {
                UpdateSpriteFrames();
                return;
            }

            if (playerMovement != null)
            {
                animator.SetFloat(SpeedHash, playerMovement.CurrentSpeed);
                animator.SetBool(IsJumpingHash, playerMovement.IsJumping);
                animator.SetBool(IsFallingHash, playerMovement.IsFalling);
                animator.SetBool(IsGroundedHash, playerMovement.IsGrounded);
            }

            animator.SetBool(IsBigHash, isBig);
        }

        /// <summary>
        /// Define si Mario es Grande (Super Mario / Fuego) o Pequeño y actualiza el Animator.
        /// </summary>
        /// <param name="big">True si es grande, False si es pequeño.</param>
        public void SetBig(bool big)
        {
            isBig = big;
            if (HasAnimator)
            {
                animator.SetBool(IsBigHash, isBig);
            }
        }

        /// <summary>
        /// Dispara la animación de crecimiento (al tomar un super champiñón o flor).
        /// </summary>
        public void TriggerGrow()
        {
            SetBig(true);
            if (HasAnimator)
            {
                animator.SetTrigger(GrowTriggerHash);
            }
        }

        /// <summary>
        /// Dispara la animación de encogimiento (al recibir daño siendo grande/fuego).
        /// </summary>
        public void TriggerShrink()
        {
            SetBig(false);
            if (HasAnimator)
            {
                animator.SetTrigger(ShrinkTriggerHash);
            }
        }

        /// <summary>
        /// Dispara la animación de muerte de Mario.
        /// Se envían los triggers 'die' y 'death' para compatibilidad con cualquier Animator Controller.
        /// </summary>
        public void TriggerDeath()
        {
            dead = true;
            if (!HasAnimator && spriteRenderer != null && deathSprite != null)
                spriteRenderer.sprite = deathSprite;
            if (HasAnimator)
            {
                animator.SetTrigger(DieTriggerHash);
                animator.SetTrigger(DeathTriggerHash);
                animator.SetBool("isDead", true);
            }
        }

        private static Sprite Frame(string name) => Resources.Load<Sprite>("MarioFrames/" + name);

        private void UpdateSpriteFrames()
        {
            if (spriteRenderer == null || playerMovement == null) return;
            Sprite next;
            if (dead) next = deathSprite;
            else if (playerMovement.IsUnderwater)
            {
                frameClock += Time.deltaTime * 5;
                var frames = isBig ? bigSwim : smallSwim;
                next = frames[(int)frameClock % frames.Length];
            }
            else if (!playerMovement.IsGrounded)
                next = isBig ? bigJump : smallJump;
            else if (playerMovement.CurrentSpeed > .15f)
            {
                frameClock += Time.deltaTime * Mathf.Lerp(5, 14, playerMovement.CurrentSpeed / 8);
                var frames = isBig ? bigRun : smallRun;
                next = frames[(int)frameClock % frames.Length];
            }
            else { frameClock = 0; next = isBig ? bigIdle : smallIdle; }
            if (next != null) spriteRenderer.sprite = next;
        }
    }
}
