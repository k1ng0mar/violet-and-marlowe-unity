using UnityEditor;
using UnityEngine;

public static class VioletImportAndSceneBuilder
{
    [MenuItem("VioletAndMarlowe/Build Scene With Violet Model")]
    public static void BuildSceneWithVioletModel()
    {
        // First, ensure the FBX is imported with correct rig settings
        string fbxPath = "Assets/Art/Characters/Violet/violet_tbp.fbx";
        var importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
        if (importer != null)
        {
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.SaveAndReimport();
            Debug.Log("[VioletImportAndSceneBuilder] Rig setup applied and saved.");
        }

        // Now build the scene normally
        WalkTheBlockSceneBuilder.BuildSceneWithVioletCharacter();
    }
}
