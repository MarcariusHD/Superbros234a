using System.Collections;
using UnityEngine;
using MarioBros.Player;

namespace MarioBros.Enemies
{
    /// <summary>
    /// Controlador de la Planta Piraña (Piranha Plant) que habita en las tuberías.
    /// Se desplaza verticalmente entrando y saliendo de la tubería mediante un ciclo temporizado.
    /// Permanece oculta ~2 segundos y expuesta ~2 segundos.
    /// Si Mario se encuentra directamente encima o muy cerca de la tubería, la planta no sale.
    /// Provoca daño a Mario al contacto mediante un Collider2D trigger (sin necesidad de Rigidbody).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PiranhaPlant : MonoBehaviour
    {
        [Header("Movimiento Vertical")]
        [Tooltip("Distancia vertical en unidades que sube la planta (PPU=16: ~1.5 a 2.0 unidades).")]
        [SerializeField] private float moveDistance = 1.75f;

        [Tooltip("Tiempo en segundos que tarda en emerger o descender.")]
        [SerializeField] private float moveDuration = 1.0f;

        [Header("Temporizadores")]
        [Tooltip("Tiempo en segundos que permanece escondida en la tubería.")]
        [SerializeField] private float hiddenDuration = 2.0f;

        [Tooltip("Tiempo en segundos que permanece expuesta fuera de la tubería.")]
        [SerializeField] private float exposedDuration = 2.0f;

        [Header("Detección de Proximidad del Jugador")]
        [Tooltip("Radio de seguridad alrededor de la tubería; si Mario está dentro, la planta no emerge.")]
        [SerializeField] private float playerDetectionRadius = 2.2f;

        [Tooltip("Capa de física correspondiente al jugador.")]
        [SerializeField] private LayerMask playerLayer;

        [Header("Optimización")]
        [Tooltip("Distancia máxima respecto a la cámara principal para pausar el movimiento.")]
        [SerializeField] private float maxCameraDistance = 22f;

        // Posiciones calculadas
        private Vector3 hiddenPosition;
        private Vector3 emergedPosition;

        // Referencias de componentes
        private Collider2D plantCollider;
        private Coroutine plantCycleCoroutine;
        private bool isAlive = true;

        // Propiedad pública de estado
        public bool IsAlive => isAlive;

        private void Awake()
        {
            plantCollider = GetComponent<Collider2D>();
            if (plantCollider != null)
            {
                // Asegurar que el collider esté configurado como Trigger para no empujar a Mario ni requerir Rigidbody
                plantCollider.isTrigger = true;
            }
        }

        private void Start()
        {
            // La posición inicial en el editor es la posición oculta (dentro de la tubería)
            hiddenPosition = transform.position;
            emergedPosition = hiddenPosition + Vector3.up * moveDistance;

            plantCycleCoroutine = StartCoroutine(PlantRoutine());
        }

        /// <summary>
        /// Ciclo continuo de la Planta Piraña:
        /// 1. Espera oculta en la tubería durante hiddenDuration (~2s).
        /// 2. Comprueba que Mario no esté cerca; si está cerca, espera hasta que se aleje.
        /// 3. Emerge suavemente hacia arriba durante moveDuration.
        /// 4. Permanece expuesta durante exposedDuration (~2s).
        /// 5. Desciende suavemente al interior de la tubería.
        /// </summary>
        private IEnumerator PlantRoutine()
        {
            while (isAlive)
            {
                // 1. Oculta dentro de la tubería
                yield return new WaitForSeconds(hiddenDuration);

                // 2. Si Mario está cerca o parado sobre la tubería, esperar a que se aleje
                while (isAlive && IsPlayerNearby())
                {
                    yield return new WaitForSeconds(0.25f);
                }

                if (!isAlive) yield break;

                // Si la cámara está demasiado lejos, esperar para ahorrar ciclos
                while (!IsNearCamera())
                {
                    yield return new WaitForSeconds(1.0f);
                }

                // 3. Subir hacia la posición expuesta
                yield return StartCoroutine(MoveSmooth(hiddenPosition, emergedPosition, moveDuration));

                // 4. Permanecer expuesta
                yield return new WaitForSeconds(exposedDuration);

                if (!isAlive) yield break;

                // 5. Descender de vuelta a la tubería
                yield return StartCoroutine(MoveSmooth(emergedPosition, hiddenPosition, moveDuration));
            }
        }

        /// <summary>
        /// Desplaza suavemente la planta entre dos posiciones usando interpolación (SmoothStep).
        /// </summary>
        private IEnumerator MoveSmooth(Vector3 startPos, Vector3 endPos, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Curva de interpolación suave (SmoothStep)
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                transform.position = Vector3.Lerp(startPos, endPos, smoothT);
                yield return null;
            }

            transform.position = endPos;
        }

        /// <summary>
        /// Comprueba si Mario se encuentra dentro del radio de proximidad de la tubería.
        /// </summary>
        /// <returns>True si Mario está cerca de la tubería; de lo contrario, False.</returns>
        private bool IsPlayerNearby()
        {
            // Chequeo por LayerMask si está configurada
            if (playerLayer != 0)
            {
                Collider2D hit = Physics2D.OverlapCircle(transform.position, playerDetectionRadius, playerLayer);
                if (hit != null) return true;
            }

            // Chequeo complementario por componente PlayerHealth o Tag "Player"
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float distance = Vector2.Distance(transform.position, player.transform.position);
                if (distance <= playerDetectionRadius)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Comprueba si la planta se encuentra en el rango visible de la cámara.
        /// </summary>
        private bool IsNearCamera()
        {
            Camera cam = Camera.main;
            if (cam == null) return true;

            float distance = Mathf.Abs(cam.transform.position.x - transform.position.x);
            return distance <= maxCameraDistance;
        }

        /// <summary>
        /// Daña a Mario si este entra en el área del trigger de la Planta Piraña.
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            DamagePlayerIfPossible(other.gameObject);
        }

        /// <summary>
        /// Daña a Mario si permanece dentro del área del trigger.
        /// </summary>
        private void OnTriggerStay2D(Collider2D other)
        {
            DamagePlayerIfPossible(other.gameObject);
        }

        /// <summary>
        /// Soporte adicional en caso de que se configure un collider no trigger.
        /// </summary>
        private void OnCollisionEnter2D(Collision2D collision)
        {
            DamagePlayerIfPossible(collision.gameObject);
        }

        /// <summary>
        /// Aplica daño al jugador llamando a PlayerHealth.TakeDamage().
        /// </summary>
        /// <param name="target">GameObject con el que colisionó la planta.</param>
        private void DamagePlayerIfPossible(GameObject target)
        {
            if (!isAlive) return;

            PlayerHealth health = target.GetComponent<PlayerHealth>();
            if (health != null && !health.IsInvincible && !health.IsDead)
            {
                health.TakeDamage();
            }
        }

        /// <summary>
        /// Destruye o desactiva la planta piraña (por ejemplo, al ser impactada por una bola de fuego o estrella).
        /// </summary>
        public void Die()
        {
            if (!isAlive) return;
            isAlive = false;

            if (plantCycleCoroutine != null)
            {
                StopCoroutine(plantCycleCoroutine);
                plantCycleCoroutine = null;
            }

            if (plantCollider != null)
            {
                plantCollider.enabled = false;
            }

            Destroy(gameObject, 0.1f);
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 basePos = Application.isPlaying ? hiddenPosition : transform.position;
            Vector3 topPos = basePos + Vector3.up * moveDistance;

            // Dibujar línea de recorrido vertical
            Gizmos.color = Color.green;
            Gizmos.DrawLine(basePos, topPos);
            Gizmos.DrawWireCube(topPos, new Vector3(1f, 1f, 0.1f));

            // Dibujar radio de detección de proximidad de Mario
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(basePos, playerDetectionRadius);
        }
    }
}
