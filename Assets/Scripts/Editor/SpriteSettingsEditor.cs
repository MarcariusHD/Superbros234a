using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor utility to configure sprite import settings for pixel art.
/// Run from menu: Tools > Configure Pixel Art Sprites
/// </summary>
public class SpriteSettingsEditor : EditorWindow
{
    [MenuItem("Tools/Configure Pixel Art Sprites")]
    static void ConfigureAllSprites()
    {
        // Find all textures in SpritesJuego folder
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/SpritesJuego" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Multiple;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.spritePixelsPerUnit = 16;
                importer.mipmapEnabled = false;
                importer.maxTextureSize = 8192;

                // Save and reimport
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();

                Debug.Log($"Configured: {path}");
            }
        }

        Debug.Log("All sprites configured for pixel art!");
        EditorUtility.DisplayDialog("Sprite Configuration",
            "All sprites in SpritesJuego have been configured for pixel art.\n\n" +
            "Settings applied:\n" +
            "- Sprite Mode: Multiple\n" +
            "- Filter Mode: Point\n" +
            "- Compression: None\n" +
            "- PPU: 16\n" +
            "- Max Size: 8192",
            "OK");
    }
}
