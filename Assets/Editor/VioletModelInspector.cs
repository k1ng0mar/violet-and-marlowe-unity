using UnityEditor;
using UnityEngine;

/// <summary>
/// Inspects the imported violet_tbp.fbx model for:
/// - Model height (from bounds)
/// - Materials and textures
/// - Avatar info
/// </summary>
public static class VioletModelInspector
{
    [MenuItem("VioletAndMarlowe/Inspect Violet Model")]
    public static void InspectVioletModel()
    {
        string fbxPath = "Assets/Art/Characters/Violet/violet_tbp.fbx";
        var go = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (go == null)
        {
            Debug.LogError($"[VioletModelInspector] Could not load {fbxPath}");
            return;
        }

        // Check for Avatar
        var subAssets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
        foreach (var sub in subAssets)
        {
            if (sub is Avatar av)
            {
                Debug.Log($"[VioletModelInspector] Avatar: name={av.name}, isHuman={av.isHuman}, isValid={av.isValid}");
            }
        }

        // Check renderers and materials
        var renderers = go.GetComponentsInChildren<Renderer>(true);
        Debug.Log($"[VioletModelInspector] Found {renderers.Length} renderers");

        foreach (var r in renderers)
        {
            Debug.Log($"[VioletModelInspector] Renderer: {r.name}, type={r.GetType().Name}, materials={r.sharedMaterials.Length}");
            foreach (var m in r.sharedMaterials)
            {
                if (m == null)
                {
                    Debug.Log($"[VioletModelInspector]   Material: NULL");
                    continue;
                }
                Debug.Log($"[VioletModelInspector]   Material: {m.name}, shader={m.shader?.name}, mainTexture={m.mainTexture?.name}, mainTextureResolution={GetTextureResolution(m.mainTexture)}");
            }
        }

        // Check model height via SkinnedMeshRenderer bounds
        var skinned = go.GetComponentInChildren<SkinnedMeshRenderer>(true);
        if (skinned != null)
        {
            var bounds = skinned.localBounds;
            Debug.Log($"[VioletModelInspector] SkinnedMeshRenderer bounds: center={bounds.center}, extents={bounds.extents}, height={bounds.extents.y * 2:F3}m");
        }
        else
        {
            var meshFilter = go.GetComponentInChildren<MeshFilter>(true);
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                var bounds = meshFilter.sharedMesh.bounds;
                Debug.Log($"[VioletModelInspector] MeshFilter bounds: center={bounds.center}, extents={bounds.extents}, height={bounds.extents.y * 2:F3}m");
            }
        }

        // Also check root GameObject children
        foreach (Transform child in go.transform)
        {
            Debug.Log($"[VioletModelInspector] Child: {child.name}, localPosition={child.localPosition}");
        }
    }

    private static string GetTextureResolution(Texture tex)
    {
        if (tex == null) return "null";
        return $"{tex.width}x{tex.height}";
    }
}