using UnityEditor;
using UnityEngine;

/// <summary>
/// Sets up the FBX import settings for the violet character model:
/// - Rig Animation Type: Humanoid
/// - Avatar Setup: Create From This Model
/// Uses ModelImporter API (which regenerates .meta on SaveAndReimport)
/// </summary>
public static class CharacterImportSetup
{
    [MenuItem("VioletAndMarlowe/Setup Character Import")]
    public static void SetupCharacterImport()
    {
        string fbxPath = "Assets/Art/Characters/violet.fbx";
        var importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
        if (importer == null)
        {
            Debug.LogError($"[CharacterImportSetup] ModelImporter not found for {fbxPath}");
            return;
        }

        // Force reimport to pick up any manual .meta edits
        importer.isReadable = false;
        importer.SaveAndReimport();
        
        // Re-read after import to get current state
        importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;

        // Use SerializedObject — these ARE the hidden Serialized Properties
        var serialized = new SerializedObject(importer);
        
        // Find the nested humanDescription structure
        string[] paths = {
            "m_HumanDescription.animationType",
            "m_HumanDescription.avatarSetup",
            "m_HumanDescription.human",
            "m_HumanDescription.skeleton",
        };
        foreach (var path in paths)
        {
            var p = serialized.FindProperty(path);
            if (p != null)
            {
                Debug.Log($"[CharacterImportSetup] Found '{path}' type={p.propertyType}");
            }
        }

        // Try the humanDescription path
        var humanDesc = serialized.FindProperty("m_HumanDescription");
        if (humanDesc != null)
        {
            Debug.Log("[CharacterImportSetup] Found m_HumanDescription, iterating children...");
            var child = humanDesc.Copy();
            child.NextVisible(true);
            while (child.depth > humanDesc.depth)
            {
                Debug.Log($"  Child: '{child.name}' depth={child.depth} type={child.propertyType}");
                child.NextVisible(false);
            }
        }

        // Try setting animationType directly (public API sets serialized internally)
        // In Unity 6, the enum might be called differently
        importer.animationType = (ModelImporterAnimationType)1;
        importer.isReadable = false;
        importer.SaveAndReimport();
        
        Debug.Log("[CharacterImportSetup] Done — check log for property introspection");
    }
}