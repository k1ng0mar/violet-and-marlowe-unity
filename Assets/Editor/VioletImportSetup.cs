using UnityEditor;
using UnityEngine;

/// <summary>
/// Sets up the FBX import settings for violet_tbp.fbx:
/// - Rig Animation Type: Humanoid
/// - Avatar Setup: Create From This Model
/// Sets properties on the importer object FIRST, then calls SaveAndReimport.
/// </summary>
public static class VioletImportSetup
{
    [MenuItem("VioletAndMarlowe/Setup Violet Import")]
    public static void SetupVioletImport()
    {
        string fbxPath = "Assets/Art/Characters/Violet/violet_tbp.fbx";
        var importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
        if (importer == null)
        {
            Debug.LogError($"[VioletImportSetup] ModelImporter not found for {fbxPath}");
            return;
        }

        // Set rig properties on the importer object BEFORE SaveAndReimport
        // Unity 6 enum: ModelImporterAnimationType.Human (not Humanoid)
        importer.animationType = ModelImporterAnimationType.Human;
        // Unity 6 enum: ModelImporterAvatarSetup.CreateFromThisModel
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;

        // Set global scale — model is ~0.013m, needs ~1.8m → scale ~140
        // But let's use 1.0 and scale in the scene builder instead
        importer.globalScale = 1.0f;
        importer.isReadable = false;

        // NOW save and reimport — this processes the settings and generates the Avatar
        importer.SaveAndReimport();
        Debug.Log("[VioletImportSetup] SaveAndReimport called — Avatar should be generated");

        // Verify the Avatar was created
        var subAssets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
        foreach (var sub in subAssets)
        {
            if (sub is Avatar av)
            {
                Debug.Log($"[VioletImportSetup] SUCCESS: Avatar found — name={av.name}, isHuman={av.isHuman}");
                return;
            }
        }
        Debug.LogWarning("[VioletImportSetup] WARNING: No Avatar sub-asset found after reimport");
    }
}