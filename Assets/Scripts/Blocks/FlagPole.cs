using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using MarioBros.Player;
using MarioBros.UI;

namespace MarioBros.Blocks
{
    /// <summary>
    /// Asta de bandera de fin de nivel (Goal Flagpole).
    /// Al hacer contacto con Mario:
    /// - Detiene el control del jugador.
    /// - Calcula los puntos según la altura del agarre (100 a 5000 pts).
    /// - Mario y la bandera descienden por el poste.
    /// - Mario camina hacia el castillo y se carga la siguiente escena.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class FlagPole : MonoBehaviour
    {
        [Header("Puntos del Asta")]
        [Tooltip("Punto superior del asta de la bandera.")]
        [SerializeField] private Transform poleTop;

        [Tooltip("Punto inferior del asta (donde termina el descenso).")]
        [SerializeField] private Transform poleBottom;

        [Tooltip("Transform de la bandera que desciende.")]
        [SerializeField] private Transform flagTransform;

        [Tooltip("Destino opcional hacia donde camina Mario (puerta del castillo).")]
        [SerializeField] private Transform castleDestination;

        [Header("Velocidades y Tiempos")]
        [Tooltip("Velocidad de deslizamiento por el poste.")]
        [SerializeField] private float slideSpeed = 5.0f;

        [Tooltip("Velocidad de caminata hacia el castillo tras descender.")]
        [SerializeField] private float walkSpeed = 3.0f;

        [Tooltip("Tiempo de espera tras llegar al castillo antes de cargar la escena.")]
        [SerializeField] private float sceneLoadDelay = 3.0f;

        [Header("Audio")]
        [SerializeField] private AudioClip flagSlideSound;
        [SerializeField] private AudioClip levelCompleteMusic;

        [Header("Transición de Escena")]
        [Tooltip("Nombre de la siguiente escena. Si está vacío, carga buildIndex + 1.")]
        [SerializeField] private string nextSceneName = "";

        private bool isTriggered = false;

        private void Awake()
        {
            SeparateFlag();
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void SeparateFlag()
        {
            var poleSprite=Resources.Load<Sprite>("ClassicFlag/Pole");
            var bannerSprite=Resources.Load<Sprite>("ClassicFlag/Banner");
            if(poleSprite==null||bannerSprite==null||flagTransform==null)return;
            var root=flagTransform.parent;
            foreach(var renderer in root.GetComponentsInChildren<SpriteRenderer>())
            {
                Vector3 p=renderer.transform.position;
                if(renderer.name.StartsWith("Overworld_")&&p.x>=197&&p.x<=199&&p.y>3&&p.y<13)
                    renderer.enabled=false;
            }
            var mast=new GameObject("Fixed flag mast");mast.transform.SetParent(root,false);
            mast.transform.position=new Vector3(198,8,0);
            var fixedRenderer=mast.AddComponent<SpriteRenderer>();fixedRenderer.sprite=poleSprite;
            fixedRenderer.sortingLayerName="Background";fixedRenderer.sortingOrder=1;
            var banner=new GameObject("Complete moving flag");banner.transform.SetParent(root,false);
            banner.transform.position=new Vector3(198,11,0);
            var moving=banner.AddComponent<SpriteRenderer>();moving.sprite=bannerSprite;
            moving.sortingLayerName="Background";moving.sortingOrder=2;
            flagTransform=banner.transform;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isTriggered) return;

            if (other.CompareTag("Player") || other.GetComponent<PlayerMovement>() != null)
            {
                isTriggered = true;
                StartCoroutine(FlagPoleSequenceRoutine(other.gameObject, other.transform.position.y));
            }
        }

        private IEnumerator FlagPoleSequenceRoutine(GameObject playerObj, float contactY)
        {
            CoopSession.BeginTransition();
            foreach(var partner in CoopSession.Players)
            {
                if(partner==null||partner.gameObject==playerObj)continue;
                partner.GetComponent<Rigidbody2D>().simulated=false;
                foreach(var partnerCollider in partner.GetComponentsInChildren<Collider2D>())partnerCollider.enabled=false;
                partner.GetComponent<PlayerAnimation>().SetScriptedPose("climb");
            }
            // Detener el juego / temporizador en el GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LevelCompleted();
            }

            PlayerMovement movement = playerObj.GetComponent<PlayerMovement>();
            Rigidbody2D rb = playerObj.GetComponent<Rigidbody2D>();
            Collider2D col = playerObj.GetComponent<Collider2D>();
            SpriteRenderer sr = playerObj.GetComponentInChildren<SpriteRenderer>();
            PlayerAnimation animation = playerObj.GetComponent<PlayerAnimation>();
            if(animation!=null) animation.SetScriptedPose("climb");

            // Desactivar controles normales de Mario
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

            if (col != null)
            {
                col.enabled = false;
            }

            // Calcular y otorgar puntaje según altura de contacto
            int points = CalculateScore(contactY);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(points);
            }

            if (MarioBros.Sound.MusicManager.Instance != null) MarioBros.Sound.MusicManager.Instance.StopImmediate();
            // Reproducir sonido de deslizamiento
            MarioBros.Sound.SfxPlayer.Play("flagpole", flagSlideSound);

            // Posicionar a Mario pegado al lado izquierdo del asta
            float poleX = transform.position.x;
            playerObj.transform.position = new Vector3(poleX - 0.25f, contactY, playerObj.transform.position.z);

            float bottomY = poleBottom != null ? poleBottom.position.y : transform.position.y;
            float flagBottomY = bottomY + 1f;

            // Secuencia de descenso sincronizado de Mario y la Bandera
            while (playerObj.transform.position.y > bottomY + 0.05f ||
                (flagTransform != null && flagTransform.position.y > flagBottomY + .01f))
            {
                // Bajar Mario
                Vector3 playerPos = playerObj.transform.position;
                playerPos.y = Mathf.MoveTowards(playerPos.y, bottomY, slideSpeed * Time.deltaTime);
                playerObj.transform.position = playerPos;
                MovePartner(playerObj,"climb");

                // Bajar Bandera
                if (flagTransform != null && flagTransform.position.y > flagBottomY)
                {
                    Vector3 flagPos = flagTransform.position;
                    flagPos.y = Mathf.MoveTowards(flagPos.y, flagBottomY, slideSpeed * Time.deltaTime);
                    flagTransform.position = flagPos;
                }

                yield return null;
            }

            // Posicionar a Mario en la base
            playerObj.transform.position = new Vector3(poleX - 0.25f, bottomY, playerObj.transform.position.z);

            // Pequeña pausa al llegar al fondo y salto al otro lado del poste
            yield return new WaitForSeconds(0.2f);
            playerObj.transform.position = new Vector3(poleX + 0.35f, bottomY, playerObj.transform.position.z);

            // Música de victoria
            

            yield return new WaitForSeconds(0.4f);

            // Caminata hacia el castillo
            if(animation!=null) animation.SetScriptedPose("walk");
            MovePartner(playerObj,"walk");
            Vector3 targetWalkPos = castleDestination != null ?
                castleDestination.position :
                playerObj.transform.position + new Vector3(7f, 0f, 0f);

            while (Mathf.Abs(playerObj.transform.position.x - targetWalkPos.x) > 0.1f)
            {
                playerObj.transform.position = Vector3.MoveTowards(
                    playerObj.transform.position,
                    new Vector3(targetWalkPos.x, bottomY, playerObj.transform.position.z),
                    walkSpeed * Time.deltaTime
                );
                yield return null;
                MovePartner(playerObj,"walk");
            }

            // Ocultar a Mario al entrar al castillo
            playerObj.transform.position=new Vector3(targetWalkPos.x,bottomY,playerObj.transform.position.z);
            if (sr != null)
            {
                sr.enabled = false;
            }
            foreach(var partner in CoopSession.Players)if(partner!=null)partner.GetComponent<SpriteRenderer>().enabled=false;

            // Esperar antes de cambiar de escena
            yield return new WaitForSeconds(sceneLoadDelay);

            LoadNextLevel();
        }
        private static void MovePartner(GameObject leader,string pose)
        {
            foreach(var partner in CoopSession.Players)
            {
                if(partner==null||partner.gameObject==leader)continue;
                partner.transform.position=leader.transform.position+Vector3.right*.8f;
                var animation=partner.GetComponent<PlayerAnimation>();
                animation.SetScriptedPose(pose);
            }
        }

        /// <summary>
        /// Calcula el puntaje proporcional según la altura a la que Mario tocó el mástil.
        /// </summary>
        private int CalculateScore(float contactY)
        {
            float topY = poleTop != null ? poleTop.position.y : transform.position.y + 7f;
            float botY = poleBottom != null ? poleBottom.position.y : transform.position.y;

            float ratio = Mathf.InverseLerp(botY, topY, contactY);

            if (ratio >= 0.85f) return 5000;
            if (ratio >= 0.65f) return 2000;
            if (ratio >= 0.40f) return 800;
            if (ratio >= 0.20f) return 400;
            return 100;
        }

        /// <summary>
        /// Carga el siguiente nivel o escena.
        /// </summary>
        private void LoadNextLevel()
        {
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
                if (nextIndex < SceneManager.sceneCountInBuildSettings &&
                    System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(nextIndex)) != "GameOver")
                {
                    SceneManager.LoadScene(nextIndex);
                }
                else
                {
                    // Si no hay más escenas, recargar la actual
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
            }
        }
    }
}
