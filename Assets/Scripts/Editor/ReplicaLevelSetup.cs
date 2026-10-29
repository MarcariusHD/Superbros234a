using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;
using MarioBros.Player;
using MarioBros.Level;
using MarioBros.Enemies;
using MarioBros.Items;
using MarioBros.Blocks;
using MarioBros.CameraControl;
using MarioBros.UI;

/// <summary>Builds editable scenes from the 16-pixel grid in the reference maps.</summary>
public static class ReplicaLevelSetup
{
    private const string Art = "Assets/Sprites/Replica/";
    private static readonly int[] UndergroundSolid = { 0, 4, 5, 25, 26, 27, 32, 35, 36, 42, 43, 44, 47 };

    [MenuItem("Tools/Mario/Build Underground, Underwater and Game Over")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Deten Play antes de generar los niveles.");
            return;
        }
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        ImportArt();
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        MasterGameSetup.ExecuteFullSetup();
        ConfigurePlayer(false);
        BuildHud("1-1");
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        BuildLevel(false);
        BuildLevel(true);
        BuildOverworld();
        BuildGameOver();
        var paths = new[] { "SampleScene", "Underground", "Underwater", "GameOver" };
        var extras = EditorBuildSettings.scenes.Where(s => !paths.Any(p => s.path == "Assets/Scenes/" + p + ".unity"));
        EditorBuildSettings.scenes = paths.Select(p => new EditorBuildSettingsScene("Assets/Scenes/" + p + ".unity", true)).Concat(extras).ToArray();
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        Debug.Log("REPLICA_BUILD_OK: scenes and build settings saved.");
    }

    private static void ImportArt()
    {
        AssetDatabase.Refresh();
        foreach (string path in Directory.GetFiles(Art, "*.png"))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 16;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
    }

    private static void BuildLevel(bool water)
    {
        string name = water ? "Underwater" : "Underground";
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), "Assets/Scenes/" + name + ".unity", true);
        EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity");
        foreach (var builder in UnityEngine.Object.FindObjectsByType<LevelBuilder>(FindObjectsSortMode.None))
            UnityEngine.Object.DestroyImmediate(builder.gameObject);
        ClearReferenceLayouts();
        var root = new GameObject(name + " - reference tile layout").transform;
        string[] rows = File.ReadAllLines(Art + name + ".csv");
        for (int row = 0; row < rows.Length; row++)
        {
            int[] cells = rows[row].Split(',').Select(int.Parse).ToArray();
            for (int x = 0; x < cells.Length; x++)
            {
                int id = cells[x];
                int y = rows.Length - row - 1;
                bool solid = water ? id == 5 || id == 17 || id == 20 : UndergroundSolid.Contains(id);
                bool coin = id == (water ? 14 : 6);
                bool question = !water && id == 31;
                bool decoration = water && (id == 4 || id == 9);
                if (!solid && !coin && !question && !decoration) continue;
                var go = Tile(name + "_" + id, new Vector2(x + .5f, y + .5f), root);
                if (solid || question)
                {
                    go.layer = LayerMask.NameToLayer("Ground");
                    go.AddComponent<BoxCollider2D>().size = Vector2.one;
                }
                if (coin)
                {
                    go.AddComponent<CircleCollider2D>().radius = .35f;
                    go.AddComponent<Coin>();
                }
                if (question)
                {
                    var block = go.AddComponent<QuestionBlock>();
                    Set(block, "coinPrefab", AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Items/Coin.prefab"));
                    Set(block, "mushroomPrefab", AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Items/Mushroom.prefab"));
                    Set(block, "emptyBlockSprite", Sprite("Assets/Sprites/Block_Empty.png"));
                    if (x == 10) SetInt(block, "content", (int)QuestionBlockContent.Mushroom);
                }
                if (!water && id == 0 && x > 0)
                {
                    var brick = go.AddComponent<BrickBlock>();
                    Set(brick, "coinPrefab", AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Items/Coin.prefab"));
                }
            }
        }
        var player = ConfigurePlayer(water);
        player.transform.position = new Vector3(3, water ? 10 : 3, 0);
        Camera.main.backgroundColor = water ? new Color32(66,64,255,255) : Color.black;
        Camera.main.transform.position = new Vector3(8, 7.5f, -10);
        var follow = Camera.main.GetComponent<CameraFollow>();
        SetFloat(follow, "minX", 8);
        SetFloat(follow, "maxX", 184);
        SetFloat(follow, "fixedY", 7.5f);
        var limit = new GameObject("Left boundary");
        limit.layer = LayerMask.NameToLayer("Ground");
        limit.transform.position = new Vector3(-.5f,7,0);
        limit.AddComponent<BoxCollider2D>().size = new Vector2(1,30);
        if (water)
        {
            var ceiling = new GameObject("Water surface - swim limit");
            ceiling.layer = LayerMask.NameToLayer("Ground");
            ceiling.transform.position = new Vector3(96,13.5f,0);
            ceiling.AddComponent<BoxCollider2D>().size = new Vector2(192,1);
            foreach (Vector2 p in new[] {new Vector2(22,3),new Vector2(46,5),new Vector2(55,4),new Vector2(83,6),new Vector2(94,11),new Vector2(105,3)})
                SwimEnemy("Blooper", p, true, root);
            for (int x = 65; x < 185; x += 13)
                SwimEnemy("CheepCheep", new Vector2(x, 4 + x % 6), false, root);
            Exit(new Vector2(188.7f,7), "SampleScene", false);
        }
        else
        {
            foreach (int x in new[] {7,8,38,39,52,53,62,63,78,79,97,98,113,114,130})
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Goomba.prefab");
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                go.transform.SetParent(root); go.transform.position = new Vector3(x + .5f, 2.6f, 0);
                go.GetComponent<SpriteRenderer>().color = Color.white;
            }
            for (int i = 0; i < 3; i++)
            {
                Lift(141, 1 + i * 5, -2, root);
                Lift(156, 2 + i * 5, 2, root);
            }
            Exit(new Vector2(165.8f,6), "Underwater", false);
            foreach (int x in new[] {178,182,186}) Exit(new Vector2(x+1,5), "Underwater", true);
        }
        BuildHud(water ? "2-2" : "1-2");
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
    }

    private static GameObject ConfigurePlayer(bool water)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        var movement = player.GetComponent<PlayerMovement>();
        SetBool(movement, "underwater", water);
        player.GetComponent<SpriteRenderer>().sprite = Sprite("Assets/Resources/MarioFrames/Small_Idle.png");
        var health = new SerializedObject(player.GetComponent<PlayerHealth>());
        health.FindProperty("smallColliderOffset").vector2Value = new Vector2(0,.025f);
        health.FindProperty("bigColliderOffset").vector2Value = new Vector2(0,.525f);
        health.ApplyModifiedPropertiesWithoutUndo();
        return player;
    }

    private static void ClearReferenceLayouts()
    {
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            if (root.name.EndsWith(" - reference tile layout"))
                UnityEngine.Object.DestroyImmediate(root);
    }

    private static void BuildOverworld()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        foreach (var builder in UnityEngine.Object.FindObjectsByType<LevelBuilder>(FindObjectsSortMode.None))
            UnityEngine.Object.DestroyImmediate(builder.gameObject);
        ClearReferenceLayouts();
        var root = new GameObject("Overworld - reference tile layout").transform;
        var rows = File.ReadAllLines(Art + "Overworld.csv");
        int[] solidIds = {11,12,13,21,22,29,30,44};
        // The sheet includes editorial pipe arrows and an outline for a hidden block.
        int[] annotationIds = {0,15,16,17,18,19,23,24,31,32};
        Transform flag = null;
        for (int row = 0; row < rows.Length; row++)
        {
            var cells = rows[row].Split(',').Select(int.Parse).ToArray();
            for (int x = 0; x < cells.Length; x++)
            {
                int id = cells[x];
                if (annotationIds.Contains(id)) continue;
                var go = Tile("Overworld_" + id, new Vector2(x + .5f, rows.Length - row - .5f), root);
                bool solid = solidIds.Contains(id);
                go.GetComponent<SpriteRenderer>().sortingLayerName = solid ? "Ground" : "Background";
                if (solid)
                {
                    go.layer = LayerMask.NameToLayer("Ground");
                    go.AddComponent<BoxCollider2D>().size = Vector2.one;
                }
                if (id == 9) flag = go.transform;
                if (id == 11)
                {
                    var block = go.AddComponent<QuestionBlock>();
                    Set(block,"coinPrefab",AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Items/Coin.prefab"));
                    Set(block,"mushroomPrefab",AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Items/Mushroom.prefab"));
                    Set(block,"emptyBlockSprite",Sprite("Assets/Sprites/Block_Empty.png"));
                    if (x == 21 || (x == 109 && row == 5)) SetInt(block,"content",(int)QuestionBlockContent.Mushroom);
                }
                if (id == 12)
                {
                    var brick = go.AddComponent<BrickBlock>();
                    Set(brick,"coinPrefab",AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Items/Coin.prefab"));
                    Set(brick,"emptyBlockSprite",Sprite("Assets/Sprites/Block_Empty.png"));
                }
            }
        }
        foreach (int x in new[] {22,40,51,52,80,82,97,98,114,115,124,125,128,129,174,175})
            GroundEnemy("Goomba",new Vector2(x+.5f,2.6f),root);
        GroundEnemy("Koopa",new Vector2(107.5f,2.8f),root);
        var goal = new GameObject("Goal 1-1 to 1-2");goal.transform.SetParent(root);goal.transform.position = new Vector3(198.5f,2.5f,0);
        var trigger = goal.AddComponent<BoxCollider2D>();trigger.isTrigger = true;trigger.size = new Vector2(.4f,10);trigger.offset = new Vector2(0,5);
        var pole = goal.AddComponent<FlagPole>();
        Set(pole,"flagTransform",flag);
        var so = new SerializedObject(pole);so.FindProperty("nextSceneName").stringValue = "Underground";so.ApplyModifiedPropertiesWithoutUndo();
        var player = ConfigurePlayer(false);player.transform.position = new Vector3(3,2.6f,0);
        Camera.main.backgroundColor = new Color32(92,148,252,255);
        Camera.main.transform.position = new Vector3(8,7.5f,-10);
        var follow = Camera.main.GetComponent<CameraFollow>();SetFloat(follow,"minX",8);SetFloat(follow,"maxX",203);SetFloat(follow,"fixedY",7.5f);
        BuildHud("1-1");
        MasterGameSetup.ConfigureGlobalLighting();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
    }

    private static void GroundEnemy(string name,Vector2 position,Transform parent)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/"+name+".prefab");
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.transform.SetParent(parent);go.transform.position = position;
    }

    private static void SwimEnemy(string sprite, Vector2 position, bool blooper, Transform parent)
    {
        var go = Tile(sprite, position, parent);
        go.GetComponent<SpriteRenderer>().sortingLayerName = "Enemies";
        go.AddComponent<BoxCollider2D>().size = new Vector2(.7f,blooper ? 1.2f : .7f);
        go.GetComponent<BoxCollider2D>().isTrigger = true;
        go.AddComponent<SwimmingEnemy>().blooper = blooper;
    }

    private static void Lift(float x, float y, float speed, Transform parent)
    {
        var go = new GameObject("Looping elevator"); go.transform.SetParent(parent); go.transform.position = new Vector3(x,y,0);
        go.layer = LayerMask.NameToLayer("Ground");
        for (int i = 0; i < 4; i++) Tile("Underground_" + (i == 0 ? 18 : i == 3 ? 20 : 19),new Vector2(x+i-1.5f,y),go.transform);
        var collider = go.AddComponent<BoxCollider2D>(); collider.size = new Vector2(4,.25f);collider.offset = new Vector2(0,.32f);
        collider.usedByEffector = true;
        go.AddComponent<PlatformEffector2D>().useOneWay = true;
        go.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        go.AddComponent<LoopingLift>().speed = speed;
    }

    private static void Exit(Vector2 position, string scene, bool down)
    {
        var go = new GameObject("Pipe exit to " + scene); go.transform.position = position;
        var collider = go.AddComponent<BoxCollider2D>();collider.size = down ? new Vector2(1.6f,.4f) : new Vector2(.6f,1.7f);collider.isTrigger = true;
        var exit = go.AddComponent<LevelExit>();exit.nextScene = scene;exit.requireDown = down;
    }

    private static GameObject Tile(string sprite, Vector2 position, Transform parent)
    {
        var go = new GameObject(sprite); go.transform.SetParent(parent);go.transform.position = position;
        var renderer = go.AddComponent<SpriteRenderer>();renderer.sprite = Sprite(Art+sprite+".png");renderer.sortingLayerName = "Ground";
        return go;
    }

    private static Canvas Canvas(string name)
    {
        var go = new GameObject(name,typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.GetComponent<CanvasScaler>();scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution = new Vector2(960,540);scaler.matchWidthOrHeight = .5f;
        return canvas;
    }

    private static TextMeshProUGUI Text(Transform parent,string name,string value,Vector2 anchor,Vector2 position,Vector2 size,float fontSize)
    {
        var go = new GameObject(name,typeof(RectTransform)); go.transform.SetParent(parent,false);
        var rt = go.GetComponent<RectTransform>();rt.anchorMin = rt.anchorMax = anchor;rt.anchoredPosition = position;rt.sizeDelta = size;
        var text = go.AddComponent<TextMeshProUGUI>();text.text = value;text.fontSize = fontSize;text.alignment = TextAlignmentOptions.Center;text.color = Color.white;text.raycastTarget = false;
        text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        return text;
    }

    private static void BuildHud(string world)
    {
        var old = GameObject.Find("HUD Canvas");if (old != null) UnityEngine.Object.DestroyImmediate(old);
        var canvas = Canvas("HUD Canvas");var ui = canvas.gameObject.AddComponent<UIManager>();
        string[] labels = {"MARIO","COINS","WORLD","TIME","LIVES"};
        string[] fields = {"scoreText","coinsText","worldText","timeText","livesText"};
        for (int i=0;i<labels.Length;i++)
        {
            Vector2 anchor = new Vector2(.12f+i*.19f,1);
            Text(canvas.transform,labels[i],labels[i],anchor,new Vector2(0,-24),new Vector2(150,30),20);
            var value = Text(canvas.transform,fields[i],"",anchor,new Vector2(0,-51),new Vector2(150,30),22);
            Set(ui,fields[i],value);
        }
        var so = new SerializedObject(ui);so.FindProperty("currentWorldName").stringValue = world;so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void BuildGameOver()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var camera = new GameObject("Main Camera").AddComponent<Camera>();camera.tag = "MainCamera";camera.clearFlags = CameraClearFlags.SolidColor;camera.backgroundColor = Color.black;
        camera.gameObject.AddComponent<AudioListener>();
        var canvas = Canvas("Game Over Canvas");
        var screen = canvas.gameObject.AddComponent<GameOverScreen>();
        screen.score = Text(canvas.transform,"Final Score","",new Vector2(.5f,.8f),Vector2.zero,new Vector2(800,60),24);
        Text(canvas.transform,"Game Over","GAME OVER",new Vector2(.5f,.52f),Vector2.zero,new Vector2(800,80),44);
        Text(canvas.transform,"Restart hint","ENTER / ESPACIO PARA REINTENTAR",new Vector2(.5f,.35f),Vector2.zero,new Vector2(800,50),22);
        var button = new GameObject("Reintentar",typeof(RectTransform),typeof(Image),typeof(Button));button.transform.SetParent(canvas.transform,false);
        var rt = button.GetComponent<RectTransform>();rt.anchorMin = rt.anchorMax = new Vector2(.5f,.22f);rt.sizeDelta = new Vector2(240,50);
        button.GetComponent<Image>().color = new Color(.18f,.18f,.18f);
        Text(button.transform,"Label","REINTENTAR",new Vector2(.5f,.5f),Vector2.zero,new Vector2(240,50),22);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(button.GetComponent<Button>().onClick,screen.Restart);
        new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),"Assets/Scenes/GameOver.unity");
    }

    private static Sprite Sprite(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);
    private static void Set(UnityEngine.Object obj,string field,UnityEngine.Object value) {var s=new SerializedObject(obj);s.FindProperty(field).objectReferenceValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
    private static void SetFloat(UnityEngine.Object obj,string field,float value) {var s=new SerializedObject(obj);s.FindProperty(field).floatValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
    private static void SetInt(UnityEngine.Object obj,string field,int value) {var s=new SerializedObject(obj);s.FindProperty(field).intValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
    private static void SetBool(UnityEngine.Object obj,string field,bool value) {var s=new SerializedObject(obj);s.FindProperty(field).boolValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
}
