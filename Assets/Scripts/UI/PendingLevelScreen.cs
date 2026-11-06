using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MarioBros.UI
{
    public class PendingLevelScreen : MonoBehaviour
    {
        private bool leaving;
        private void Start()
        {
            if(Application.CanStreamedLevelBeLoaded("World2_3"))
            {
                SceneManager.LoadScene("World2_3");return;
            }
            Time.timeScale = 1;
            if (GameManager.Instance != null) GameManager.Instance.LevelCompleted();
        }
        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame) Return();
        }
        public void Return()
        {
            if (leaving) return;
            leaving = true;
            SceneManager.LoadScene("Underground");
        }
    }
}
