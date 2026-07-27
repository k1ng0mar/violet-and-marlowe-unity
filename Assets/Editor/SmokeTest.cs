using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

public class SmokeTest
{
    [MenuItem("VioletAndMarlowe/Run Smoke Test")]
    public static void RunSmokeTest()
    {
        Debug.Log("=== SMOKE TEST START ===");

        bool allPassed = true;

        // 1. Open the WalkTheBlock scene
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/WalkTheBlock.unity");
        if (!scene.IsValid())
        {
            Debug.LogError("SMOKE TEST FAILED: Could not open WalkTheBlock.unity");
            return;
        }
        Debug.Log("[PASS] Scene loaded: WalkTheBlock.unity");

        // 2. Verify Player exists with CharacterController + PlayerController
        var player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogError("SMOKE TEST FAILED: Player GameObject not found");
            return;
        }
        Debug.Log("[PASS] Player GameObject exists");

        var cc = player.GetComponent<CharacterController>();
        if (cc == null)
        {
            Debug.LogError("SMOKE TEST FAILED: CharacterController not found on Player");
            allPassed = false;
        }
        else
        {
            Debug.Log($"[PASS] CharacterController exists (height={cc.height}, radius={cc.radius})");
        }

        var pc = player.GetComponent<PlayerController>();
        if (pc == null)
        {
            Debug.LogError("SMOKE TEST FAILED: PlayerController not found on Player");
            allPassed = false;
        }
        else
        {
            Debug.Log("[PASS] PlayerController exists");
            Debug.Log($"  walkSpeed={pc.walkSpeed}, runSpeed={pc.runSpeed}, jumpHeight={pc.jumpHeight}");
            Debug.Log($"  dashSpeed={pc.dashSpeed}, dashCooldown={pc.dashCooldown}");
            Debug.Log($"  crouchHeight={pc.crouchHeight}, standingHeight={pc.standingHeight}");
        }

        // 3. Verify CameraRoot exists
        var cameraRoot = player.transform.Find("CameraRoot");
        if (cameraRoot == null)
        {
            Debug.LogError("SMOKE TEST FAILED: CameraRoot not found under Player");
            allPassed = false;
        }
        else
        {
            Debug.Log($"[PASS] CameraRoot exists at local pos: {cameraRoot.localPosition}");
        }

        // 4. Verify MainCamera exists
        var cam = cameraRoot?.Find("MainCamera");
        if (cam == null)
        {
            Debug.LogError("SMOKE TEST FAILED: MainCamera not found under CameraRoot");
            allPassed = false;
        }
        else
        {
            var camera = cam.GetComponent<Camera>();
            if (camera == null)
            {
                Debug.LogError("SMOKE TEST FAILED: Camera component not found on MainCamera");
                allPassed = false;
            }
            else
            {
                Debug.Log($"[PASS] MainCamera exists with Camera component (tag={cam.tag})");
            }
        }

        // 5. Verify CityBlock exists with CityBlockGenerator
        var cityBlock = GameObject.Find("CityBlock");
        if (cityBlock == null)
        {
            Debug.LogError("SMOKE TEST FAILED: CityBlock GameObject not found");
            allPassed = false;
        }
        else
        {
            Debug.Log("[PASS] CityBlock GameObject exists");
        }

        var gen = cityBlock?.GetComponent<CityBlockGenerator>();
        if (gen == null)
        {
            Debug.LogError("SMOKE TEST FAILED: CityBlockGenerator not found on CityBlock");
            allPassed = false;
        }
        else
        {
            Debug.Log("[PASS] CityBlockGenerator exists");
            Debug.Log($"  warmMaterial assigned: {gen.warmMaterial != null}");
            Debug.Log($"  greyMaterial assigned: {gen.greyMaterial != null}");
        }

        // 6. Verify Input System is available
        try
        {
            int deviceCount = InputSystem.devices.Count;
            Debug.Log($"[PASS] InputSystem initialized (devices: {deviceCount})");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SMOKE TEST FAILED: InputSystem not initialized: {e.Message}");
            allPassed = false;
        }

        // 7. Count colliders in scene
        var colliders = Object.FindObjectsByType<Collider>(FindObjectsSortMode.None);
        Debug.Log($"[PASS] Colliders in scene: {colliders.Length}");

        // 7b. Assert collider count > 2 (buildings + crates present)
        if (colliders.Length > 2)
        {
            Debug.Log($"[PASS] Collider count > 2: {colliders.Length} (buildings + crates present)");
        }
        else
        {
            Debug.LogError($"SMOKE TEST FAILED: Collider count <= 2: {colliders.Length} (expected > 2 for buildings + crates)");
            allPassed = false;
        }

        // 8. Count all GameObjects
        var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        Debug.Log($"[PASS] Total GameObjects in scene: {allObjects.Length}");

        if (allPassed)
        {
            Debug.Log("=== SMOKE TEST PASSED ===");
        }
        else
        {
            Debug.LogError("=== SMOKE TEST FAILED ===");
        }

        // Write result to file for batchmode verification
        System.IO.File.WriteAllText("/tmp/smoke_test_result.txt", allPassed ? "PASSED" : "FAILED");
    }
}
