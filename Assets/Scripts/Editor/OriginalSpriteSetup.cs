using System.IO;
using UnityEditor;
using UnityEngine;

public static class OriginalSpriteSetup
{
    public static void Extract()
    {
        var map = new Texture2D(2, 2);
        map.LoadImage(File.ReadAllBytes("Assets/SpritesJuego/NES - Super Mario Bros. - Stages - World 1-1.png"));
        Save(map, "Castle", 202 * 16, 128, 80, 80);
        Save(map, "FlagPole", 198 * 16, 32, 16, 176);
        Save(map, "Block_Hard", 80 * 16, 80, 16, 16);
        Object.DestroyImmediate(map);
        AssetDatabase.Refresh();
        foreach (string name in new[] { "Castle", "FlagPole" })
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath("Assets/Sprites/" + name + ".png");
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = new Vector2(.5f, 0);
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }
    }

    private static void Save(Texture2D source, string name, int x, int top, int width, int height)
    {
        var pixels = source.GetPixels(x, source.height - top - height, width, height);
        Color32 sky = new Color32(92,148,252,255);
        for (int i = 0; i < pixels.Length; i++)
        {
            Color32 c = pixels[i];
            if (c.r == sky.r && c.g == sky.g && c.b == sky.b) pixels[i] = Color.clear;
        }
        var texture = new Texture2D(width, height);
        texture.SetPixels(pixels);texture.Apply();
        File.WriteAllBytes("Assets/Sprites/" + name + ".png", texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
    }
}
