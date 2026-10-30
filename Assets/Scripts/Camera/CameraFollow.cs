using UnityEngine;

namespace MarioBros.CameraControl
{
    /// <summary>
    /// Cámara de desplazamiento lateral clásico estilo Super Mario Bros (NES).
    /// Solo se desplaza horizontalmente hacia la derecha (nunca retrocede hacia la izquierda).
    /// Mantiene la resolución de aspecto y tamaño ortográfico de NES (7.5 para 240px a 16 PPU).
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFollow : MonoBehaviour
    {
        [Header("Objetivo a Seguir")]
        [Tooltip("Transform del jugador (Mario) a seguir.")]
        [SerializeField] private Transform target;

        [Header("Configuración de Desplazamiento")]
        [Tooltip("Desplazamiento horizontal para mantener a Mario ligeramente a la izquierda del centro de la pantalla.")]
        [SerializeField] private float offsetX = 2.0f;

        [Tooltip("Tiempo de suavizado para el movimiento de la cámara (SmoothDamp).")]
        [SerializeField] private float smoothTime = 0.3f;

        [Tooltip("Si es verdadero, la cámara solo avanzará hacia la derecha y nunca retrocederá.")]
        [SerializeField] private bool onlyScrollRight = true;

        [Header("Límites del Nivel (Bounds)")]
        [Tooltip("Límite mínimo en el eje X para la posición de la cámara.")]
        [SerializeField] private float minX = 0f;

        [Tooltip("Límite máximo en el eje X para la posición de la cámara.")]
        [SerializeField] private float maxX = 250f;

        [Tooltip("Altura fija en el eje Y de la cámara.")]
        [SerializeField] private float fixedY = 7.5f;

        [Header("Configuración de Viewport NES")]
        [Tooltip("Tamaño ortográfico para emular 240px de altura a 16 PPU (240 / 16 / 2 = 7.5).")]
        [SerializeField] private float nesOrthographicSize = 7.5f;

        [Tooltip("Si se debe forzar automáticamente el tamaño ortográfico al iniciar.")]
        [SerializeField] private bool autoSetOrthoSize = true;

        // Referencias internas
        private Camera cam;
        private float currentVelocityX;
        private float furthestRightX;
        [SerializeField] private bool followAboveCeiling;

        // Propiedades públicas
        public Transform Target { get => target; set => target = value; }
        public float FurthestRightX => furthestRightX;

        private void Awake()
        {
            cam = GetComponent<Camera>();

            if (autoSetOrthoSize && cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = nesOrthographicSize;
            }
        }

        private void Start()
        {
            // Si no se asignó el objetivo manualmente, intentar buscar el GameObject con tag Player
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
            }

            // Inicializar la posición máxima alcanzada a la posición inicial
            furthestRightX = transform.position.x;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // Calcular la posición objetivo en X manteniendo a Mario a la izquierda del centro
            // Al sumar offsetX, el centro de la cámara se coloca por delante de Mario
            float targetX = target.position.x + offsetX;

            // Si solo se permite avanzar a la derecha, el objetivo nunca puede ser menor a la posición más avanzada alcanzada
            if (onlyScrollRight)
            {
                targetX = Mathf.Max(targetX, furthestRightX);
            }

            // Clampear entre los límites mínimos y máximos configurados para el nivel
            targetX = Mathf.Clamp(targetX, minX, maxX);

            // Suavizar el desplazamiento horizontal con SmoothDamp
            float newX = Mathf.SmoothDamp(transform.position.x, targetX, ref currentVelocityX, smoothTime);

            // Si solo se avanza a la derecha, garantizar que la cámara nunca se mueva a la izquierda
            if (onlyScrollRight)
            {
                newX = Mathf.Max(newX, transform.position.x);
                furthestRightX = Mathf.Max(furthestRightX, newX);
            }

            // Actualizar la posición de la cámara (manteniendo Y y Z fijos)
            float viewY = followAboveCeiling ? Mathf.Clamp(target.position.y - 3f, fixedY, 12f) : fixedY;
            transform.position = new Vector3(newX, viewY, transform.position.z);
        }

        /// <summary>
        /// Reinicia la posición más avanzada de la cámara al reiniciar un nivel.
        /// </summary>
        public void ResetPosition(Vector3 newPosition)
        {
            transform.position = new Vector3(newPosition.x, fixedY, transform.position.z);
            furthestRightX = newPosition.x;
            currentVelocityX = 0f;
        }
        public void SetRegion(float minimum,float maximum,float initialX)
        {
            minX=minimum;maxX=maximum;ResetPosition(new Vector3(initialX,fixedY,transform.position.z));
        }
    }
}
