using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using MarioBros.Sound;

/// <summary>
/// Herramientas de Abril para la música:
///   Tools > Abril > Configurar Musica   -> ajusta los .wav y crea Resources/MusicManager.prefab
///   Tools > Abril > Verificar Musica    -> revisa que todo esté bien conectado
/// </summary>
public static class MusicSetupEditor
{
    private const string MusicFolder = "Assets/Audio/Music";
    private const string PrefabPath = "Assets/Resources/MusicManager.prefab";

    // escena -> (archivo, loop, detener al terminar nivel)
    private static readonly (string scene, string file, bool loop, bool stopOnEnd)[] Map =
    {
        ("TitleScreen", "Music_Title",       true,  false),
        ("SampleScene", "Music_Overworld",   true,  true),
        ("Underground", "Music_Underground", true,  true),
        ("Underwater",  "Music_Underwater",  true,  true),
        ("GameOver",    "Music_GameOver",    false, false),
    };

    [MenuItem("Tools/Abril/Configurar Musica")]
    public static void Configure()
    {
        // 1) Importación: música en streaming, comprimida (no ocupa RAM)
        foreach (var m in Map)
        {
            string path = $"{MusicFolder}/{m.file}.wav";
            var imp = AssetImporter.GetAtPath(path) as AudioImporter;
            if (imp == null) { Debug.LogError($"[Musica] No encuentro {path}"); continue; }
            var s = imp.defaultSampleSettings;
            s.loadType = AudioClipLoadType.Streaming;
            s.compressionFormat = AudioCompressionFormat.Vorbis;
            s.quality = 0.7f;
            imp.defaultSampleSettings = s;
            imp.loadInBackground = true;
            imp.SaveAndReimport();
        }

        // 2) Prefab en Resources (el script lo instancia solo)
        Directory.CreateDirectory("Assets/Resources");
        var go = new GameObject("MusicManager");
        go.AddComponent<AudioSource>().playOnAwake = false;
        go.AddComponent<MusicManager>();

        var so = new SerializedObject(go.GetComponent<MusicManager>());
        var list = so.FindProperty("tracks");
        list.arraySize = Map.Length;
        for (int i = 0; i < Map.Length; i++)
        {
            var el = list.GetArrayElementAtIndex(i);
            el.FindPropertyRelative("sceneName").stringValue = Map[i].scene;
            el.FindPropertyRelative("clip").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<AudioClip>($"{MusicFolder}/{Map[i].file}.wav");
            el.FindPropertyRelative("loop").boolValue = Map[i].loop;
            el.FindPropertyRelative("volume").floatValue = 1f;
            el.FindPropertyRelative("stopWhenLevelEnds").boolValue = Map[i].stopOnEnd;
        }
        so.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
        Object.DestroyImmediate(go);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Musica] Listo: prefab creado en " + PrefabPath + ". Ahora ejecuta Tools > Abril > Verificar Musica.");
        Verify();
    }

    [MenuItem("Tools/Abril/Verificar Musica")]
    public static void Verify()
    {
        var problems = new List<string>();

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null || prefab.GetComponent<MusicManager>() == null)
            problems.Add("Falta " + PrefabPath + " (ejecuta Configurar Musica).");

        var inBuild = new HashSet<string>();
        foreach (var s in EditorBuildSettings.scenes)
            inBuild.Add(Path.GetFileNameWithoutExtension(s.path));

        foreach (var m in Map)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{MusicFolder}/{m.file}.wav");
            if (clip == null) problems.Add($"Falta el audio {m.file}.wav");
            else if (clip.length < 1f) problems.Add($"{m.file}.wav dura menos de 1 s");
            if (!inBuild.Contains(m.scene)) problems.Add($"La escena {m.scene} no esta en Build Settings.");
            if (!File.Exists($"Assets/Scenes/{m.scene}.unity")) problems.Add($"No existe la escena {m.scene}.");
        }

        if (prefab != null)
        {
            var so = new SerializedObject(prefab.GetComponent<MusicManager>());
            var list = so.FindProperty("tracks");
            for (int i = 0; i < list.arraySize; i++)
            {
                var el = list.GetArrayElementAtIndex(i);
                if (el.FindPropertyRelative("clip").objectReferenceValue == null)
                    problems.Add("Pista sin clip: " + el.FindPropertyRelative("sceneName").stringValue);
            }
            float vol = so.FindProperty("musicVolume").floatValue;
            if (vol > 0.4f) problems.Add($"musicVolume={vol:0.00} es alto; puede tapar los SFX (recomendado 0.1-0.25).");
        }

        if (problems.Count == 0) Debug.Log("[Musica] OK: audios, prefab y escenas estan bien conectados.");
        else foreach (var p in problems) Debug.LogWarning("[Musica] " + p);
    }
}
