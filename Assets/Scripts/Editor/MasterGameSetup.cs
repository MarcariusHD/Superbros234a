using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using MarioBros.Blocks;
using MarioBros.CameraControl;
using MarioBros.Enemies;
using MarioBros.Items;
using MarioBros.Level;
using MarioBros.Player;
using MarioBros.UI;

public static class MasterGameSetup
{

    private static T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (comp == null) comp = go.AddComponent<T>();
        return comp;
    }

    [MenuItem("Tools/Setup Mario - Ready To Play (1-Click)")]
    public static void ExecuteFullSetup()
    {
        Debug.Log(">>> [MasterGameSetup] Starting 1-Click Game Setup...");

        OriginalSpriteSetup.Extract();
        // 1. Configure Sprites
        ConfigureAllSprites();

        // 2. Generate and wire all Prefabs
        GameObject groundPrefab = CreateOrUpdatePrefab("Assets/Prefabs/Blocks/GroundBlock.prefab", "GroundBlock", "Ground", "Ground", "Assets/Sprites/Block_Ground.png", go =>
        {
            var col = GetOrAddComponent<BoxCollider2D>(go);
            col.size = Vector2.one;
        });

        GameObject brickPrefab = CreateOrUpdatePrefab("Assets/Prefabs/Blocks/BrickBlock.prefab", "BrickBlock", "BrickBlock", "Blocks", "Assets/Sprites/Block_Brick.png", go =>
        {
            var col = GetOrAddComponent<BoxCollider2D>(go);
            col.size = Vector2.one;
            GetOrAddComponent<BrickBlock>(go);
        });

        GameObject hardPrefab = CreateOrUpdatePrefab("Assets/Prefabs/Blocks/HardBlock.prefab", "HardBlock", "Ground", "Blocks", "Assets/Sprites/Block_Hard.png", go =>
        {
            var col = GetOrAddComponent<BoxCollider2D>(go);
            col.size = Vector2.one;
        });

        GameObject questionPrefab = CreateOrUpdatePrefab("Assets/Prefabs/Blocks/QuestionBlock.prefab", "QuestionBlock", "QuestionBlock", "Blocks", "Assets/Sprites/Block_Question.png", go =>
        {
            var col = GetOrAddComponent<BoxCollider2D>(go);
            col.size = Vector2.one;
            GetOrAddComponent<QuestionBlock>(go);
        });

        GameObject coinPrefab = CreateOrUpdatePrefab("Assets/Prefabs/Items/Coin.prefab", "Coin", "Coin", "Items", "Assets/Sprites/Item_Coin.png", go =>
        {
            var col = GetOrAddComponent<CircleCollider2D>(go);
            col.radius = 0.4f;
            col.isTrigger = true;
            GetOrAddComponent<Coin>(go);
        });

        GameObject mushroomPrefab = CreateOrUpdatePrefab("Assets/Prefabs/Items/Mushroom.prefab", "Mushroom", "PowerUp", "Items", "Assets/Sprites/Item_Mushroom.png", go =>
        {
            var rb = GetOrAddComponent<Rigidbody2D>(go);
            rb.freezeRotation = true;
            var col = GetOrAddComponent<BoxCollider2D>(go);
            col.size = new Vector2(0.9f, 0.9f);
            GetOrAddComponent<Mushroom>(go);
        });

        GameObject starPrefab = CreateOrUpdatePrefab("Assets/Prefabs/Items/Starman.prefab", "Starman", "PowerUp", "Items", "Assets/Sprites/Item_Starman.png", go =>
        {
            var rb = GetOrAddComponent<Rigidbody2D>(go);
            rb.freezeRotation = true;
            var col = GetOrAddComponent<BoxCollider2D>(go);
            col.size = new Vector2(0.9f, 0.9f);
            GetOrAddComponent<Starman>(go);
        });

        // Wire empty block & items into blocks
        Sprite emptyBlockSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Block_Empty.png");
        if (questionPrefab != null)
        {
            var qb = questionPrefab.GetComponent<QuestionBlock>();
            if (qb != null)
            {
                var so = new SerializedObject(qb);
                if (coinPrefab != null) so.FindProperty("coinPrefab").objectReferenceValue = coinPrefab;
                if (mushroomPrefab != null) so.FindProperty("mushroomPrefab").objectReferenceValue = mushroomPrefab;
                if (starPrefab != null) so.FindProperty("starmanPrefab").objectReferenceValue = starPrefab;
                if (emptyBlockSprite != null) so.FindProperty("emptyBlockSprite").objectReferenceValue = emptyBlockSprite;
                so.ApplyModifiedProperties();
            }
        }

        if (brickPrefab != null)
        {
            var bb = brickPrefab.GetComponent<BrickBlock>();
            if (bb != null)
            {
                var so = new SerializedObject(bb);
                if (coinPrefab != null) so.FindProperty("coinPrefab").objectReferenceValue = coinPrefab;
                if (emptyBlockSprite != null) so.FindProperty("emptyBlockSprite").objectReferenceValue = emptyBlockSprite;
                so.ApplyModifiedProperties();
            }
        }

        GameObject goombaPrefab = CreateOrUpdatePrefab("Assets/Prefabs/Enemies/Goomba.prefab", "Goomba", "Enemy", "Enemies", "Assets/Sprites/Goomba_Walk1.png", go =>
        {
            var rb = GetOrAddComponent<Rigidbody2D>(go);
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var col = GetOrAddComponent<BoxCollider2D>(go);
            col.size = new Vector2(0.85f, 0.85f);
            GetOrAddComponent<Goomba>(go);
        });

        GameObject koopaPrefab = CreateOrUpdatePrefab("Assets/Prefabs/Enemies/Koopa.prefab", "Koopa", "Enemy", "Enemies", "Assets/Sprites/Koopa_Walk1.png", go =>
        {
            var rb = GetOrAddComponent<Rigidbody2D>(go);
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var col = GetOrAddComponent<BoxCollider2D>(go);
            col.size = new Vector2(0.85f, 1.4f);
            GetOrAddComponent<KoopaController>(go);
        });

        GameObject pipePrefab = CreateOrUpdatePrefab("Assets/Prefabs/Level/Pipe.prefab", "Pipe", "Pipe", "Blocks", "Assets/Sprites/Pipe.png", go =>
        {
            var col = GetOrAddComponent<BoxCollider2D>(go);
            col.size = new Vector2(2f, 2f);
            GetOrAddComponent<Pipe>(go);
        });

        GameObject flagPolePrefab = CreateOrUpdatePrefab("Assets/Prefabs/Level/FlagPole.prefab", "FlagPole", "FlagPole", "Decoration", "Assets/Sprites/FlagPole.png", go =>
        {
            var col = GetOrAddComponent<BoxCollider2D>(go);
            col.isTrigger = true;
            col.size = new Vector2(0.5f, 9.5f);
            col.offset = new Vector2(0, 4.75f);
            GetOrAddComponent<FlagPole>(go);
        });

        GameObject castlePrefab = CreateOrUpdatePrefab("Assets/Prefabs/Level/Castle.prefab", "Castle", "Untagged", "Decoration", "Assets/Sprites/Castle.png", null);
        GameObject cloudPrefab = CreateOrUpdatePrefab("Assets/Prefabs/Level/Cloud.prefab", "Cloud", "Untagged", "Background", "Assets/Sprites/Cloud.png", null);
        GameObject bushPrefab = CreateOrUpdatePrefab("Assets/Prefabs/Level/Bush.prefab", "Bush", "Untagged", "Decoration", "Assets/Sprites/Bush.png", null);
        GameObject hillPrefab = CreateOrUpdatePrefab("Assets/Prefabs/Level/Hill.prefab", "Hill", "Untagged", "Background", "Assets/Sprites/Hill.png", null);

        AssetDatabase.SaveAssets();

        // 3. Set up the Scene
        SetupSceneObjects(groundPrefab, brickPrefab, questionPrefab, hardPrefab, pipePrefab,
                         coinPrefab, goombaPrefab, koopaPrefab, flagPolePrefab, castlePrefab,
                         cloudPrefab, bushPrefab, hillPrefab);

        ConfigureGlobalLighting();

        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveOpenScenes();
        Debug.Log(">>> [MasterGameSetup] 1-Click Game Setup Complete! You can press PLAY now!");
    }

    [MenuItem("Tools/Mario/Fix Lighting in Current Scene")]
    public static void ConfigureGlobalLighting()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Deten Play antes de corregir la iluminacion de la escena.");
            return;
        }

        var lights = UnityEngine.Object.FindObjectsByType<UnityEngine.Rendering.Universal.Light2D>(FindObjectsSortMode.None);
        foreach (var light in lights)
        {
            if (light.lightType != UnityEngine.Rendering.Universal.Light2D.LightType.Global) continue;
            var serialized = new SerializedObject(light);
            var layers = serialized.FindProperty("m_ApplyToSortingLayers");
            if (layers == null)
                throw new InvalidOperationException("No se encontraron las capas de la luz global: " + light.name);
            var sortingLayers = SortingLayer.layers;
            layers.arraySize = sortingLayers.Length;
            for (int i = 0; i < sortingLayers.Length; i++)
                layers.GetArrayElementAtIndex(i).intValue = sortingLayers[i].id;
            serialized.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(light.gameObject.scene);
        }
    }

    private static void ConfigureAllSprites()
    {
        EnsureFolder("Assets/Sprites");
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Prefabs/Blocks");
        EnsureFolder("Assets/Prefabs/Enemies");
        EnsureFolder("Assets/Prefabs/Items");
        EnsureFolder("Assets/Prefabs/Level");

        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Sprites" });
        bool anyChanged = false;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                if (importer.textureType != TextureImporterType.Sprite ||
                    importer.filterMode != FilterMode.Point ||
                    importer.textureCompression != TextureImporterCompression.Uncompressed ||
                    importer.spritePixelsPerUnit != 16)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.filterMode = FilterMode.Point;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.spritePixelsPerUnit = 16;
                    importer.mipmapEnabled = false;
                    importer.maxTextureSize = 2048;
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                    anyChanged = true;
                }
            }
        }

        if (anyChanged)
        {
            AssetDatabase.Refresh();
        }
    }

    private static GameObject CreateOrUpdatePrefab(string prefabPath, string name, string tag, string sortingLayer, string spritePath, Action<GameObject> configure)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        GameObject go = new GameObject(name);
        try
        {
            if (!string.IsNullOrEmpty(tag) && tag != "Untagged")
            {
                try { go.tag = tag; } catch { }
            }

            int layer = LayerMask.NameToLayer(tag == "Enemy" ? "EnemyLayer" : (tag == "Coin" || tag == "PowerUp" ? "ItemLayer" : (tag == "Ground" || tag == "BrickBlock" || tag == "QuestionBlock" || tag == "Pipe" ? "Ground" : "Default")));
            if (layer != -1) go.layer = layer;

            SpriteRenderer sr = GetOrAddComponent<SpriteRenderer>(go);
            sr.sprite = sprite;
            sr.sortingLayerName = sortingLayer;

            configure?.Invoke(go);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            return prefab;
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }

    private static void SetupSceneObjects(
        GameObject ground, GameObject brick, GameObject question, GameObject hard, GameObject pipe,
        GameObject coin, GameObject goomba, GameObject koopa, GameObject flag, GameObject castle,
        GameObject cloud, GameObject bush, GameObject hill)
    {
        // === MARIO ===
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("Mario");
        if (player == null)
        {
            player = new GameObject("Mario");
            player.transform.position = new Vector3(2.5f, 2f, 0f);
        }

        try { player.tag = "Player"; } catch { }
        int pLayer = LayerMask.NameToLayer("PlayerLayer");
        if (pLayer != -1) player.layer = pLayer;

        SpriteRenderer sr = GetOrAddComponent<SpriteRenderer>(player);
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Mario_Small_Idle.png");
        sr.sortingLayerName = "Player";

        Rigidbody2D rb = GetOrAddComponent<Rigidbody2D>(player);
        rb.gravityScale = 3f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        BoxCollider2D col = GetOrAddComponent<BoxCollider2D>(player);
        col.size = new Vector2(0.8f, 0.9f);
        col.offset = new Vector2(0, 0.05f);

        PlayerMovement pm = GetOrAddComponent<PlayerMovement>(player);
        GetOrAddComponent<PlayerAnimation>(player);
        GetOrAddComponent<PlayerHealth>(player);

        Transform groundCheck = player.transform.Find("GroundCheck");
        if (groundCheck == null)
        {
            GameObject gcObj = new GameObject("GroundCheck");
            groundCheck = gcObj.transform;
            groundCheck.SetParent(player.transform);
            groundCheck.localPosition = new Vector3(0, -0.5f, 0);
        }

        var pmSo = new SerializedObject(pm);
        var gcProp = pmSo.FindProperty("groundCheck");
        if (gcProp != null) gcProp.objectReferenceValue = groundCheck;
        pmSo.ApplyModifiedProperties();

        // === CAMERA ===
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.orthographic = true;
            mainCam.orthographicSize = 7.5f;
            mainCam.backgroundColor = new Color(0.36f, 0.68f, 1f);
            mainCam.transform.position = new Vector3(7.5f, 6f, -10f);

            CameraFollow cf = GetOrAddComponent<CameraFollow>(mainCam.gameObject);
            var cfSo = new SerializedObject(cf);
            var targetProp = cfSo.FindProperty("target");
            if (targetProp != null) targetProp.objectReferenceValue = player.transform;
            cfSo.ApplyModifiedProperties();
        }

        // === GAME MANAGER ===
        if (UnityEngine.Object.FindFirstObjectByType<GameManager>() == null)
        {
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
        }

        // === HUD CANVAS ===
        GameObject canvasObj = GameObject.Find("HUD Canvas");
        if (canvasObj == null)
        {
            canvasObj = new GameObject("HUD Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            UIManager uim = canvasObj.AddComponent<UIManager>();

            var scoreTmp = CreateUIText(canvasObj.transform, "ScoreText", new Vector2(-350, 220), "MARIO\n000000");
            var coinTmp = CreateUIText(canvasObj.transform, "CoinText", new Vector2(-100, 220), "\u00d7 00");
            var worldTmp = CreateUIText(canvasObj.transform, "WorldText", new Vector2(100, 220), "WORLD\n1-1");
            var timeTmp = CreateUIText(canvasObj.transform, "TimeText", new Vector2(350, 220), "TIME\n400");

            var uimSo = new SerializedObject(uim);
            var pScore = uimSo.FindProperty("scoreText");
            var pCoin = uimSo.FindProperty("coinsText");
            var pWorld = uimSo.FindProperty("worldText");
            var pTime = uimSo.FindProperty("timeText");
            if (pScore != null) pScore.objectReferenceValue = scoreTmp;
            if (pCoin != null) pCoin.objectReferenceValue = coinTmp;
            if (pWorld != null) pWorld.objectReferenceValue = worldTmp;
            if (pTime != null) pTime.objectReferenceValue = timeTmp;
            uimSo.ApplyModifiedProperties();
        }

        // === LEVEL BUILDER ===
        LevelBuilder lb = UnityEngine.Object.FindFirstObjectByType<LevelBuilder>();
        if (lb == null)
        {
            GameObject lbObj = new GameObject("LevelBuilder");
            lb = lbObj.AddComponent<LevelBuilder>();
        }

        SerializedObject lbSo = new SerializedObject(lb);
        SetProp(lbSo, "groundBlockPrefab", ground);
        SetProp(lbSo, "brickBlockPrefab", brick);
        SetProp(lbSo, "questionBlockPrefab", question);
        SetProp(lbSo, "hardBlockPrefab", hard);
        SetProp(lbSo, "pipePrefab", pipe);
        SetProp(lbSo, "coinPrefab", coin);
        SetProp(lbSo, "goombaPrefab", goomba);
        SetProp(lbSo, "koopaPrefab", koopa);
        SetProp(lbSo, "flagPolePrefab", flag);
        SetProp(lbSo, "castlePrefab", castle);
        SetProp(lbSo, "cloudPrefab", cloud);
        SetProp(lbSo, "bushPrefab", bush);
        SetProp(lbSo, "hillPrefab", hill);
        SetPropBool(lbSo, "buildOnStart", true);
        lbSo.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    private static TextMeshProUGUI CreateUIText(Transform parent, string name, Vector2 pos, string text)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(200, 60);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 20;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        return tmp;
    }

    private static void SetProp(SerializedObject so, string name, GameObject obj)
    {
        if (obj == null) return;
        var prop = so.FindProperty(name);
        if (prop != null) prop.objectReferenceValue = obj;
    }

    private static void SetPropBool(SerializedObject so, string name, bool val)
    {
        var prop = so.FindProperty(name);
        if (prop != null) prop.boolValue = val;
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path).Replace("\\", "/");
            string folder = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
