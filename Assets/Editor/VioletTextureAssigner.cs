using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Creates a persistent material asset with the violet albedo texture
/// and assigns it to the model in the scene. The FBX's default material
/// gets overwritten by an external material assignment on the renderer.
/// </summary>
public static class VioletTextureAssigner
{
    [MenuItem("VioletAndMarlowe/Assign Violet Texture")]
    public static void AssignTexture()
    {
        string fbxPath = "Assets/Art/Characters/Violet/violet_tbp.fbx";
        string texPath = "Assets/Art/Characters/Violet/violet_albedo.png";
        string matPath = "Assets/Art/Characters/Violet/violet_material.mat";

        // Load the texture
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        if (texture == null)
        {
            Debug.LogError($"[VioletTextureAssigner] Texture not found at {texPath}");
            return;
        }
        Debug.Log($"[VioletTextureAssigner] Loaded texture: {texture.name}, {texture.width}x{texture.height}");

        // Create or load a persistent material asset
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null)
        {
            // Create new material asset with Standard shader
            mat = new Material(Shader.Find("Standard"));
            mat.name = "violet_material";
            AssetDatabase.CreateAsset(mat, matPath);
            Debug.Log($"[VioletTextureAssigner] Created new material at {matPath}");
        }

        // Assign texture to the persistent material
        mat.mainTexture = texture;
        EditorUtility.SetDirty(mat);
        AssetDatabase.SaveAssets();
        Debug.Log($"[VioletTextureAssigner] Material mainTexture={mat.mainTexture?.name}, res={mat.mainTexture?.width}x{mat.mainTexture?.height}");

        // Now rebuild the scene — the scene builder will assign this material to the renderer
        Debug.Log("[VioletTextureAssigner] Now rebuilding scene with textured model...");
        WalkTheBlockSceneBuilder.BuildSceneWithVioletCharacter();

        // After scene build, find the player's SkinnedMeshRenderer and assign our material
        var player = GameObject.Find("Player");
        if (player != null)
        {
            var smr = player.GetComponentInChildren<SkinnedMeshRenderer>();
            if (smr != null)
            {
                smr.sharedMaterial = mat;
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/WalkTheBlock.unity");
                Debug.Log($"[VioletTextureAssigner] Assigned violet_material to SkinnedMeshRenderer. mainTexture={smr.sharedMaterial.mainTexture?.name}");
            }
            else
            {
                Debug.LogWarning("[VioletTextureAssigner] No SkinnedMeshRenderer found on player");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[VioletTextureAssigner] DONE");
    }
}
