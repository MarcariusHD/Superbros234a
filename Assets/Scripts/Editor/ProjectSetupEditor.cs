using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor utility to set up tags and sorting layers for the Mario project.
/// Run from menu: Tools > Setup Mario Tags & Layers
/// </summary>
public class ProjectSetupEditor : EditorWindow
{
    [MenuItem("Tools/Setup Mario Tags and Layers")]
    static void SetupTagsAndLayers()
    {
        // === TAGS ===
        string[] tags = new string[]
        {
            "Player",
            "Enemy",
            "Coin",
            "PowerUp",
            "QuestionBlock",
            "BrickBlock",
            "Pipe",
            "FlagPole",
            "KillZone",
            "Ground"
        };

        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        foreach (string tag in tags)
        {
            bool found = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
                Debug.Log($"Tag added: {tag}");
            }
        }

        // === SORTING LAYERS ===
        string[] sortingLayers = new string[]
        {
            "Background",
            "Decoration",
            "Ground",
            "Blocks",
            "Items",
            "Enemies",
            "Player",
            "UI"
        };

        SerializedProperty sortingLayersProp = tagManager.FindProperty("m_SortingLayers");

        foreach (string layer in sortingLayers)
        {
            bool found = false;
            for (int i = 0; i < sortingLayersProp.arraySize; i++)
            {
                SerializedProperty entry = sortingLayersProp.GetArrayElementAtIndex(i);
                if (entry.FindPropertyRelative("name").stringValue == layer)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                sortingLayersProp.InsertArrayElementAtIndex(sortingLayersProp.arraySize);
                SerializedProperty newEntry = sortingLayersProp.GetArrayElementAtIndex(sortingLayersProp.arraySize - 1);
                newEntry.FindPropertyRelative("name").stringValue = layer;
                newEntry.FindPropertyRelative("uniqueID").intValue = layer.GetHashCode();
                Debug.Log($"Sorting Layer added: {layer}");
            }
        }

        // === PHYSICS LAYERS ===
        // Layer 6 = Ground, Layer 7 = Player, Layer 8 = Enemy, Layer 9 = Item
        string[] layerNames = new string[] { "Ground", "PlayerLayer", "EnemyLayer", "ItemLayer" };
        int[] layerIndices = new int[] { 6, 7, 8, 9 };

        SerializedProperty layersProp = tagManager.FindProperty("layers");
        for (int i = 0; i < layerNames.Length; i++)
        {
            SerializedProperty sp = layersProp.GetArrayElementAtIndex(layerIndices[i]);
            if (string.IsNullOrEmpty(sp.stringValue))
            {
                sp.stringValue = layerNames[i];
                Debug.Log($"Layer {layerIndices[i]} set to: {layerNames[i]}");
            }
        }

        tagManager.ApplyModifiedProperties();

        EditorUtility.DisplayDialog("Project Setup",
            "Tags, Sorting Layers, and Physics Layers configured!\n\n" +
            "Tags: Player, Enemy, Coin, PowerUp, QuestionBlock, BrickBlock, Pipe, FlagPole, KillZone, Ground\n\n" +
            "Sorting Layers: Background, Decoration, Ground, Blocks, Items, Enemies, Player, UI\n\n" +
            "Physics Layers: Ground(6), PlayerLayer(7), EnemyLayer(8), ItemLayer(9)",
            "OK");
    }
}
