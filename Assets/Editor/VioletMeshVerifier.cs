using UnityEditor;
using UnityEngine;

/// <summary>
/// Inspects the violet_tbp.fbx mesh vertex count and UV count for UV-match verification.
/// </summary>
public static class VioletMeshVerifier
{
    [MenuItem("VioletAndMarlowe/Verify Violet Mesh")]
    public static void VerifyMesh()
    {
        string fbxPath = "Assets/Art/Characters/Violet/violet_tbp.fbx";
        var go = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (go == null)
        {
            Debug.LogError($"[VioletMeshVerifier] Could not load {fbxPath}");
            return;
        }

        var smr = go.GetComponentInChildren<SkinnedMeshRenderer>(true);
        if (smr != null && smr.sharedMesh != null)
        {
            var mesh = smr.sharedMesh;
            Debug.Log($"[VioletMeshVerifier] FBX Mesh: {mesh.name}");
            Debug.Log($"[VioletMeshVerifier]   vertexCount={mesh.vertexCount}");
            Debug.Log($"[VioletMeshVerifier]   uvCount={mesh.uv?.Length ?? 0}");
            Debug.Log($"[VioletMeshVerifier]   uv2Count={mesh.uv2?.Length ?? 0}");
            Debug.Log($"[VioletMeshVerifier]   subMeshCount={mesh.subMeshCount}");
            Debug.Log($"[VioletMeshVerifier]   blendShapeCount={mesh.blendShapeCount}");
            for (int i = 0; i < Mathf.Min(mesh.subMeshCount, 3); i++)
            {
                var sub = mesh.GetSubMesh(i);
                Debug.Log($"[VioletMeshVerifier]   subMesh[{i}]: indexCount={sub.indexCount}, topology={sub.topology}");
            }
        }
        else
        {
            var mf = go.GetComponentInChildren<MeshFilter>(true);
            if (mf != null && mf.sharedMesh != null)
            {
                var mesh = mf.sharedMesh;
                Debug.Log($"[VioletMeshVerifier] FBX Mesh (MeshFilter): {mesh.name}");
                Debug.Log($"[VioletMeshVerifier]   vertexCount={mesh.vertexCount}");
                Debug.Log($"[VioletMeshVerifier]   uvCount={mesh.uv?.Length ?? 0}");
            }
            else
            {
                Debug.LogError("[VioletMeshVerifier] No SkinnedMeshRenderer or MeshFilter found");
            }
        }

        // Also check if the texture was imported
        string texPath = "Assets/Art/Characters/Violet/violet_albedo.png";
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        if (tex != null)
        {
            Debug.Log($"[VioletMeshVerifier] Texture: {tex.name}, {tex.width}x{tex.height}, format={tex.format}");
        }
        else
        {
            Debug.LogWarning($"[VioletMeshVerifier] Texture not found at {texPath} (may need Unity import)");
        }
    }
}
