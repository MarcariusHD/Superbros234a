using UnityEngine;
using MarioBros.Player;

namespace MarioBros.Enemies
{
    /// <summary>
    /// Controlador del enemigo Goomba al estilo NES Super Mario Bros.
    /// Patrulla de lado a lado cambiando de sentido contra paredes u otros enemigos.
    /// Al ser pisado desde arriba, activa la animación de aplastado, desactiva colisiones
    /// y se destruye tras 0.5 segundos. Si toca a Mario lateralmente, le causa daño.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class Goomba : EnemyBase
    {
        [Header("Configuración Goomba")]
        [Tooltip("Tiempo en segundos que el Goomba permanece aplastado antes de desaparecer.")]
        [SerializeField] private float squishDuration = 0.5f;

        [Tooltip("Nombre del parámetro booleano en el Animator para la animación de aplastamiento.")]
        [SerializeField] private string squishedBoolParam = "isSquished";

        [Tooltip("Distancia máxima respecto a la cámara tras la cual se desactiva para optimización.")]
        [SerializeField] private float maxCameraDistance = 25f;

        // Hash del parámetro de animación para mejor rendimiento
        private int squishedHash;
        private BoxCollider2D boxCol;
        private Sprite walkOne, walkTwo, squishSprite;

        protected override void Awake()
        {
            base.Awake();
            string palette = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Underground" ? "Underground" : "Overworld";
            string prefix = "MarioFrames/Goomba_" + palette;
            walkOne = Resources.Load<Sprite>(prefix + "_Walk1");
            walkTwo = Resources.Load<Sprite>(prefix + "_Walk2");
            squishSprite = Resources.Load<Sprite>(prefix + "_Squish");
            if (spriteRenderer != null) spriteRenderer.color = Color.white;

            boxCol = GetComponent<BoxCollider2D>();
            speed = 2f; // Velocidad de caminata estándar ~2f
            squishedHash = Animator.StringToHash(squishedBoolParam);

            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.freezeRotation = true;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (spriteRenderer != null && (animator == null || animator.runtimeAnimatorController == null))
            {
                Sprite frame = !isAlive ? squishSprite : ((int)(Time.time * 6) % 2 == 0 ? walkOne : walkTwo);
                if (frame != null) spriteRenderer.sprite = frame;
            }

            if (!isAlive) return;

            // Optimización: Desactivar si queda demasiado lejos de la cámara
            CheckFarCameraDeactivation();
        }

        /// <summary>
        /// Comprueba la distancia con la cámara para desactivar el Goomba si quedó muy lejos.
        /// </summary>
        private void CheckFarCameraDeactivation()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            float distanceToCamera = Mathf.Abs(transform.position.x - cam.transform.position.x);
            if (distanceToCamera > maxCameraDistance && isActivated)
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Invocado cuando Mario pisa al Goomba desde arriba.
        /// Hace rebotar a Mario, activa la animación 'isSquished', desactiva la física
        /// y destruye el GameObject tras squishDuration (0.5s).
        /// </summary>
        /// <param name="player">GameObject del jugador.</param>
        public override void OnPlayerStomp(GameObject player)
        {
            if (!isAlive) return;
            isAlive = false;

            // 1. Mario rebota hacia arriba
            BouncePlayer(player);

            // 2. Desactivar collider para que no interactúe más con Mario ni el entorno
            if (boxCol != null)
            {
                boxCol.enabled = false;
            }
            if (col != null)
            {
                col.enabled = false;
            }

            // 3. Detener físicas por completo
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }

            // 4. Activar animación de Goomba aplastado
            if (animator != null)
            {
                animator.SetBool(squishedHash, true);
            }

            // 5. Destruir después de 0.5s
            Destroy(gameObject, squishDuration);
        }

        /// <summary>
        /// Invocado cuando Mario toca al Goomba lateralmente o por debajo.
        /// Causa daño a Mario a través de PlayerHealth.
        /// </summary>
        /// <param name="player">GameObject del jugador.</param>
        public override void OnPlayerContact(GameObject player)
        {
            if (!isAlive) return;

            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null && !health.IsInvincible && !health.IsDead)
            {
                health.TakeDamage();
            }
        }

        /// <summary>
        /// Muerte estándar por efecto externo.
        /// </summary>
        public override void Die()
        {
            if (!isAlive) return;
            isAlive = false;

            if (boxCol != null) boxCol.enabled = false;
            if (col != null) col.enabled = false;

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }

            if (animator != null)
            {
                animator.SetBool(squishedHash, true);
            }

            Destroy(gameObject, squishDuration);
        }
    }
}
