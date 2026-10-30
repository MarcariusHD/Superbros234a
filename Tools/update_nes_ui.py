from pathlib import Path
p=Path('Assets/Scripts/Editor/ReplicaLevelSetup.cs');s=p.read_text()
a=s.index('    private static void BuildHud(');b=s.index('    private static void BuildGameOver()',a)
s=s[:a]+'''    private static void BuildHud(string world)
    {
        var old = GameObject.Find("HUD Canvas");
        if (old != null) UnityEngine.Object.DestroyImmediate(old);
        var canvas = Canvas("HUD Canvas");
        canvas.gameObject.AddComponent<NesHud>();
    }

'''+s[b:]
a=s.index('    private static void BuildGameOver()');b=s.index('    private static Sprite Sprite(',a)
s=s[:a]+'''    private static void BuildGameOver()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var camera = new GameObject("Main Camera").AddComponent<Camera>();camera.tag = "MainCamera";
        camera.clearFlags = CameraClearFlags.SolidColor;camera.backgroundColor = Color.black;
        camera.gameObject.AddComponent<AudioListener>();
        new GameObject("Game Over NES").AddComponent<GameOverScreen>();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),"Assets/Scenes/GameOver.unity");
    }

'''+s[b:];p.write_text(s)
