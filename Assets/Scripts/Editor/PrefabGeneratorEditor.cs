using System.IO;
using UnityEditor;
using UnityEngine;
using MarioBros.Blocks;
using MarioBros.Enemies;
using MarioBros.Items;
using MarioBros.Level;

/// <summary>
/// Editor utility to generate all essential Mario prefabs and connect them to LevelBuilder.
/// Run from menu: Tools > Generate Mario Prefabs
/// </summary>
public class PrefabGeneratorEditor : EditorWindow
{
    [MenuItem("Tools/Generate Mario Prefabs")]
    public static void GenerateAllPrefabs()
    {
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Prefabs/Blocks");
        EnsureFolder("Assets/Prefabs/Enemies");
        EnsureFolder("Assets/Prefabs/Items");
        EnsureFolder("Assets/Prefabs/Level");

        // 1. Blocks
        GameObject groundPrefab = CreateGroundPrefab("GroundBlock", "Assets/Prefabs/Blocks/GroundBlock.prefab");
        GameObject brickPrefab = CreateBrickPrefab("BrickBlock", "Assets/Prefabs/Blocks/BrickBlock.prefab");
        GameObject hardPrefab = CreateGroundPrefab("HardBlock", "Assets/Prefabs/Blocks/HardBlock.prefab");
        GameObject questionPrefab = CreateQuestionBlockPrefab("QuestionBlock", "Assets/Prefabs/Blocks/QuestionBlock.prefab");

        // 2. Items
        GameObject coinPrefab = CreateCoinPrefab("Coin", "Assets/Prefabs/Items/Coin.prefab");
        GameObject mushroomPrefab = CreateMushroomPrefab("Mushroom", "Assets/Prefabs/Items/Mushroom.prefab");
        GameObject starPrefab = CreateStarmanPrefab("Starman", "Assets/Prefabs/Items/Starman.prefab");

        // Link item prefabs to blocks
        if (questionPrefab != null)
        {
            var qb = questionPrefab.GetComponent<QuestionBlock>();
            if (qb != null)
            {
                SerializedObject so = new SerializedObject(qb);
                if (coinPrefab != null) so.FindProperty("coinPrefab").objectReferenceValue = coinPrefab;
                if (mushroomPrefab != null) so.FindProperty("mushroomPrefab").objectReferenceValue = mushroomPrefab;
                if (starPrefab != null) so.FindProperty("starPrefab").objectReferenceValue = starPrefab;
                so.ApplyModifiedProperties();
            }
        }

        if (brickPrefab != null && coinPrefab != null)
        {
            var bb = brickPrefab.GetComponent<BrickBlock>();
            if (bb != null)
            {
                SerializedObject so = new SerializedObject(bb);
                so.FindProperty("coinPrefab").objectReferenceValue = coinPrefab;
                so.ApplyModifiedProperties();
            }
        }

        // 3. Enemies
        GameObject goombaPrefab = CreateGoombaPrefab("Goomba", "Assets/Prefabs/Enemies/Goomba.prefab");
        GameObject koopaPrefab = CreateKoopaPrefab("Koopa", "Assets/Prefabs/Enemies/Koopa.prefab");

        // 4. Level & Goal
        GameObject pipePrefab = CreatePipePrefab("Pipe", "Assets/Prefabs/Level/Pipe.prefab");
        GameObject flagPolePrefab = CreateFlagPolePrefab("FlagPole", "Assets/Prefabs/Level/FlagPole.prefab");
        GameObject castlePrefab = CreateSimpleObjectPrefab("Castle", "Assets/Prefabs/Level/Castle.prefab", "Decoration");
        GameObject cloudPrefab = CreateSimpleObjectPrefab("Cloud", "Assets/Prefabs/Level/Cloud.prefab", "Background");
        GameObject bushPrefab = CreateSimpleObjectPrefab("Bush", "Assets/Prefabs/Level/Bush.prefab", "Decoration");
        GameObject hillPrefab = CreateSimpleObjectPrefab("Hill", "Assets/Prefabs/Level/Hill.prefab", "Background");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 5. Connect to LevelBuilder if present in scene
        LevelBuilder lb = Object.FindFirstObjectByType<LevelBuilder>();
        if (lb != null)
        {
            SerializedObject lbSo = new SerializedObject(lb);
            AssignProp(lbSo, "groundBlockPrefab", groundPrefab);
            AssignProp(lbSo, "brickBlockPrefab", brickPrefab);
            AssignProp(lbSo, "questionBlockPrefab", questionPrefab);
            AssignProp(lbSo, "hardBlockPrefab", hardPrefab);
            AssignProp(lbSo, "pipePrefab", pipePrefab);

            AssignProp(lbSo, "coinPrefab", coinPrefab);
            AssignProp(lbSo, "goombaPrefab", goombaPrefab);
            AssignProp(lbSo, "koopaPrefab", koopaPrefab);
            AssignProp(lbSo, "flagPolePrefab", flagPolePrefab);
            AssignProp(lbSo, "castlePrefab", castlePrefab);

            AssignProp(lbSo, "cloudPrefab", cloudPrefab);
            AssignProp(lbSo, "bushPrefab", bushPrefab);
            AssignProp(lbSo, "hillPrefab", hillPrefab);

            lbSo.ApplyModifiedProperties();
            Debug.Log("Prefabs successfully assigned to LevelBuilder in the scene!");
        }

        EditorUtility.DisplayDialog("Mario Prefabs",
            "Prefabs created successfully in Assets/Prefabs/!\n\n" +
            "- Ground, Brick, Question, HardBlock\n" +
            "- Pipe, FlagPole, Castle, Decorations\n" +
            "- Goomba, Koopa\n" +
            "- Coin, Mushroom, Starman\n\n" +
            (lb != null ? "Assigned to LevelBuilder in active scene." : "Add a LevelBuilder to scene and run again to link."),
            "OK");
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

    private static void AssignProp(SerializedObject so, string propName, GameObject val)
    {
        if (val == null) return;
        SerializedProperty prop = so.FindProperty(propName);
        if (prop != null) prop.objectReferenceValue = val;
    }

    private static GameObject CreateGroundPrefab(string name, string path)
    {
        GameObject go = new GameObject(name);
        go.tag = "Ground";
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer != -1) go.layer = groundLayer;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Ground";

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        DestroyImmediate(go);
        return prefab;
    }

    private static GameObject CreateBrickPrefab(string name, string path)
    {
        GameObject go = new GameObject(name);
        go.tag = "BrickBlock";
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer != -1) go.layer = groundLayer;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Blocks";

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;

        go.AddComponent<BrickBlock>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        DestroyImmediate(go);
        return prefab;
    }

    private static GameObject CreateQuestionBlockPrefab(string name, string path)
    {
        GameObject go = new GameObject(name);
        go.tag = "QuestionBlock";
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer != -1) go.layer = groundLayer;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Blocks";

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;

        go.AddComponent<QuestionBlock>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        DestroyImmediate(go);
        return prefab;
    }

    private static GameObject CreateCoinPrefab(string name, string path)
    {
        GameObject go = new GameObject(name);
        go.tag = "Coin";
        int itemLayer = LayerMask.NameToLayer("ItemLayer");
        if (itemLayer != -1) go.layer = itemLayer;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Items";

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;
        col.isTrigger = true;

        go.AddComponent<Coin>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        DestroyImmediate(go);
        return prefab;
    }

    private static GameObject CreateMushroomPrefab(string name, string path)
    {
        GameObject go = new GameObject(name);
        go.tag = "PowerUp";
        int itemLayer = LayerMask.NameToLayer("ItemLayer");
        if (itemLayer != -1) go.layer = itemLayer;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Items";

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.9f, 0.9f);

        go.AddComponent<Mushroom>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        DestroyImmediate(go);
        return prefab;
    }

    private static GameObject CreateStarmanPrefab(string name, string path)
    {
        GameObject go = new GameObject(name);
        go.tag = "PowerUp";
        int itemLayer = LayerMask.NameToLayer("ItemLayer");
        if (itemLayer != -1) go.layer = itemLayer;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Items";

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.9f, 0.9f);

        go.AddComponent<Starman>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        DestroyImmediate(go);
        return prefab;
    }

    private static GameObject CreateGoombaPrefab(string name, string path)
    {
        GameObject go = new GameObject(name);
        go.tag = "Enemy";
        int enemyLayer = LayerMask.NameToLayer("EnemyLayer");
        if (enemyLayer != -1) go.layer = enemyLayer;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Enemies";

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.85f, 0.85f);

        go.AddComponent<Goomba>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        DestroyImmediate(go);
        return prefab;
    }

    private static GameObject CreateKoopaPrefab(string name, string path)
    {
        GameObject go = new GameObject(name);
        go.tag = "Enemy";
        int enemyLayer = LayerMask.NameToLayer("EnemyLayer");
        if (enemyLayer != -1) go.layer = enemyLayer;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Enemies";

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.85f, 1.4f);

        go.AddComponent<KoopaController>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        DestroyImmediate(go);
        return prefab;
    }

    private static GameObject CreatePipePrefab(string name, string path)
    {
        GameObject go = new GameObject(name);
        go.tag = "Pipe";
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer != -1) go.layer = groundLayer;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Blocks";

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(2f, 2f);

        go.AddComponent<Pipe>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        DestroyImmediate(go);
        return prefab;
    }

    private static GameObject CreateFlagPolePrefab(string name, string path)
    {
        GameObject go = new GameObject(name);
        go.tag = "FlagPole";

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Decoration";

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.5f, 9.5f);
        col.offset = new Vector2(0, 4.75f);

        go.AddComponent<FlagPole>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        DestroyImmediate(go);
        return prefab;
    }

    private static GameObject CreateSimpleObjectPrefab(string name, string path, string sortingLayer)
    {
        GameObject go = new GameObject(name);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = sortingLayer;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        DestroyImmediate(go);
        return prefab;
    }
}
