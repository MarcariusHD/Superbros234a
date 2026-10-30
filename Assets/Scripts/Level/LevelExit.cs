using System.Collections;
using MarioBros.Player;
using MarioBros.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MarioBros.Level
{
    public class LevelExit : MonoBehaviour
    {
        public string nextScene = "Underwater";
        public bool requireDown;
        private bool entered;

        private void OnTriggerStay2D(Collider2D other)
        {
            var health = other.GetComponent<PlayerHealth>();
            if (entered || health == null || health.IsDead || (requireDown && !MarioInput.Down)) return;
            if (GameManager.CurrentState != GameState.Playing) return;
            if (requireDown && (Mathf.Abs(other.bounds.center.x - transform.position.x) > .55f ||
                other.bounds.min.y < transform.position.y - .3f)) return;
            if (!Application.CanStreamedLevelBeLoaded(nextScene))
            {
                Debug.LogError("Falta generar o registrar la escena: " + nextScene, this);
                return;
            }
            entered = true;
            StartCoroutine(Travel(other.gameObject));
        }

        private IEnumerator Travel(GameObject player)
        {
            GameManager.Instance.LevelCompleted();
            var travel=new PipeTransit(player);
            yield return travel.Enter(transform.position,requireDown);
            SceneManager.LoadScene(nextScene);
        }
    }
}
