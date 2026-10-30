using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MarioBros.UI
{
    public class GameOverScreen : MonoBehaviour
    {
        private bool restarting;
        public static void ShowRuntimeScene()
        {
            var previous=SceneManager.GetActiveScene();
            var scene=SceneManager.CreateScene("GameOver");SceneManager.SetActiveScene(scene);
            new GameObject("Game Over NES").AddComponent<GameOverScreen>();
            SceneManager.UnloadSceneAsync(previous);
        }
        private void Start()
        {
            Time.timeScale=1;
            if(GameManager.Instance==null) new GameObject("GameManager").AddComponent<GameManager>();
            GameManager.Instance.SetState(GameState.GameOver);
            ClassicFrontEnd.ShowStatus("GameOver",GameManager.LastWorld);
        }
        private void Update()
        {
            if(MarioInput.JumpDown || (Keyboard.current!=null && Keyboard.current.enterKey.wasPressedThisFrame)) Restart();
        }
        public void Restart()
        {
            if(restarting)return;restarting=true;
            if(Application.CanStreamedLevelBeLoaded("TitleScreen")) SceneManager.LoadScene("TitleScreen");
            else GameManager.Instance.RestartGame();
        }
    }
}
