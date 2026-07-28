using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

/// <summary>
/// Reusable utility: extracts an embedded texture from a GLB file and creates
/// a persistent Unity material asset, then assigns it to the SkinnedMeshRenderer
/// of a model in the scene.
///
/// Usage (headless):
///   xvfb-run -a unity -batchmode -nographics -projectPath . \
///     -executeMethod GLBTextureExtractor.ExtractAndAssign \
///     -glbPath "/path/to/model.glb" \
///     -fbxPath "Assets/.../model.fbx" \
///     -imageIndex 1 \
///     -materialPath "Assets/.../model_material.mat"
///
/// Or call ExtractTextureFromGLB() directly from other editor scripts.
/// </summary>
public static class GLBTextureExtractor
{
    /// <summary>
    /// Extracts a PNG/JPEG image from a GLB file's binary chunk by image index.
    /// Returns the raw bytes, or null if not found.
    /// </summary>
    public static byte[] ExtractTextureFromGLB(string glbPath, int imageIndex)
    {
        return GLBTextureExtractorImpl.Extract(glbPath, imageIndex);
    }

    /// <summary>
    /// Full pipeline entry point for headless batchmode.
    /// Reads -glbPath, -fbxPath, -imageIndex, -materialPath from command line.
    /// </summary>
    public static void ExtractAndAssign()
    {
        var args = System.Environment.GetCommandLineArgs();
        string glbPath = null, fbxPath = null, materialPath = null;
        int imageIndex = 1;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-glbPath": glbPath = args[++i]; break;
                case "-fbxPath": fbxPath = args[++i]; break;
                case "-imageIndex": imageIndex = int.Parse(args[++i]); break;
                case "-materialPath": materialPath = args[++i]; break;
            }
        }

        if (string.IsNullOrEmpty(glbPath) || string.IsNullOrEmpty(fbxPath))
        {
            Debug.LogError("[GLBTextureExtractor] Usage: -glbPath <path> -fbxPath <path> [-imageIndex N] [-materialPath <path>]");
            return;
        }

        // Derive material path if not provided
        if (string.IsNullOrEmpty(materialPath))
        {
            var dir = Path.GetDirectoryName(fbxPath);
            var name = Path.GetFileNameWithoutExtension(fbxPath);
            materialPath = Path.Combine(dir, name + "_material.mat");
        }

        // Step 1: Extract texture
        byte[] textureBytes = ExtractTextureFromGLB(glbPath, imageIndex);
        if (textureBytes == null)
        {
            Debug.LogError($"[GLBTextureExtractor] No image at index {imageIndex} in {glbPath}");
            return;
        }

        // Step 2: Save PNG to Assets
        var texDir = Path.GetDirectoryName(materialPath);
        var texName = Path.GetFileNameWithoutExtension(materialPath) + "_albedo.png";
        var texPath = Path.Combine(texDir, texName);
        File.WriteAllBytes(texPath, textureBytes);
        AssetDatabase.ImportAsset(texPath);
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);

        // Step 3: Create persistent material
        var mat = new Material(Shader.Find("Standard"));
        mat.mainTexture = texture;
        AssetDatabase.CreateAsset(mat, materialPath);
        AssetDatabase.SaveAssets();

        Debug.Log($"[GLBTextureExtractor] Created {materialPath} with mainTexture={texture.name} ({texture.width}x{texture.height})");

        // Step 4: Assign to model in active scene
        var go = GameObject.Find(fbxPath);
        if (go == null)
        {
            // Try finding by model name
            var modelName = Path.GetFileNameWithoutExtension(fbxPath);
            go = GameObject.Find(modelName);
        }

        if (go != null)
        {
            var smr = go.GetComponentInChildren<SkinnedMeshRenderer>();
            if (smr != null)
            {
                smr.material = mat;
                EditorUtility.SetDirty(smr);
                Debug.Log($"[GLBTextureExtractor] Assigned material to {smr.gameObject.name}, mainTexture={smr.material.mainTexture?.name}");
                EditorSceneManager.SaveOpenScenes();
            }
            else
            {
                Debug.LogWarning($"[GLBTextureExtractor] No SkinnedMeshRenderer found under {go.name}");
            }
        }
        else
        {
            Debug.LogWarning($"[GLBTextureExtractor] No GameObject found matching {fbxPath} in scene");
        }
    }
}
