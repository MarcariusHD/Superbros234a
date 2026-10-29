using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using MarioBros.CameraControl;
using MarioBros.Player;
using MarioBros.UI;
using MarioBros.Level;

/// <summary>
/// Editor utility to set up the initial Mario scene with all required GameObjects.
/// Run from menu: Tools > Setup Mario Scene
/// </summary>
public class SceneSetupEditor : EditorWindow
{
    [MenuItem("Tools/Setup Mario Scene")]
    static void SetupScene()
    {
        // === PLAYER ===
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            player = new GameObject("Mario");
            player.tag = "Player";
            int playerLayer = LayerMask.NameToLayer("PlayerLayer");
            if (playerLayer != -1) player.layer = playerLayer;
            player.transform.position = new Vector3(2.5f, 2f, 0f);

            // Sprite Renderer
            SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Player";

            // Physics
            Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            BoxCollider2D col = player.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 0.9f);
            col.offset = new Vector2(0, 0.05f);

            // Scripts
            PlayerMovement pm = player.AddComponent<PlayerMovement>();
            player.AddComponent<PlayerAnimation>();
            player.AddComponent<PlayerHealth>();

            // Ground Check child object
            GameObject groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(player.transform);
            groundCheck.transform.localPosition = new Vector3(0, -0.5f, 0);

            // Wire groundCheck to PlayerMovement
            SerializedObject pmSo = new SerializedObject(pm);
            SerializedProperty gcProp = pmSo.FindProperty("groundCheck");
            if (gcProp != null)
            {
                gcProp.objectReferenceValue = groundCheck.transform;
                pmSo.ApplyModifiedProperties();
            }

            Debug.Log("Mario created with all components!");
        }

        // === CAMERA ===
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.orthographic = true;
            mainCam.orthographicSize = 7.5f; // NES viewport: 240px / 16ppu / 2 = 7.5
            mainCam.backgroundColor = new Color(0.36f, 0.68f, 1f); // Azul cielo Mario
            mainCam.transform.position = new Vector3(7.5f, 6f, -10f);

            // Add CameraFollow if not present
            CameraFollow cf = mainCam.GetComponent<CameraFollow>();
            if (cf == null)
            {
                cf = mainCam.gameObject.AddComponent<CameraFollow>();
            }

            if (player != null)
            {
                SerializedObject cfSo = new SerializedObject(cf);
                SerializedProperty targetProp = cfSo.FindProperty("target");
                if (targetProp != null)
                {
                    targetProp.objectReferenceValue = player.transform;
                    cfSo.ApplyModifiedProperties();
                }
            }
        }

        // === GAME MANAGER ===
        if (Object.FindFirstObjectByType<GameManager>() == null)
        {
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
            Debug.Log("GameManager created!");
        }

        // === UI CANVAS ===
        GameObject existingCanvas = GameObject.Find("HUD Canvas");
        if (existingCanvas == null)
        {
            // Create Canvas
            GameObject canvas = new GameObject("HUD Canvas");
            Canvas c = canvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Add UIManager
            UIManager uiManager = canvas.AddComponent<UIManager>();

            // Create text elements
            var scoreTmp = CreateUIText(canvas.transform, "ScoreText", new Vector2(-350, 220), "MARIO\n000000");
            var coinTmp = CreateUIText(canvas.transform, "CoinText", new Vector2(-100, 220), "\u00d7 00");
            var worldTmp = CreateUIText(canvas.transform, "WorldText", new Vector2(100, 220), "WORLD\n1-1");
            var timeTmp = CreateUIText(canvas.transform, "TimeText", new Vector2(350, 220), "TIME\n400");

            // Wire text references to UIManager
            SerializedObject uiSo = new SerializedObject(uiManager);
            SerializedProperty pScore = uiSo.FindProperty("scoreText");
            SerializedProperty pCoin = uiSo.FindProperty("coinsText");
            SerializedProperty pWorld = uiSo.FindProperty("worldText");
            SerializedProperty pTime = uiSo.FindProperty("timeText");

            if (pScore != null) pScore.objectReferenceValue = scoreTmp;
            if (pCoin != null) pCoin.objectReferenceValue = coinTmp;
            if (pWorld != null) pWorld.objectReferenceValue = worldTmp;
            if (pTime != null) pTime.objectReferenceValue = timeTmp;
            uiSo.ApplyModifiedProperties();

            Debug.Log("HUD Canvas created!");
        }

        // === LEVEL BUILDER ===
        if (Object.FindFirstObjectByType<LevelBuilder>() == null)
        {
            GameObject lb = new GameObject("LevelBuilder");
            lb.AddComponent<LevelBuilder>();
            Debug.Log("LevelBuilder created!");
        }

        // Mark scene as dirty
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Scene Setup",
            "Mario scene has been set up!\n\n" +
            "Created:\n" +
            "- Camera (orthographic, NES viewport, tracking Mario)\n" +
            "- Mario (with components and groundCheck)\n" +
            "- GameManager\n" +
            "- HUD Canvas (with texts assigned)\n" +
            "- LevelBuilder\n\n" +
            "Next steps:\n" +
            "1. Run 'Tools > Configure Pixel Art Sprites'\n" +
            "2. Run 'Tools > Setup Mario Tags and Layers'\n" +
            "3. Create/assign prefabs in LevelBuilder",
            "OK");
    }

    private static TMPro.TextMeshProUGUI CreateUIText(Transform parent, string name, Vector2 position, string defaultText)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent);

        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(200, 60);

        var tmp = textObj.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = defaultText;
        tmp.fontSize = 18;
        tmp.color = Color.white;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        return tmp;
    }
}
