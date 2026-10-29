using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace MarioBros.UI
{
    public class GameOverScreen : MonoBehaviour
    {
        public TextMeshProUGUI score;
        private bool restarting;
        private void Awake()
        {
            if (score == null) BuildInterface();
        }

        // Also works if a custom build profile accidentally excludes the saved scene.
        public static void ShowRuntimeScene()
        {
            var previous = SceneManager.GetActiveScene();
            var scene = SceneManager.CreateScene("GameOver");
            SceneManager.SetActiveScene(scene);
            new GameObject("Game Over Screen").AddComponent<GameOverScreen>();
            SceneManager.UnloadSceneAsync(previous);
        }

        private void BuildInterface()
        {
            var cameraObject = new GameObject("Game Over Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.transform.SetParent(transform);
            var camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.cullingMask = 0;

            var canvasObject = new GameObject("Game Over Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(960, 540);
            scaler.matchWidthOrHeight = .5f;

            score = Label(canvas.transform, "Final Score", "", .8f, 24);
            Label(canvas.transform, "Title", "GAME OVER", .52f, 44);
            Label(canvas.transform, "Hint", "ENTER / ESPACIO PARA REINTENTAR", .35f, 22);
            var buttonObject = new GameObject("Reintentar", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(canvas.transform, false);
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .22f);
            rect.sizeDelta = new Vector2(240, 50);
            buttonObject.GetComponent<Image>().color = new Color(.18f, .18f, .18f);
            var caption = Label(buttonObject.transform, "Label", "REINTENTAR", .5f, 22);
            caption.rectTransform.sizeDelta = new Vector2(240, 50);
            buttonObject.GetComponent<Button>().onClick.AddListener(Restart);
            var events = new GameObject("Game Over Input", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.transform.SetParent(transform);
        }

        private static TextMeshProUGUI Label(Transform parent, string name, string text, float y, float size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var label = go.AddComponent<TextMeshProUGUI>();
            label.rectTransform.anchorMin = label.rectTransform.anchorMax = new Vector2(.5f, y);
            label.rectTransform.sizeDelta = new Vector2(880, 70);
            label.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            label.text = text;
            label.fontSize = size;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            return label;
        }
        private void Start()
        {
            Time.timeScale = 1;
            if (GameManager.Instance == null) new GameObject("GameManager").AddComponent<GameManager>();
            GameManager.Instance.SetState(GameState.GameOver);
            score.text = "MARIO  " + GameManager.Score.ToString("D6") + "     COINS  " + GameManager.Coins.ToString("D2");
        }
        private void Update()
        {
            var kb = Keyboard.current;
            if (MarioInput.JumpDown || (kb != null && kb.enterKey.wasPressedThisFrame)) Restart();
        }
        public void Restart()
        {
            if (restarting) return;
            restarting = true;
            GameManager.Instance.RestartGame();
        }
    }
}
