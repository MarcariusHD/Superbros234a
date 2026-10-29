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
            entered = true;
            StartCoroutine(Travel(other.gameObject));
        }

        private IEnumerator Travel(GameObject player)
        {
            GameManager.Instance.LevelCompleted();
            player.GetComponent<PlayerMovement>().enabled = false;
            var body = player.GetComponent<Rigidbody2D>();
            body.linearVelocity = Vector2.zero;
            body.simulated = false;
            var start = player.transform.position;
            for (float t = 0; t < 1; t += Time.deltaTime)
            {
                player.transform.position = start + (requireDown ? Vector3.down : Vector3.right) * t;
                yield return null;
            }
            SceneManager.LoadScene(nextScene);
        }
    }
}
