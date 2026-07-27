using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

/// <summary>
/// Gate test: character model skinned height, instance scale, and hips position.
/// Instance scale is the FINAL approach — pre-skin scaling (vertex bake, globalScale)
/// is broken for this asset due to bindpose scale-dependence.
/// </summary>
public class ScaleBakePlayModeTests
{
    const float EXPECTED_SCALE = 142.07f;
    const float MIN_HEIGHT = 1.75f;
    const float MAX_HEIGHT = 1.85f;

    private GameObject player;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        SceneManager.LoadScene("WalkTheBlock", LoadSceneMode.Single);
        yield return null;
        yield return new WaitForSeconds(0.5f);

        player = GameObject.Find("Player");
        Assert.IsNotNull(player, "Player not found in WalkTheBlock scene");
    }

    [UnityTest]
    public IEnumerator ScaleImport_ModelHeightIsCorrect()
    {
        Transform visual = player.transform.Find("PlayerVisual");
        if (visual == null)
            visual = FindDeep(player.transform, "PlayerVisual");
        Assert.IsNotNull(visual, "PlayerVisual child not found");

        var smr = visual.GetComponentInChildren<SkinnedMeshRenderer>();
        Assert.IsNotNull(smr, "SkinnedMeshRenderer not found");

        // Let skinning update
        yield return new WaitForSeconds(0.1f);

        Bounds worldBounds = smr.bounds;
        float worldHeight = worldBounds.size.y;
        float feetY = worldBounds.center.y - worldBounds.extents.y;

        Debug.Log($"[ScaleTest] Skinned: size.y={worldHeight:F4} center.y={worldBounds.center.y:F4} feetY={feetY:F4}");

        // --- HEIGHT ---
        Assert.GreaterOrEqual(worldHeight, MIN_HEIGHT,
            $"Skinned height {worldHeight:F3}m < {MIN_HEIGHT}");
        Assert.LessOrEqual(worldHeight, MAX_HEIGHT,
            $"Skinned height {worldHeight:F3}m > {MAX_HEIGHT}");

        // --- INSTANCE SCALE ---
        Assert.AreEqual(EXPECTED_SCALE, visual.localScale.y, 0.5f,
            $"localScale.y={visual.localScale.y:F2} expected ~{EXPECTED_SCALE}");

        // --- HIPS BONE ---
        var hips = FindDeep(visual, "mixamorig:Hips");
        if (hips != null)
        {
            float hipsRelY = hips.position.y - player.transform.position.y;
            Debug.Log($"[ScaleTest] Hips relY={hipsRelY:F4} (player Y={player.transform.position.y:F4})");
            Assert.GreaterOrEqual(hipsRelY, 0.90f,
                $"Hips Y={hipsRelY:F3} < 0.90");
            Assert.LessOrEqual(hipsRelY, 1.10f,
                $"Hips Y={hipsRelY:F3} > 1.10");
        }
        else
        {
            Debug.LogWarning("[ScaleTest] mixamorig:Hips not found — skipping hips check");
        }

        Debug.Log($"[ScaleTest] PASS: height={worldHeight:F2}m, localScale.y={visual.localScale.y:F2}");
    }

    [UnityTearDown]
    public IEnumerator TearDown() { yield return null; }

    static Transform FindDeep(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var found = FindDeep(child, name);
            if (found != null) return found;
        }
        return null;
    }
}
