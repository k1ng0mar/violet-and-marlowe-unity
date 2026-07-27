using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class WalkTheBlockSceneBuilder
{
    [MenuItem("VioletAndMarlowe/Build Walk-The-Block Scene")]
    public static void BuildScene()
    {
        string scenePath = "Assets/Scenes/WalkTheBlock.unity";

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        GameObject playerObj = new GameObject("Player");
        playerObj.transform.position = new Vector3(0, 1.1f, 0);
        var controller = playerObj.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.4f;
        controller.center = new Vector3(0, 1f, 0);
        controller.skinWidth = 0.01f;
        controller.stepOffset = 0.0f;
        controller.minMoveDistance = 0.0f;

        var playerController = playerObj.AddComponent<PlayerController>();

        GameObject cameraRootObj = new GameObject("CameraRoot");
        cameraRootObj.transform.SetParent(playerObj.transform);
        cameraRootObj.transform.localPosition = new Vector3(0, 1.6f, -2.5f);

        var cameraRoot = cameraRootObj.transform;
        playerController.cameraRoot = cameraRoot;

        GameObject cameraObj = new GameObject("MainCamera");
        cameraObj.transform.SetParent(cameraRoot);
        cameraObj.transform.localPosition = Vector3.zero;
        cameraObj.transform.localRotation = Quaternion.identity;

        var cam = cameraObj.AddComponent<Camera>();
        cam.tag = "MainCamera";

        GameObject cityBlockObj = new GameObject("CityBlock");
        var cityGenerator = cityBlockObj.AddComponent<CityBlockGenerator>();

        Material warmMat = new Material(Shader.Find("Unlit/Color"));
        warmMat.color = new Color(0.949f, 0.463f, 0.180f); // #F2762E
        warmMat.name = "WarmMaterial";

        Material greyMat = new Material(Shader.Find("Unlit/Color"));
        greyMat.color = new Color(0.431f, 0.431f, 0.451f); // #6E6E73
        greyMat.name = "GreyMaterial";

        cityGenerator.warmMaterial = warmMat;
        cityGenerator.greyMaterial = greyMat;

        GameObject cratePrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cratePrefab.name = "CratePrefab";
        cratePrefab.transform.localScale = new Vector3(1, 1, 1);
        cratePrefab.GetComponent<Renderer>().material = greyMat;
        cityGenerator.cratePrefab = cratePrefab;

        // Generate the city block immediately so primitives exist in the saved scene
        cityGenerator.GenerateCityBlock();

        // Build the bank (heist target) — placed at (30,0,0), clear of spawn plaza
        BankBuilder.BuildBank(cityBlockObj, warmMat, greyMat);

        // Set up mobile touch controls
        MobileControlsSetup.SetupMobileControls();

        // Set up HUD (minimap, partner card, health/bar, weapon, reticle, banner, debug overlay, dev config)
        HUDSetup.SetupHUD();

        if (!System.IO.Directory.Exists("Assets/Scenes"))
            System.IO.Directory.CreateDirectory("Assets/Scenes");

        EditorSceneManager.SaveScene(scene, scenePath, false);
        EditorSceneManager.CloseScene(scene, true);

        Debug.Log($"Scene saved to {scenePath}");
    }

    /// <summary>
    /// Build the Walk-The-Block scene with the violet_tbp.fbx character model.
    /// </summary>
    [MenuItem("VioletAndMarlowe/Build Scene With Violet Model")]
    public static void BuildSceneWithVioletCharacter()
    {
        // First, ensure the FBX is imported with correct rig settings
        string fbxPath = "Assets/Art/Characters/Violet/violet_tbp.fbx";
        var importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
        if (importer != null)
        {
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.SaveAndReimport();
            Debug.Log("[WalkTheBlockSceneBuilder] Rig setup applied and saved.");
        }

        // Load the violet_tbp.fbx prefab (scale fixed at import via globalScale)
        GameObject violetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (violetPrefab == null)
        {
            Debug.LogError($"[WalkTheBlockSceneBuilder] Failed to load {fbxPath}");
            return;
        }

        // Build the scene first
        BuildScene();

        // Replace the proxy body with the violet model
        Transform player = GameObject.Find("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("[WalkTheBlockSceneBuilder] Player not found after scene build");
            return;
        }

        var pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.characterModelPrefab = violetPrefab;
            pc.characterModelScale = 142.07f; // post-skin instance scale (bindpose reconciliation is scale-dependent)

            // Destroy existing proxy body before creating character model
            if (pc.proxyBody != null)
            {
                GameObject.DestroyImmediate(pc.proxyBody.gameObject);
                pc.proxyBody = null;
            }

            // Manually create the character model and assign to proxyBody
            var instance = Object.Instantiate(violetPrefab, player);
            instance.name = "PlayerVisual";
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one * 142.07f; // post-skin instance scale

            // Add Animator if not present
            var animator = instance.GetComponentInChildren<Animator>();
            if (animator == null)
                animator = instance.AddComponent<Animator>();

            // Remove any colliders from the model
            var colliders = instance.GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
                Object.DestroyImmediate(col);

            // Assign the violet material with albedo texture
            var smr = instance.GetComponentInChildren<SkinnedMeshRenderer>();
            var violetMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Characters/Violet/violet_material.mat");
            if (violetMat != null && smr != null)
            {
                smr.sharedMaterial = violetMat;
                Debug.Log("[WalkTheBlockSceneBuilder] Assigned violet_material to SkinnedMeshRenderer, mainTexture=" + (smr.sharedMaterial.mainTexture?.name ?? "null"));
            }

            pc.proxyBody = instance.transform;
            Debug.Log("[WalkTheBlockSceneBuilder] Replaced proxy body with violet model, scale=1, animator.isHuman=" + animator.isHuman);
        }

        // Save the scene
        string scenePath = "Assets/Scenes/WalkTheBlock.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[WalkTheBlockSceneBuilder] Scene built with violet_tbp model.");
    }
}
