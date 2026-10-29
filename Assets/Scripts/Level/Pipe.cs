using System.Collections;
using UnityEngine;
using MarioBros.Player;

namespace MarioBros.Level
{
    /// <summary>
    /// Dirección en la que el jugador debe presionar para entrar a la tubería.
    /// </summary>
    public enum PipeEntryDirection
    {
        Down,   // Tubería vertical: Mario se agacha con tecla Abajo
        Right,  // Tubería horizontal: Mario camina hacia la derecha
        Left,   // Tubería horizontal: Mario camina hacia la izquierda
        Up      // Tubería invertida: Mario salta hacia arriba
    }

    /// <summary>
    /// Tubería transportadora (Warp Pipe) estilo Super Mario Bros.
    /// Permite a Mario descender o entrar a una tubería usando el sistema clásico de Input (Input.GetKey).
    /// Ejecuta animación de deslizamiento y teletransporta al destino o tubería vinculada.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Pipe : MonoBehaviour
    {
        [Header("Configuración de Entrada")]
        [Tooltip("Si Mario puede entrar a través de esta tubería.")]
        [SerializeField] private bool isEnterable = true;

        [Tooltip("Dirección en la que Mario debe presionar el control para ingresar.")]
        [SerializeField] private PipeEntryDirection entryDirection = PipeEntryDirection.Down;

        [Header("Destino de Teletransporte")]
        [Tooltip("Tubería de salida vinculada (opcional, para animación completa de entrada y salida).")]
        [SerializeField] private Pipe destinationPipe;

        [Tooltip("Transform alternativo de salida directa si no se vincula otra tubería.")]
        [SerializeField] private Transform directExitPoint;

        [Header("Animación")]
        [Tooltip("Distancia que recorre Mario deslizándose dentro de la tubería.")]
        [SerializeField] private float slideDistance = 1.5f;

        [Tooltip("Tiempo en segundos que tarda en introducirse o emerger.")]
        [SerializeField] private float slideDuration = 0.8f;

        [Header("Audio")]
        [Tooltip("Sonido clásico al entrar o salir de una tubería.")]
        [SerializeField] private AudioClip pipeWarpSound;

        [Header("Planta Piraña (Opcional)")]
        [Tooltip("Referencia a la planta piraña que habita esta tubería.")]
        [SerializeField] private GameObject piranhaPlant;

        // Estado interno
        private bool isPlayerInTrigger = false;
        private GameObject playerInstance;
        private bool isTransitioning = false;

        public PipeEntryDirection EntryDirection => entryDirection;
        public float SlideDistance => slideDistance;
        public float SlideDuration => slideDuration;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") || other.GetComponent<PlayerMovement>() != null)
            {
                isPlayerInTrigger = true;
                playerInstance = other.gameObject;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject == playerInstance)
            {
                isPlayerInTrigger = false;
                playerInstance = null;
            }
        }

        private void Update()
        {
            if (!isEnterable || isTransitioning || !isPlayerInTrigger || playerInstance == null)
            {
                return;
            }

            // Comprobar si Mario presiona la dirección adecuada usando el sistema de Input tradicional (Old Input System)
            if (IsInputActive())
            {
                PlayerMovement movement = playerInstance.GetComponent<PlayerMovement>();

                // Para entrar hacia abajo, Mario debe estar en el suelo
                if (entryDirection == PipeEntryDirection.Down && movement != null && !movement.IsGrounded)
                {
                    return;
                }

                // Si hay una planta piraña activa y visible, no permitir entrar
                if (piranhaPlant != null && piranhaPlant.activeInHierarchy)
                {
                    // Si la planta tiene algún script o collider activo
                    Collider2D plantCol = piranhaPlant.GetComponent<Collider2D>();
                    if (plantCol != null && plantCol.enabled)
                    {
                        return;
                    }
                }

                StartCoroutine(EnterPipeRoutine(playerInstance));
            }
        }

        /// <summary>
        /// Comprueba el Old Input System (Input.GetKey / Input.GetAxisRaw) según la dirección de entrada.
        /// </summary>
        private bool IsInputActive()
        {
            switch (entryDirection)
            {
                case PipeEntryDirection.Down:
                    return Input.GetKey(KeyCode.DownArrow) ||
                           Input.GetKey(KeyCode.S) ||
                           Input.GetAxisRaw("Vertical") < -0.5f;

                case PipeEntryDirection.Right:
                    return Input.GetKey(KeyCode.RightArrow) ||
                           Input.GetKey(KeyCode.D) ||
                           Input.GetAxisRaw("Horizontal") > 0.5f;

                case PipeEntryDirection.Left:
                    return Input.GetKey(KeyCode.LeftArrow) ||
                           Input.GetKey(KeyCode.A) ||
                           Input.GetAxisRaw("Horizontal") < -0.5f;

                case PipeEntryDirection.Up:
                    return Input.GetKey(KeyCode.UpArrow) ||
                           Input.GetKey(KeyCode.W) ||
                           Input.GetAxisRaw("Vertical") > 0.5f;

                default:
                    return false;
            }
        }

        private Vector3 GetDirectionVector(PipeEntryDirection dir)
        {
            switch (dir)
            {
                case PipeEntryDirection.Down: return Vector3.down;
                case PipeEntryDirection.Right: return Vector3.right;
                case PipeEntryDirection.Left: return Vector3.left;
                case PipeEntryDirection.Up: return Vector3.up;
                default: return Vector3.down;
            }
        }

        /// <summary>
        /// Rutina de animación para ingresar a la tubería.
        /// </summary>
        private IEnumerator EnterPipeRoutine(GameObject player)
        {
            isTransitioning = true;

            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            Collider2D[] colliders = player.GetComponentsInChildren<Collider2D>();
            SpriteRenderer sr = player.GetComponentInChildren<SpriteRenderer>();

            // Deshabilitar control del jugador y física
            if (movement != null)
            {
                movement.StopMovement();
                movement.enabled = false;
            }

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.linearVelocity = Vector2.zero;
            }

            foreach (var col in colliders)
            {
                col.enabled = false;
            }

            // Cambiar capa de renderizado temporalmente para que Mario quede oculto tras la tubería
            int originalOrder = sr != null ? sr.sortingOrder : 0;
            if (sr != null)
            {
                sr.sortingOrder = -5;
            }

            // Reproducir sonido
            if (pipeWarpSound != null)
            {
                AudioSource.PlayClipAtPoint(pipeWarpSound, transform.position);
            }

            // Deslizar a Mario hacia el interior de la tubería
            Vector3 startPos = player.transform.position;
            Vector3 targetPos = startPos + GetDirectionVector(entryDirection) * slideDistance;
            float elapsed = 0f;

            while (elapsed < slideDuration)
            {
                player.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / slideDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            player.transform.position = targetPos;

            yield return new WaitForSeconds(0.2f);

            // Teletransportar a la tubería de destino o al punto directo
            if (destinationPipe != null)
            {
                destinationPipe.ReceivePlayer(player, originalOrder);
            }
            else if (directExitPoint != null)
            {
                player.transform.position = directExitPoint.position;

                // Restaurar componentes
                if (sr != null) sr.sortingOrder = originalOrder;
                foreach (var col in colliders) col.enabled = true;
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.linearVelocity = Vector2.zero;
                }
                if (movement != null) movement.enabled = true;
            }

            isTransitioning = false;
        }

        /// <summary>
        /// Recibe a Mario desde una tubería de entrada y ejecuta la animación de emergencia (salida).
        /// </summary>
        public void ReceivePlayer(GameObject player, int originalSortingOrder)
        {
            StartCoroutine(ExitPipeRoutine(player, originalSortingOrder));
        }

        private IEnumerator ExitPipeRoutine(GameObject player, int originalSortingOrder)
        {
            isTransitioning = true;

            SpriteRenderer sr = player.GetComponentInChildren<SpriteRenderer>();
            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            Collider2D[] colliders = player.GetComponentsInChildren<Collider2D>();

            // Dirección en la que Mario sale de la tubería de destino (opuesta a su orientación natural o hacia arriba/derecha)
            Vector3 exitDirectionVec = Vector3.up;
            if (entryDirection == PipeEntryDirection.Down)
            {
                exitDirectionVec = Vector3.up; // Emerge hacia arriba
            }
            else if (entryDirection == PipeEntryDirection.Left)
            {
                exitDirectionVec = Vector3.right; // Emerge hacia la derecha
            }

            // Colocar a Mario oculto dentro de la tubería de destino
            Vector3 endPos = transform.position + exitDirectionVec * 0.75f;
            Vector3 startPos = endPos - exitDirectionVec * slideDistance;
            player.transform.position = startPos;

            // Reproducir sonido al salir
            if (pipeWarpSound != null)
            {
                AudioSource.PlayClipAtPoint(pipeWarpSound, transform.position);
            }

            float elapsed = 0f;
            while (elapsed < slideDuration)
            {
                player.transform.position = Vector3.Lerp(startPos, endPos, elapsed / slideDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            player.transform.position = endPos;

            // Restaurar sorting order y reactivar controles
            if (sr != null)
            {
                sr.sortingOrder = originalSortingOrder;
            }

            foreach (var col in colliders)
            {
                col.enabled = true;
            }

            if (rb != null)
            {
                rb.isKinematic = false;
                rb.linearVelocity = Vector2.zero;
            }

            if (movement != null)
            {
                movement.enabled = true;
            }

            isTransitioning = false;
        }
    }
}
