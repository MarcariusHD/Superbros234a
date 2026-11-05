using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MarioBros.Blocks;
using MarioBros.CameraControl;
using MarioBros.Enemies;
using MarioBros.Items;
using MarioBros.Level;
using MarioBros.Player;
using MarioBros.UI;
using MarioBros.World1_4;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Herramienta de autoría: deja la aventura completa guardada y editable, sin iniciar el juego.</summary>
public static class World1_4AdventureBuilder
{
    private const string Art = "Assets/Resources/World1_4/";
    private const string Prefabs = "Assets/Prefabs/World1_4/";
    private const string ScenePath = "Assets/Scenes/World1_4.unity";
    private static AdventureLevel level;
    private static Transform root;
    private static Material spriteMaterial;
    private static AdventureZone exterior, cave, water, mixed, lava, arenaZone, secret;
    private static readonly Color Sky = new Color32(92, 148, 252, 255);
    private static readonly Color Gold = new Color32(248, 184, 0, 255);

    [MenuItem("Tools/Mario/Crear Mundo 1-4 - La aventura final")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Esta herramienta trabaja exclusivamente fuera de Play Mode.");
        if (File.Exists(ScenePath)) throw new InvalidOperationException("Ya existe World1_4.unity. Se conserva para no sobrescribir trabajo local.");
        CreateArt();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive); SceneManager.SetActiveScene(scene);
        root = Group("MUNDO 1-4 — LA AVENTURA FINAL", null);
        level = root.gameObject.AddComponent<AdventureLevel>();
        Services();
        CreateZones();
        Exterior();
        Caverns();
        WaterPassage();
        FourWorlds();
        LavaApproach();
        SecretRoom();
        BossArena();
        Interface();
        ConnectPipes();
        Solid("Límite exterior", -.5f, 7, 1, 28, exterior.transform);
        Solid("Límite final", 346.5f, 7, 1, 28, arenaZone.transform);
        EditorSceneManager.SaveScene(scene, ScenePath);
        var scenes = EditorBuildSettings.scenes.ToList();
        int index = scenes.FindIndex(s => s.path == ScenePath);
        if (index < 0) scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
        else scenes[index] = new EditorBuildSettingsScene(ScenePath, true);
        EditorBuildSettings.scenes = scenes.ToArray(); AssetDatabase.SaveAssets();
        for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            var other = SceneManager.GetSceneAt(i);
            if (other != scene && !other.isDirty) EditorSceneManager.CloseScene(other, true);
        }
        Selection.activeGameObject = root.gameObject;
        if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.LookAt(new Vector3(13, 7.5f, 0), Quaternion.identity, 9, true, true);
        Debug.Log("Mundo 1-4: aventura construida y guardada. No se ha ejecutado el juego ni pruebas.");
    }

    private static void CreateArt()
    {
        Directory.CreateDirectory(Art); Directory.CreateDirectory(Prefabs);
        var source = Read("Assets/art/SourceSheets/NES - Super Mario Bros. - Enemies & Bosses - Enemies & Bosses (1).png");
        Cut(source, "Bowser_WalkA", 68, 208, 32, 32, true, true);
        Cut(source, "Bowser_WalkB", 102, 208, 32, 32, true, true);
        Cut(source, "Bowser_FireA", 0, 208, 32, 32, true, true);
        Cut(source, "Bowser_FireB", 34, 208, 32, 32, true, true);
        Cut(source, "FireA", 102, 242, 24, 8, true, false); Cut(source, "FireB", 102, 252, 24, 8, true, false);
        Object.DestroyImmediate(source);
        var castle = Read("Assets/art/LevelReferences/NES - Super Mario Bros. - Stages - World 1-4.png");
        Cut(castle, "CastleStone", 16, 32, 16, 16, false, false);
        Cut(castle, "LavaBody", 208, 208, 16, 16, false, false);
        var wave = new Texture2D(16, 16, TextureFormat.RGBA32, false); wave.SetPixels(castle.GetPixels(208, castle.height - 208, 16, 16));
        for (int f = 0; f < 4; f++)
        {
            var frame = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            for (int y = 0; y < 16; y++) for (int x = 0; x < 16; x++) frame.SetPixel(x, y, wave.GetPixel((x + f * 4) % 16, y));
            Save(frame, "LavaSurface" + f);
        }
        Object.DestroyImmediate(wave); Object.DestroyImmediate(castle);
        // Único píxel neutro: fondos planos NES e interfaz; los objetos usan sus sprites originales.
        var pixel = new Texture2D(1, 1, TextureFormat.RGBA32, false); pixel.SetPixel(0, 0, Color.white); Save(pixel, "Pixel");
        SpanishGlyphs();
        AssetDatabase.Refresh();
        foreach (string path in Directory.GetFiles(Art, "*.png"))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 16; importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.npotScale = TextureImporterNPOTScale.None;
            importer.alphaIsTransparency = true;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom; settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spritePivot = Path.GetFileName(path).StartsWith("Bowser_") ? new Vector2(.5f, 0) : new Vector2(.5f, .5f);
            importer.SetTextureSettings(settings); importer.SaveAndReimport();
        }
        spriteMaterial = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
        AssetDatabase.CreateAsset(spriteMaterial, Art + "NesSprites.mat");
    }
    private static void SpanishGlyphs()
    {
        var original = Read("Assets/Resources/ClassicUI/Font.png");
        string chars = NesText.SpanishCharacters;
        var output = new Texture2D(chars.Length * 8, 8, TextureFormat.RGBA32, false); output.SetPixels(new Color[chars.Length * 64]);
        string accents = "ÁÉÍÓÚÑ", letters = "AEIOUN";
        for (int i = 0; i < accents.Length; i++)
        {
            int from = NesText.Characters.IndexOf(letters[i]), to = chars.IndexOf(accents[i]);
            output.SetPixels(to * 8, 0, 8, 8, original.GetPixels(from * 8, 0, 8, 8));
            if (i < 5) { output.SetPixel(to * 8 + 4, 7, Color.white); output.SetPixel(to * 8 + 5, 7, Color.white); }
            else foreach (int x in new[] { 1, 2, 4, 5 }) output.SetPixel(to * 8 + x, 7, Color.white);
        }
        int inverse = chars.IndexOf('¡'), bang = NesText.Characters.IndexOf('!');
        for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++) output.SetPixel(inverse * 8 + x, y, original.GetPixel(bang * 8 + x, 7 - y));
        string[] special = { "¿", "—", ":", "/" };
        string[][] rows = {
            new[] {"00110000","00000000","00110000","01100000","11000000","11000110","01111100","00000000"},
            new[] {"00000000","00000000","00000000","11111111","11111111","00000000","00000000","00000000"},
            new[] {"00000000","00110000","00110000","00000000","00110000","00110000","00000000","00000000"},
            new[] {"00000011","00000110","00001100","00011000","00110000","01100000","11000000","00000000"}
        };
        for (int c = 0; c < special.Length; c++) for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++)
            if (rows[c][y][x] == '1') output.SetPixel(chars.IndexOf(special[c][0]) * 8 + x, 7 - y, Color.white);
        Save(output, "SpanishGlyphs"); Object.DestroyImmediate(original);
    }
    private static Texture2D Read(string path) { var t = new Texture2D(2, 2); t.LoadImage(File.ReadAllBytes(path)); return t; }
    private static void Save(Texture2D texture, string name) { texture.Apply(); File.WriteAllBytes(Art + name + ".png", texture.EncodeToPNG()); Object.DestroyImmediate(texture); }
    private static void Cut(Texture2D source, string name, int x, int top, int width, int height, bool transparent, bool flip)
    {
        var t = new Texture2D(width, height, TextureFormat.RGBA32, false);
        for (int y = 0; y < height; y++) for (int xx = 0; xx < width; xx++)
        {
            Color32 c = source.GetPixel(x + xx, source.height - top - height + y);
            bool bg = (c.r == 0 && (c.g == 41 && c.b == 140 || c.g == 40 && c.b == 136)) ||
                (c.r == 146 && c.g == 144 && c.b == 255) || (c.r == 148 && c.g == 148 && c.b == 255);
            t.SetPixel(flip ? width - xx - 1 : xx, y, transparent && bg ? Color.clear : c);
        }
        Save(t, name);
    }
    private static void Services()
    {
        var systems = Group("Sistemas de la aventura", root);
        // GameManager necesita ser una raíz para que su DontDestroyOnLoad conserve la partida.
        new GameObject("Administrador de partida").AddComponent<GameManager>();
        var mario = new GameObject("Mario"); mario.transform.SetParent(systems); mario.tag = "Player"; mario.layer = LayerMask.NameToLayer("PlayerLayer");
        mario.transform.position = new Vector3(3, 2.5f, 0);
        var rb = mario.AddComponent<Rigidbody2D>(); rb.freezeRotation = true; rb.gravityScale = 3; rb.interpolation = RigidbodyInterpolation2D.Interpolate; rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var box = mario.AddComponent<BoxCollider2D>(); box.size = new Vector2(.75f, .95f); box.offset = new Vector2(0, .025f);
        var sr = mario.AddComponent<SpriteRenderer>(); sr.sprite = Sprite("Assets/Resources/MarioFrames/Small_Idle.png"); sr.sortingLayerName = "Player"; sr.sharedMaterial = spriteMaterial;
        mario.AddComponent<PlayerAnimation>(); mario.AddComponent<PlayerMovement>(); level.player = mario.AddComponent<PlayerHealth>();
        SetVector(level.player, "smallColliderOffset", new Vector2(0, .025f)); SetVector(level.player, "bigColliderOffset", new Vector2(0, .525f));
        var camera = new GameObject("Cámara principal NES").AddComponent<Camera>(); camera.transform.SetParent(systems); camera.tag = "MainCamera";
        camera.transform.position = new Vector3(13, 7.5f, -10); camera.orthographic = true; camera.orthographicSize = 7.5f; camera.backgroundColor = Sky; camera.clearFlags = CameraClearFlags.SolidColor;
        camera.gameObject.AddComponent<AudioListener>();
        var follow = camera.gameObject.AddComponent<CameraFollow>(); follow.Target = mario.transform; SetBool(follow, "onlyScrollRight", false);
        level.view = camera.gameObject.AddComponent<AdventureCamera>(); level.view.view = camera; level.view.follow = follow; level.view.player = mario.transform;
        var light = new GameObject("Luz principal").AddComponent<Light>(); light.transform.SetParent(systems); light.type = LightType.Directional; light.intensity = .1f;
        level.sound = mario.AddComponent<AdventureAudio>(); level.sound.source = mario.AddComponent<AudioSource>();
        level.sound.source.playOnAwake = false; level.sound.source.spatialBlend = 0; level.sound.source.volume = .8f;
        level.sound.coin = Sound("coin"); level.sound.jumpSmall = Sound("jumpsmall"); level.sound.jumpBig = Sound("jump"); level.sound.stomp = Sound("stompswim"); level.sound.powerup = Sound("powerup");
        level.sound.fire = Sound("fire"); level.sound.fireball = Sound("fireball"); level.sound.impact = Sound("billfirework"); level.sound.mechanism = Sound("brick");
        level.sound.bowserFall = Sound("bowserfall"); level.sound.death = Sound("death"); level.sound.celebration = Sound("flagpole"); level.sound.pipe = Sound("pipepowerdown");
        level.checkpointSpawn = Group("Reaparición en el último refugio", systems); level.checkpointSpawn.position = new Vector3(310, 2.5f, 0);
        ItemPrefabs();
    }
    private static void ItemPrefabs()
    {
        foreach (string type in new[] { "Coin", "Mushroom", "Starman" })
        {
            var copy = Instance("Assets/Prefabs/Items/" + type + ".prefab", root, Vector2.zero);
            if (type == "Coin") Set(copy.GetComponent<Coin>(), "collectSound", null);
            else if (type == "Mushroom") { Set(copy.GetComponent<Mushroom>(), "powerUpSound", null); Set(copy.GetComponent<Mushroom>(), "spawnSound", null); }
            else { Set(copy.GetComponent<Starman>(), "starSound", Sound("powerup")); Set(copy.GetComponent<Starman>(), "spawnSound", null); }
            PrefabUtility.SaveAsPrefabAsset(copy, Prefabs + type + ".prefab"); Object.DestroyImmediate(copy);
        }
    }
    private static void CreateZones()
    {
        exterior = Zone("01 — EL CAMINO DEL REINO", AdventureEnvironment.Exterior, 0, 58, 0, 58);
        cave = Zone("02 — LAS CAVERNAS SECRETAS", AdventureEnvironment.Underground, 64, 120, 62, 346);
        water = Zone("03 — EL PASAJE ACUÁTICO", AdventureEnvironment.Underwater, 122, 160, 62, 346);
        mixed = Zone("04 — LOS CUATRO MUNDOS", AdventureEnvironment.Aqueduct, 164, 240, 62, 346);
        lava = Zone("05 — EL ÚLTIMO PUENTE", AdventureEnvironment.Lava, 240, 318, 62, 346);
        arenaZone = Zone("06 — BOWSER", AdventureEnvironment.Bowser, 318, 346, 62, 346);
        secret = Zone("SECRETO — LAS CUATRO CORONAS", AdventureEnvironment.Secret, 380, 398, 380, 398);
        cave.hint = "LAS MONEDAS MARCAN DOS CAMINOS\nOBSERVA LOS ELEVADORES";
        water.hint = "ESPACIO/Z: NADAR\nBUSCA LA TUBERÍA DE SALIDA";
        mixed.hint = "POR ARRIBA O POR EL AGUA\nTÚ ELIGES EL CAMINO";
        lava.hint = "OBSERVA EL FUEGO\nCADA ISLA ES UN DESCANSO";
        level.initialZone = exterior; level.checkpointZone = lava;
    }
    private static AdventureZone Zone(string name, AdventureEnvironment environment, float left, float right, float cameraLeft, float cameraRight)
    {
        var t = Group(name, root); var z = t.gameObject.AddComponent<AdventureZone>(); z.environment = environment; z.title = name;
        z.cameraLeft = cameraLeft; z.cameraRight = cameraRight;
        var box = t.gameObject.AddComponent<BoxCollider2D>(); box.isTrigger = true; box.size = new Vector2(right - left, 22); box.offset = new Vector2((left + right) / 2, 7);
        return z;
    }
    private static void Exterior()
    {
        var p = exterior.transform;
        Flat("Cielo azul clásico", -20, 64, -10, 24, Sky, p);
        Ground(0, 26, 2, Sprite("Assets/Sprites/Block_Ground.png"), p); Ground(29, 58, 2, Sprite("Assets/Sprites/Block_Ground.png"), p);
        foreach (float x in new[] { 7f, 24f, 42f, 54f }) Scenery("Hill", x, 3, p);
        foreach (float x in new[] { 5f, 21f, 38f, 53f })
        {
            var cloud = Scenery("Cloud", x, 10 + ((int)x % 3), p); var visual = cloud.AddComponent<AdventureVisual>(); visual.parallax = .06f; visual.drift = .5f;
        }
        foreach (float x in new[] { 11f, 31f, 47f }) Scenery("Bush", x, 2.5f, p);
        Block(10.5f, 5.5f, true, QuestionBlockContent.Coin, p);
        Block(13.5f, 5.5f, false, QuestionBlockContent.Coin, p); Block(14.5f, 5.5f, true, QuestionBlockContent.Mushroom, p); Block(15.5f, 5.5f, false, QuestionBlockContent.Coin, p);
        Ground(18, 22, 4, Sprite("Assets/Sprites/Block_Hard.png"), p, 2);
        Lift(20, 7, Vector2.zero, p); Lift(24, 8.5f, Vector2.zero, p);
        for (int i = 0; i < 6; i++) Coin(18 + i * 1.2f, i < 3 ? 8 : 9.5f, p);
        Block(24, 11, true, QuestionBlockContent.Starman, p);
        CoinArc(24, 31, 3.5f, p);
        Pipe(35, 2, "Tubería del prado", p);
        Enemy("Goomba", 20, 2.5f, p); Enemy("Koopa", 40, 2.75f, p); Enemy("Goomba", 48, 2.5f, p);
        Block(43.5f, 5.5f, false, QuestionBlockContent.Coin, p); Block(44.5f, 5.5f, true, QuestionBlockContent.Coin, p);
        for (int i = 0; i < 4; i++) Coin(49.5f + i, 3.2f, p);
        Pipe(56, 2, "Entrada a las cavernas", p);
        WorldLabel("ABAJO: ENTRAR", 54.5f, 6.2f, p, 5);
        Ground(58, 62, 11, Sprite("Assets/Sprites/Block_Hard.png"), p, 11);
    }
    private static void Caverns()
    {
        var p = cave.transform; Flat("Oscuridad de las cavernas", 62, 122, -10, 24, Color.black, p);
        Ground(64, 80, 2, TileSprite("Underground_0"), p); Ground(84, 100, 2, TileSprite("Underground_0"), p); Ground(104, 120, 2, TileSprite("Underground_0"), p);
        Ground(64, 120, 15, TileSprite("Underground_0"), p, 2);
        Ground(62, 64, 15, TileSprite("Underground_0"), p, 15); Ground(118, 122, 15, TileSprite("Underground_0"), p, 15);
        Pipe(68, 2, "Salida hacia las cavernas", p);
        Lift(82, 3, new Vector2(2, 0), p); Lift(102, 5, new Vector2(0, 4), p, 5, Mathf.PI);
        Lift(94, 5.5f, new Vector2(0, 4), p, 5);
        Ground(88, 90, 3.5f, TileSprite("Underground_0"), p, 2);
        Ground(91, 93, 5, TileSprite("Underground_0"), p, 1);
        Ground(97, 99, 7.5f, TileSprite("Underground_0"), p, 1);
        Lift(102, 8, Vector2.zero, p);
        Ground(105, 114, 8.5f, TileSprite("Underground_0"), p, 1);
        Block(73.5f, 5.5f, false, QuestionBlockContent.Coin, p, true); Block(74.5f, 5.5f, true, QuestionBlockContent.Mushroom, p, true);
        Block(75.5f, 5.5f, false, QuestionBlockContent.Coin, p, true);
        Enemy("Goomba", 77, 2.5f, p, true); Enemy("Koopa", 110, 2.75f, p);
        CoinArc(78, 86, 4.3f, p);
        foreach (float x in new[] { 91f, 94f, 97f, 101f, 106f, 108f, 110f, 112f }) Coin(x, x < 101 ? 8.5f : 9.8f, p);
        Block(111, 11, true, QuestionBlockContent.Starman, p, true);
        WorldLabel("EL TECHO GUARDA UN SECRETO", 106, 12, p, 4);
        Pipe(115.5f, 2, "Entrada al pasaje acuático", p);
        WorldLabel("ABAJO: AL AGUA", 114.5f, 6.3f, p, 5);
    }
    private static void WaterPassage()
    {
        var p = water.transform;
        Flat("Agua azul NES", 122, 162, -10, 24, new Color32(66, 64, 255, 255), p);
        Ground(122, 160, 1, TileSprite("Underwater_5"), p, 2);
        Ground(120, 122, 15, TileSprite("Underwater_5"), p, 15); Ground(158, 164, 15, TileSprite("Underwater_5"), p, 15);
        Ground(122, 160, 14, TileSprite("Underwater_5"), p, 2);
        Water(122, 158, 1, 11.5f, p);
        Pipe(126, 1, "Llegada al pasaje acuático", p);
        Ground(133, 136, 3, TileSprite("Underwater_5"), p, 2);
        Ground(141, 144, 6.5f, TileSprite("Underwater_5"), p, 2);
        Ground(148, 151, 3, TileSprite("Underwater_5"), p, 2);
        foreach (float x in new[] { 130f, 137f, 146f, 152f })
        {
            for (int y = 1; y < 3 + (int)x % 2; y++)
            {
                var coral = Image("Coral original — obstáculo sólido", TileSprite("Underwater_9"), x + .5f, y + .5f, p, "Ground"); coral.gameObject.AddComponent<CoralHazard>();
                coral.gameObject.layer = LayerMask.NameToLayer("Ground");
            }
        }
        SwimEnemy(136, 8, false, p); SwimEnemy(145, 8, true, p); SwimEnemy(151, 4.5f, false, p);
        for (int i = 0; i < 18; i++) Coin(128.5f + i * 1.4f, 5 + Mathf.Sin(i * .4f) * 2.2f, p);
        Pipe(155, 1, "Salida del pasaje acuático", p);
        WorldLabel("ABAJO: SALIR", 154.5f, 5.1f, p, 5);
        for (int i = 0; i < 7; i++)
        {
            var bubble = Image("Burbuja del pasaje", TileSprite("Underwater_6"), 128 + i * 4.3f, 3 + i % 5, p, "Background", -2);
            bubble.gameObject.AddComponent<AdventureVisual>().bob = .35f;
        }
    }
    private static void FourWorlds()
    {
        var p = mixed.transform; Flat("Cielo del acueducto", 162, 242, -10, 24, Sky, p);
        foreach (float x in new[] { 167f, 190f, 211f, 232f }) Scenery("Cloud", x, 11, p);
        foreach (float x in new[] { 170f, 202f, 233f }) Scenery("Hill", x, 3, p);
        Ground(164, 174, 2, Sprite("Assets/Sprites/Block_Ground.png"), p);
        Ground(174, 194, .5f, TileSprite("Underwater_5"), p, 2); Water(175, 193, .5f, 5, p);
        Ground(174, 175, 5, TileSprite("Underground_0"), p, 5); Ground(193, 194, 5, TileSprite("Underground_0"), p, 5);
        Ground(194, 214, 2, Sprite("Assets/Sprites/Block_Ground.png"), p);
        Pipe(168, 2, "Llegada al acueducto", p);
        Lift(175, 6, new Vector2(0, 1.5f), p); Lift(179, 7.5f, Vector2.zero, p);
        Ground(180, 184, 7.5f, TileSprite("Underground_0"), p, 1);
        Pipe(182, 7.5f, "Tubería del secreto especial", p);
        Lift(188, 7, new Vector2(2, 0), p); Lift(192, 6, Vector2.zero, p);
        SwimEnemy(183, 2.9f, false, p);
        Enemy("Goomba", 171, 2.5f, p); Enemy("Koopa", 206, 2.75f, p);
        foreach (float x in new[] { 176f, 178f, 186f, 188f, 190f }) Coin(x, 8.8f, p);
        for (int i = 0; i < 8; i++) Coin(177 + i * 1.8f, 2.2f + Mathf.Sin(i * .5f) * .5f, p);
        Block(200.5f, 5.5f, false, QuestionBlockContent.Coin, p); Block(201.5f, 5.5f, true, QuestionBlockContent.Mushroom, p); Block(202.5f, 5.5f, false, QuestionBlockContent.Coin, p);
        Ground(214, 232, .5f, TileSprite("Underwater_5"), p, 2); Water(215, 231, .5f, 3.5f, p);
        Ground(214, 215, 3.5f, TileSprite("Underground_0"), p, 4); Ground(231, 232, 3.5f, TileSprite("Underground_0"), p, 4);
        Lift(218, 4.8f, new Vector2(2, 0), p); Ground(222, 225, 5, TileSprite("Underground_0"), p, 1);
        Lift(228, 5.8f, new Vector2(0, 2), p); Ground(232, 240, 2, Sprite("Assets/Sprites/Block_Hard.png"), p);
        Pipe(235, 2, "Regreso del secreto especial", p);
        Firebar(224, 9, 2.2f, p);
        CoinArc(212, 220, 6.5f, p); CoinArc(225, 233, 7.2f, p);
        WorldLabel("¿Y SI MIRAS ARRIBA?", 181, 11.2f, p, 5);
        WorldLabel("LOS CUATRO MUNDOS", 208, 11, p, 5);
    }
    private static void LavaApproach()
    {
        var p = lava.transform; Flat("Oscuridad del último puente", 240, 318, -10, 24, Color.black, p);
        Ground(240, 250, 2, NewSprite("CastleStone"), p); Ground(254, 258, 2, NewSprite("CastleStone"), p);
        Ground(266, 272, 3, NewSprite("CastleStone"), p, 3); Ground(280, 286, 2, NewSprite("CastleStone"), p);
        Ground(289, 294, 2.5f, NewSprite("CastleStone"), p, 1); Ground(302, 318, 2, NewSprite("CastleStone"), p);
        Ground(240, 318, 15, NewSprite("CastleStone"), p, 2);
        for (int i = 0; i < 4; i++) Ground(240 + i, 241 + i, 8 - i, NewSprite("CastleStone"), p, 1);
        Lava(250, 266, new Vector2(248, 2.5f), p); Lava(272, 280, new Vector2(270, 3.5f), p); Lava(286, 302, new Vector2(284, 2.5f), p);
        Lift(262, 3, new Vector2(2, 0), p); Lift(276, 3.4f, new Vector2(0, 1.5f), p); Lift(298, 3, new Vector2(2, 0), p);
        Firebar(269, 7, 2.4f, p); Firebar(291, 7.5f, 2.3f, p); Jet(282, 2, .9f, p);
        foreach (float x in new[] { 248f, 254f, 258f, 262f, 268f, 274f, 278f, 284f, 289f, 294f, 298f, 303f }) Coin(x, 4.5f + (x > 266 && x < 280 ? 1 : 0), p);
        Enemy("Goomba", 245, 2.5f, p);
        var checkpoint = Group("Punto de control — último refugio", p); checkpoint.position = new Vector3(308, 3, 0);
        var flag = Image("Bandera original de control", Sprite("Assets/Sprites/FlagPole.png"), 308, 2, checkpoint, "Decoration"); flag.transform.localScale = Vector3.one * .375f;
        var box = checkpoint.gameObject.AddComponent<BoxCollider2D>(); box.size = new Vector2(2, 4); box.isTrigger = true;
        checkpoint.gameObject.AddComponent<AdventureCheckpoint>().flag = flag;
        Block(312, 5.5f, true, QuestionBlockContent.Mushroom, p);
        WorldLabel("ÚLTIMO REFUGIO", 310, 8, p, 5);
        WorldLabel("EL PUENTE DEL ECO", 316, 10.5f, p, 5);
    }
    private static void SecretRoom()
    {
        var p = secret.transform; Flat("Fondo de la sala secreta", 365, 413, -10, 24, Color.black, p);
        Ground(380, 398, 2, TileSprite("Underground_0"), p); Ground(380, 398, 15, TileSprite("Underground_0"), p, 2);
        Ground(379, 380, 15, TileSprite("Underground_0"), p, 15); Ground(398, 399, 15, TileSprite("Underground_0"), p, 15);
        Pipe(382, 2, "Entrada al secreto", p); Pipe(395, 2, "Salida del secreto", p);
        Ground(384, 386, 4, Sprite("Assets/Sprites/Block_Hard.png"), p);
        Ground(387, 389, 5.5f, TileSprite("Underground_0"), p, 1);
        Ground(390, 392, 4, TileSprite("Underwater_5"), p, 2);
        Ground(393, 395, 5.5f, NewSprite("CastleStone"), p, 1);
        foreach (float x in new[] { 385f, 388f, 391f, 394f }) for (int i = 0; i < 3; i++) Coin(x, 7 + i, p);
        Block(389.5f, 10.5f, true, QuestionBlockContent.Starman, p, true);
        WorldLabel("LAS CUATRO CORONAS", 389, 12, p, 5);
    }
    private static void ConnectPipes()
    {
        Link("Entrada a las cavernas", "Salida hacia las cavernas", cave, false);
        Link("Entrada al pasaje acuático", "Llegada al pasaje acuático", water, true);
        Link("Salida del pasaje acuático", "Llegada al acueducto", mixed, false);
        Link("Tubería del secreto especial", "Entrada al secreto", secret, false);
        Link("Salida del secreto", "Regreso del secreto especial", mixed, false);
    }
    private static void Link(string from, string to, AdventureZone zone, bool underwater)
    {
        var source = root.GetComponentsInChildren<Transform>().First(t => t.name == from);
        var target = root.GetComponentsInChildren<Transform>().First(t => t.name == to);
        var trigger = Group("Boca de la tubería — " + from, source); trigger.position = source.position + Vector3.up;
        var box = trigger.gameObject.AddComponent<BoxCollider2D>(); box.isTrigger = true; box.size = new Vector2(1.6f, .45f); box.offset = Vector2.up * .1f;
        var pipe = trigger.gameObject.AddComponent<AdventurePipe>(); pipe.destinationZone = zone; pipe.underwaterDestination = underwater;
        var spawn = Group("Emerger — " + to, target); spawn.position = target.position + Vector3.up * 1.5f; pipe.destination = spawn;
    }
    private static GameObject Scenery(string type, float x, float y, Transform parent)
    {
        var go = Instance("Assets/Prefabs/Level/" + type + ".prefab", parent, new Vector2(x, y)); go.name = type == "Cloud" ? "Nube NES" : type == "Hill" ? "Colina original" : "Arbusto original"; return go;
    }
    private static GameObject Pipe(float x, float floor, string name, Transform parent)
    {
        var go = Instance("Assets/Prefabs/Level/Pipe.prefab", parent, new Vector2(x, floor + 1)); go.name = name;
        SetBool(go.GetComponent<MarioBros.Level.Pipe>(), "isEnterable", false); return go;
    }
    private static void Block(float x, float y, bool question, QuestionBlockContent content, Transform parent, bool blue = false)
    {
        var go = Instance("Assets/Prefabs/Blocks/" + (question ? "QuestionBlock" : "BrickBlock") + ".prefab", parent, new Vector2(x, y));
        go.name = question ? "Bloque de interrogación — " + content : "Ladrillo rompible original";
        if (question)
        {
            var block = go.GetComponent<QuestionBlock>(); SetInt(block, "content", (int)content);
            Set(block, "coinPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + "Coin.prefab"));
            Set(block, "mushroomPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + "Mushroom.prefab"));
            Set(block, "starmanPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + "Starman.prefab"));
            Set(block, "bumpSound", Sound("bump")); Set(block, "powerupSpawnSound", Sound("item"));
        }
        else
        {
            var block = go.GetComponent<BrickBlock>(); SetBool(block, "useUndergroundDebris", blue);
            Set(block, "coinPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + "Coin.prefab")); Set(block, "breakSound", Sound("brick")); Set(block, "bumpSound", Sound("bump"));
            if (blue) go.GetComponent<SpriteRenderer>().sprite = TileSprite("Underground_0");
        }
    }
    private static void Coin(float x, float y, Transform parent)
    {
        var go = Instance(Prefabs + "Coin.prefab", parent, new Vector2(x, y)); go.name = "Moneda animada — ruta y recompensa";
        go.GetComponent<SpriteRenderer>().sprite = Sprite("Assets/Resources/ClassicEffects/Coin/0.png");
    }
    private static void CoinArc(float left, float right, float height, Transform parent)
    {
        for (int i = 0; i < 6; i++) { float t = i / 5f; Coin(Mathf.Lerp(left, right, t), height + Mathf.Sin(t * Mathf.PI) * 1.8f, parent); }
    }
    private static void Enemy(string type, float x, float y, Transform parent, bool blue = false)
    {
        var go = Instance("Assets/Prefabs/Enemies/" + type + ".prefab", parent, new Vector2(x, y)); go.name = type + " — guardia de la ruta";
        if (type == "Goomba") SetBool(go.GetComponent<Goomba>(), "undergroundPalette", blue);
        else { SetVector(go.GetComponent<KoopaController>(), "walkingColliderOffset", Vector2.zero); SetVector(go.GetComponent<KoopaController>(), "shellColliderOffset", new Vector2(0, -.25f)); }
    }
    private static void SwimEnemy(float x, float y, bool blooper, Transform parent)
    {
        var image = Image(blooper ? "Blooper original" : "Cheep Cheep original", blooper ? TileSprite("Blooper") : Sprite("Assets/Resources/MarioFrames/CheepCheep1.png"), x, y, parent, "Enemies");
        image.gameObject.layer = LayerMask.NameToLayer("EnemyLayer"); image.gameObject.AddComponent<BoxCollider2D>(); image.gameObject.AddComponent<Rigidbody2D>();
        image.gameObject.AddComponent<SwimmingEnemy>().blooper = blooper;
    }
    private static void Lift(float x, float y, Vector2 travel, Transform parent, float period = 5, float phase = 0)
    {
        var image = Image(travel == Vector2.zero ? "Plataforma elevada original" : "Elevador del recorrido", Sprite("Assets/Sprites/LiftPlatform.png"), x, y, parent, "Ground");
        image.gameObject.layer = LayerMask.NameToLayer("Ground");
        var box = image.gameObject.AddComponent<BoxCollider2D>(); box.size = new Vector2(3, .4f); box.usedByEffector = true;
        image.gameObject.AddComponent<PlatformEffector2D>().useOneWay = true;
        if (travel == Vector2.zero) return;
        var rb = image.gameObject.AddComponent<Rigidbody2D>(); rb.bodyType = RigidbodyType2D.Kinematic; rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        var platform = image.gameObject.AddComponent<AdventurePlatform>(); platform.travel = travel; platform.period = period; platform.phase = phase;
    }
    private static void Water(float left, float right, float bottom, float surface, Transform parent)
    {
        var volume = Group("Volumen de nado — movimiento existente", parent); volume.position = new Vector3((left + right) / 2, (bottom + surface) / 2, 0);
        var box = volume.gameObject.AddComponent<BoxCollider2D>(); box.isTrigger = true; box.size = new Vector2(right - left, surface - bottom); volume.gameObject.AddComponent<AdventureWater>();
        Flat("Agua de color plano NES", left, right, bottom, surface, new Color32(66, 64, 255, 255), parent, -20);
        for (float x = left; x < right; x++) Image("Superficie de agua original", TileSprite("Underwater_4"), x + .5f, surface - .5f, parent, "Background", -19);
    }
    private static void Lava(float left, float right, Vector2 safe, Transform parent)
    {
        var lake = Group("Lava NES — daño existente", parent); lake.position = new Vector3((left + right) / 2, -1.2f, 0);
        var box = lake.gameObject.AddComponent<BoxCollider2D>(); box.isTrigger = true; box.size = new Vector2(right - left, 3.8f);
        var hazard = lake.gameObject.AddComponent<AdventureHazard>(); hazard.lava = true; hazard.safeShore = safe;
        for (float x = left; x < right; x++)
        {
            for (int row = 0; row < 3; row++) Image("Lava original", NewSprite("LavaBody"), x + .5f, -.5f - row, lake, "Ground", 3);
            var wave = Image("Ola de lava animada", NewSprite("LavaSurface0"), x + .5f, .5f, lake, "Ground", 4);
            var anim = wave.gameObject.AddComponent<AdventureVisual>(); anim.frames = Enumerable.Range(0, 4).Select(i => NewSprite("LavaSurface" + i)).ToArray(); anim.frameRate = 5;
        }
    }
    private static void Firebar(float x, float y, float length, Transform parent)
    {
        var bar = Group("Barra giratoria de fuego", parent); bar.position = new Vector3(x, y, 0);
        bar.gameObject.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        bar.gameObject.AddComponent<AdventureFireTrap>().rotating = true;
        Image("Anclaje de piedra original", NewSprite("CastleStone"), x, y, bar, "Ground");
        for (float d = .5f; d <= length; d += .5f)
        {
            var fire = Image("Fuego de la barra", NewSprite("FireA"), x + d, y, bar, "Enemies", 5); fire.transform.localScale = Vector3.one * .5f;
            var anim = fire.gameObject.AddComponent<AdventureVisual>(); anim.frames = new[] { NewSprite("FireA"), NewSprite("FireB") }; anim.frameRate = 10;
            var box = fire.gameObject.AddComponent<CircleCollider2D>(); box.radius = .4f; box.isTrigger = true; fire.gameObject.AddComponent<AdventureHazard>();
        }
    }
    private static void Jet(float x, float floor, float phase, Transform parent)
    {
        var root = Group("Respiradero con aviso", parent); root.position = new Vector3(x, floor, 0);
        var fire = Group("Chorro de fuego NES", root); fire.position = new Vector3(x, floor + 1, 0);
        for (int i = 0; i < 2; i++)
        {
            var image = Image("Llama original", NewSprite("FireA"), x, floor + .75f + i, fire, "Enemies", 5); image.transform.rotation = Quaternion.Euler(0, 0, 90);
            var anim = image.gameObject.AddComponent<AdventureVisual>(); anim.frames = new[] { NewSprite("FireA"), NewSprite("FireB") }; anim.frameRate = 10;
        }
        var box = fire.gameObject.AddComponent<BoxCollider2D>(); box.isTrigger = true; box.size = new Vector2(.45f, 2.3f); fire.gameObject.AddComponent<AdventureHazard>();
        var warning = Image("Aviso de fuego", Sprite("Assets/Sprites/Block_Question.png"), x, floor + .2f, root, "Items", 5); warning.transform.localScale = Vector3.one * .5f;
        var trap = root.gameObject.AddComponent<AdventureFireTrap>(); trap.flame = fire.gameObject; trap.warning = warning; trap.phase = phase; fire.gameObject.SetActive(false);
    }
    private static void WorldLabel(string text, float x, float y, Transform parent, float size)
    {
        var go = new GameObject("Inscripción — " + text, typeof(RectTransform), typeof(Canvas)); go.transform.SetParent(parent, false);
        var canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace; canvas.sortingLayerName = "Decoration"; canvas.sortingOrder = 20;
        go.transform.position = new Vector3(x, y, 0); go.transform.localScale = Vector3.one / 16;
        Text(go.transform, text, 128, 120, size);
    }

    private static void BossArena()
    {
        var p = arenaZone.transform; Flat("Fondo negro de la arena NES", 318, 366, -12, 26, Color.black, p);
        Ground(318, 324, 2.5f, NewSprite("CastleStone"), p, 3); Ground(338, 346, 2.5f, NewSprite("CastleStone"), p, 3);
        Ground(318, 346, 15, NewSprite("CastleStone"), p, 2);
        // Columnas pequeñas, hechas con la misma piedra gris del mapa original.
        foreach (int x in new[] { 318, 344 }) for (int y = 3; y < 11; y++) Image("Pilar NES de la arena", NewSprite("CastleStone"), x + .5f, y + .5f, p, "Background", -5);
        Lava(324, 338, new Vector2(321, 3), p);
        var arena = Group("Arena del puente del eco", p); level.arena = arena.gameObject.AddComponent<AdventureArena>();
        var sensor = arena.gameObject.AddComponent<BoxCollider2D>(); sensor.isTrigger = true; sensor.size = new Vector2(1, 14); sensor.offset = new Vector2(320, 7);
        var segments = new List<GameObject>();
        for (int x = 324; x < 338; x++)
        {
            var segment = Image("Segmento del puente — " + (x - 323), NewSprite("CastleStone"), x + .5f, 2, arena, "Ground", 2);
            segment.gameObject.layer = LayerMask.NameToLayer("Ground"); segment.gameObject.AddComponent<BoxCollider2D>().size = Vector2.one; segments.Add(segment.gameObject);
        }
        level.arena.bridge = segments.ToArray();
        var gate = Solid("Reja al comenzar la batalla", 318.2f, 7, .5f, 12, arena);
        for (int y = 2; y < 13; y++) Image("Piedra de la reja", NewSprite("CastleStone"), 318.2f, y + .5f, gate.transform, "Ground", 5);
        level.arena.gate = gate.gameObject; gate.gameObject.SetActive(false);
        level.arena.switches = new[] { Switch(321.5f, arena), Switch(342, arena) };
        Lift(327, 5.5f, Vector2.zero, arena); Lift(335, 5.5f, Vector2.zero, arena);
        var boss = Group("Bowser — jefe final NES", arena); boss.position = new Vector3(332, 2.55f, 0); boss.gameObject.layer = LayerMask.NameToLayer("EnemyLayer");
        var rb = boss.gameObject.AddComponent<Rigidbody2D>(); rb.bodyType = RigidbodyType2D.Kinematic; rb.gravityScale = 0;
        var hitbox = boss.gameObject.AddComponent<BoxCollider2D>(); hitbox.isTrigger = true; hitbox.size = new Vector2(1.65f, 1.8f); hitbox.offset = new Vector2(0, .9f);
        var image = Image("Bowser — sprites clásicos de 32 píxeles", NewSprite("Bowser_WalkA"), 332, 2.55f, boss, "Enemies", 5);
        var contact = boss.gameObject.AddComponent<AdventureHazard>(); contact.armed = false;
        var attacks = boss.gameObject.AddComponent<FinalBowserAttacks>();
        attacks.firePrefab = Projectile("Aliento", false); attacks.wavePrefab = Projectile("Onda", true); attacks.pillarPrefab = Projectile("LlamaTemporal", true);
        var marker = Image("Aviso de caída de Bowser", Sprite("Assets/Sprites/Block_Empty.png"), 332, 2.7f, arena, "Items", 6);
        marker.transform.localScale = new Vector3(3, .125f, 1); marker.color = new Color32(248, 56, 0, 255); marker.enabled = false; attacks.landingMarker = marker;
        attacks.pillarMarkers = new[] { 328f, 336f }.Select(x =>
        {
            var warning = Image("Aviso de llama temporal", Sprite("Assets/Sprites/Block_Empty.png"), x, 2.7f, arena, "Items", 6);
            warning.transform.localScale = new Vector3(1, .125f, 1); warning.color = new Color32(248, 56, 0, 255); warning.enabled = false; return warning;
        }).ToArray();
        var controller = boss.gameObject.AddComponent<FinalBowser>(); controller.image = image; controller.attacks = attacks; controller.contact = contact;
        controller.walkFrames = new[] { NewSprite("Bowser_WalkA"), NewSprite("Bowser_WalkB") }; controller.fireFrames = new[] { NewSprite("Bowser_FireA"), NewSprite("Bowser_FireB") };
        level.arena.boss = controller;
        WorldLabel("EL PUENTE DEL ECO", 331, 11, arena, 6);
    }
    private static AdventureSwitch Switch(float x, Transform parent)
    {
        var image = Image("Interruptor de Bowser — golpe desde abajo", Sprite("Assets/Sprites/Block_Empty.png"), x, 5.5f, parent, "Blocks", 4);
        image.gameObject.layer = LayerMask.NameToLayer("Ground"); image.gameObject.AddComponent<BoxCollider2D>().size = Vector2.one;
        var block = image.gameObject.AddComponent<AdventureSwitch>(); block.arena = level.arena; block.image = image;
        block.readySprite = Sprite("Assets/Sprites/Block_Question.png"); block.restingSprite = Sprite("Assets/Sprites/Block_Empty.png");
        var signal = Group("Aviso — ¡GOLPEA!", parent); WorldLabel("¡GOLPEA!", x, 7.2f, signal, 5); block.signal = signal.gameObject; signal.gameObject.SetActive(false);
        return block;
    }
    private static GameObject Projectile(string name, bool vertical)
    {
        var go = new GameObject(name);
        var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = NewSprite("FireA"); sr.sharedMaterial = spriteMaterial; sr.sortingLayerName = "Enemies"; sr.sortingOrder = 10;
        var visual = go.AddComponent<AdventureVisual>(); visual.frames = new[] { NewSprite("FireA"), NewSprite("FireB") }; visual.frameRate = 12;
        var box = go.AddComponent<BoxCollider2D>(); box.isTrigger = true; box.size = new Vector2(1.2f, .35f);
        var rb = go.AddComponent<Rigidbody2D>(); rb.bodyType = RigidbodyType2D.Kinematic; rb.gravityScale = 0;
        if (vertical) go.transform.rotation = Quaternion.Euler(0, 0, 90);
        var hazard = go.AddComponent<AdventureHazard>(); hazard.disappearOnContact = !vertical;
        go.AddComponent<FinalBowserProjectile>();
        var result = PrefabUtility.SaveAsPrefabAsset(go, Prefabs + name + ".prefab"); Object.DestroyImmediate(go); return result;
    }
    private static void Interface()
    {
        var hudObject = new GameObject("HUD de Marco — MUNDO 1-4", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler)); hudObject.transform.SetParent(root, false);
        hudObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay; hudObject.GetComponent<Canvas>().pixelPerfect = true; hudObject.GetComponent<Canvas>().sortingOrder = 100;
        var nesHud = hudObject.AddComponent<NesHud>(); nesHud.spanish = true; nesHud.worldOverride = "1-4";
        var overlay = new GameObject("Presentación NES de la aventura", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)); overlay.transform.SetParent(root, false);
        var canvas = overlay.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.pixelPerfect = true; canvas.sortingOrder = 150;
        var scaler = overlay.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(256, 240); scaler.matchWidthOrHeight = 1;
        var frame = UIGroup("Marco NES 256 x 240", overlay.transform);
        level.presentation = overlay.AddComponent<AdventurePresentation>();
        level.presentation.zone = Text(frame, "01 — EL CAMINO DEL REINO", 128, 233, 4);
        var announcement = UIGroup("Título y anuncios", frame);
        Panel("Fondo de anuncio", announcement, 128, 78, 244, 42, Color.black);
        level.presentation.announcement = Text(announcement, "MUNDO 1-4\nLA AVENTURA FINAL", 128, 78, 8);
        level.presentation.announcementGroup = announcement.gameObject.AddComponent<CanvasGroup>(); level.presentation.announcementGroup.alpha = 0; level.presentation.announcementGroup.blocksRaycasts = false;
        var hint = UIGroup("Indicaciones", frame);
        Panel("Fondo de la indicación", hint, 128, 213, 248, 22, new Color(0, 0, 0, .88f));
        level.presentation.hint = Text(hint, "", 128, 213, 5); level.presentation.hintGroup = hint.gameObject.AddComponent<CanvasGroup>(); level.presentation.hintGroup.alpha = 0; level.presentation.hintGroup.blocksRaycasts = false;
        var boss = UIGroup("Información del jefe", frame);
        Panel("Fondo del jefe", boss, 128, 52, 148, 26, Color.black);
        level.presentation.bossTitle = Text(boss, "BOWSER — FASE 1/3", 128, 45, 6);
        level.presentation.healthCoins = new Image[6];
        for (int i = 0; i < 6; i++)
        {
            var coin = Panel("Resistencia de Bowser " + (i + 1), boss, 91 + i * 15, 59, 8, 8, Color.white);
            coin.sprite = Sprite("Assets/Resources/ClassicUI/Coin.png"); level.presentation.healthCoins[i] = coin;
        }
        level.presentation.bossPanel = boss.gameObject; boss.gameObject.SetActive(false);
        var victory = UIGroup("Final NES — victoria", overlay.transform);
        victory.anchorMin = Vector2.zero; victory.anchorMax = Vector2.one; victory.sizeDelta = Vector2.zero;
        var veil = Panel("Fondo del final", victory, 128, 120, 256, 240, Color.black);
        veil.rectTransform.anchorMin = Vector2.zero; veil.rectTransform.anchorMax = Vector2.one; veil.rectTransform.sizeDelta = Vector2.zero;
        // El marco utiliza líneas de píxel de interfaz, sin modificar la pantalla de título original.
        Panel("Marco dorado", victory, 128, 120, 248, 224, Gold); Panel("Interior negro", victory, 128, 120, 244, 220, Color.black);
        var banner = Panel("Mario celebra", victory, 128, 67, 24, 24, Color.white); banner.sprite = Sprite("Assets/Resources/MarioFrames/Small_Jump.png"); banner.preserveAspect = true;
        foreach (int x in new[] { 70, 186 }) { var star = Panel("Estrella original", victory, x, 67, 16, 16, Color.white); star.sprite = Sprite("Assets/Sprites/Item_Starman.png"); }
        Text(victory, "¡FELICIDADES!", 128, 35, 12);
        Text(victory, "¡HAS DERROTADO A BOWSER!", 128, 96, 8);
        Text(victory, "¡EL REINO CHAMPIÑÓN\nESTÁ A SALVO!", 128, 127, 8);
        level.presentation.victoryScore = Text(victory, "000000 PUNTOS", 128, 156, 6);
        level.presentation.menuButton = UIButton(victory, "VOLVER AL MENÚ", 128, 184);
        UnityEventTools.AddPersistentListener(level.presentation.menuButton.onClick, level.presentation.ReturnToMenu);
        var replay = UIButton(victory, "JUGAR DE NUEVO", 128, 208); UnityEventTools.AddPersistentListener(replay.onClick, level.presentation.Replay);
        level.presentation.victoryPanel = victory.gameObject; victory.gameObject.SetActive(false);
        var fade = Panel("Transición por tubería", overlay.transform, 128, 120, 256, 240, Color.black);
        fade.rectTransform.anchorMin = Vector2.zero; fade.rectTransform.anchorMax = Vector2.one; fade.rectTransform.anchoredPosition = Vector2.zero; fade.rectTransform.sizeDelta = Vector2.zero;
        level.presentation.fadeGroup = fade.gameObject.AddComponent<CanvasGroup>(); level.presentation.fadeGroup.alpha = 0; level.presentation.fadeGroup.blocksRaycasts = false;
        var input = new GameObject("Entrada de menú NES", typeof(EventSystem), typeof(InputSystemUIInputModule)); input.transform.SetParent(root, false);
        level.presentation.celebration = Celebration();
    }
    private static ParticleSystem Celebration()
    {
        var go = new GameObject("Celebración — monedas NES"); go.transform.SetParent(root, false); go.transform.position = new Vector3(331, 11, 0);
        var ps = go.AddComponent<ParticleSystem>(); ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main; main.playOnAwake = false; main.loop = false; main.duration = 5; main.startLifetime = 4; main.startSpeed = 0; main.startSize = .35f; main.maxParticles = 120; main.gravityModifier = .1f;
        var emission = ps.emission; emission.rateOverTime = 18;
        var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Box; shape.scale = new Vector3(22, 1, .01f);
        var texture = ps.textureSheetAnimation; texture.enabled = true; texture.mode = ParticleSystemAnimationMode.Sprites;
        var coin = Sprite("Assets/Resources/ClassicEffects/Coin/0.png"); texture.AddSprite(coin);
        var celebrationMaterial = new Material(spriteMaterial); celebrationMaterial.mainTexture = coin.texture;
        AssetDatabase.CreateAsset(celebrationMaterial, Art + "NesCelebration.mat");
        var render = ps.GetComponent<ParticleSystemRenderer>(); render.sharedMaterial = celebrationMaterial; render.sortingLayerName = "Items"; render.sortingOrder = 40; return ps;
    }
    private static RectTransform UIGroup(string name, Transform parent)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent, false); rect.sizeDelta = new Vector2(256, 240); return rect;
    }
    private static NesText Text(Transform parent, string text, float x, float y, float size)
    {
        var label = NesText.Create(parent, text, x, y, size); label.rectTransform.sizeDelta = new Vector2(256, 40); return label;
    }
    private static Image Panel(string name, Transform parent, float x, float y, float width, float height, Color color)
    {
        var image = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<Image>(); image.transform.SetParent(parent, false);
        image.rectTransform.anchoredPosition = new Vector2(x - 128, 120 - y); image.rectTransform.sizeDelta = new Vector2(width, height); image.color = color; image.raycastTarget = false; return image;
    }
    private static Button UIButton(Transform parent, string text, float x, float y)
    {
        var panel = Panel("Botón — " + text, parent, x, y, 170, 18, new Color32(0, 56, 128, 255)); panel.raycastTarget = true;
        var button = panel.gameObject.AddComponent<Button>(); button.targetGraphic = panel;
        var colors = button.colors; colors.selectedColor = new Color32(248, 184, 0, 255); colors.highlightedColor = colors.selectedColor; colors.pressedColor = new Color32(184, 104, 0, 255); button.colors = colors;
        Text(panel.transform, text, 128, 120, 7); return button;
    }

    private static Transform Group(string name, Transform parent) { var go = new GameObject(name); go.transform.SetParent(parent, false); return go.transform; }
    private static Sprite Sprite(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);
    private static Sprite NewSprite(string name) => Sprite(Art + name + ".png");
    private static Sprite TileSprite(string name) => Sprite("Assets/Sprites/Replica/" + name + ".png");
    private static AudioClip Sound(string name) => AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/" + name + ".wav");
    private static GameObject Instance(string path, Transform parent, Vector2 p)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path)); go.transform.SetParent(parent); go.transform.position = p;
        foreach (var image in go.GetComponentsInChildren<SpriteRenderer>()) image.sharedMaterial = spriteMaterial;
        return go;
    }
    private static SpriteRenderer Image(string name, Sprite sprite, float x, float y, Transform parent, string layer = "Decoration", int order = 0)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.position = new Vector3(x, y, 0);
        var image = go.AddComponent<SpriteRenderer>(); image.sprite = sprite; image.sortingLayerName = layer; image.sortingOrder = order; image.sharedMaterial = spriteMaterial; return image;
    }
    private static SpriteRenderer Flat(string name, float left, float right, float bottom, float top, Color color, Transform parent, int order = -100)
    {
        var image = Image(name, NewSprite("Pixel"), (left + right) / 2, (bottom + top) / 2, parent, "Background", order);
        image.transform.localScale = new Vector3((right - left) * 16, (top - bottom) * 16, 1); image.color = color; return image;
    }
    private static BoxCollider2D Solid(string name, float x, float y, float width, float height, Transform parent)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.position = new Vector3(x, y, 0); go.layer = LayerMask.NameToLayer("Ground");
        var box = go.AddComponent<BoxCollider2D>(); box.size = new Vector2(width, height); return box;
    }
    private static void Ground(float left, float right, float top, Sprite sprite, Transform parent, int depth = 2)
    {
        var floor = Group("Suelo y roca sólida", parent);
        for (int y = 0; y < depth; y++) for (float x = left; x < right; x++) Image("Bloque de suelo NES", sprite, x + .5f, top - .5f - y, floor, "Ground");
        Solid("Colisión del suelo", (left + right) / 2, top - depth / 2f, right - left, depth, floor);
    }
    private static void Set(Object target, string field, Object value) { var s = new SerializedObject(target); s.FindProperty(field).objectReferenceValue = value; s.ApplyModifiedPropertiesWithoutUndo(); }
    private static void SetBool(Object target, string field, bool value) { var s = new SerializedObject(target); s.FindProperty(field).boolValue = value; s.ApplyModifiedPropertiesWithoutUndo(); }
    private static void SetInt(Object target, string field, int value) { var s = new SerializedObject(target); s.FindProperty(field).intValue = value; s.ApplyModifiedPropertiesWithoutUndo(); }
    private static void SetVector(Object target, string field, Vector2 value) { var s = new SerializedObject(target); s.FindProperty(field).vector2Value = value; s.ApplyModifiedPropertiesWithoutUndo(); }
}
