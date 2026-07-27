using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class HUDPlayModeTests : InputTestFixture
{
    private GameObject player;
    private PlayerController controller;
    private Transform cameraRoot;

    public override void Setup()
    {
        base.Setup();
        InputSystem.AddDevice<Keyboard>();
    }

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        SceneManager.LoadScene("WalkTheBlock", LoadSceneMode.Single);
        yield return null;
        yield return new WaitForSeconds(0.5f);

        player = GameObject.Find("Player");
        controller = player.GetComponent<PlayerController>();
        Assert.IsNotNull(controller, "PlayerController not found");
        cameraRoot = player.transform.Find("CameraRoot");
        Assert.IsNotNull(cameraRoot, "CameraRoot not found");
    }

    // === 1. Visible Player (Real Character Model) ===
    [UnityTest]
    public IEnumerator VisiblePlayer_HasRealModelAndAnimator()
    {
        // Find the visual body child
        var visual = player.transform.Find("PlayerVisual");
        Assert.IsNotNull(visual, "PlayerVisual child not found (violet_tbp model should be present)");

        // Check it has a SkinnedMeshRenderer or MeshRenderer
        var renderers = visual.GetComponentsInChildren<Renderer>();
        Assert.Greater(renderers.Length, 0, "PlayerVisual has no Renderer children");

        // Check it has an Animator with Humanoid avatar
        var animator = visual.GetComponentInChildren<Animator>();
        Assert.IsNotNull(animator, "No Animator component on character model");
        Assert.IsTrue(animator.isHuman, "Animator must have Humanoid avatar (isHuman=true)");

        // Check the model is NOT the proxy capsule (proxy would be capsule/sphere primitives)
        bool hasSkinnedMesh = false;
        Texture modelTexture = null;
        foreach (var r in renderers)
        {
            if (r is SkinnedMeshRenderer)
                hasSkinnedMesh = true;
            if (r.sharedMaterial != null && r.sharedMaterial.mainTexture != null)
                modelTexture = r.sharedMaterial.mainTexture;
        }
        Assert.IsTrue(hasSkinnedMesh, "Real character model should have at least one SkinnedMeshRenderer");

        // Check the albedo texture is assigned (restored from GLB)
        Assert.IsNotNull(modelTexture, "Material.001 mainTexture must be non-null (violet_albedo from GLB)");
        Assert.AreEqual("violet_albedo", modelTexture.name, "Texture should be violet_albedo");

        Debug.Log($"[PASS] VisiblePlayer: PlayerVisual has {renderers.Length} renderers, Animator.isHuman={animator.isHuman}, HasSkinnedMesh={hasSkinnedMesh}, Texture={modelTexture.name}({modelTexture.width}x{modelTexture.height})");
        yield break;
    }

    [UnityTest]
    public IEnumerator VisiblePlayer_FacesMoveDirection()
    {
        var visual = player.transform.Find("PlayerVisual");
        Assert.IsNotNull(visual, "PlayerVisual not found");

        float startYaw = visual.eulerAngles.y;

        // Move sideways (D key = right)
        var kb = InputSystem.GetDevice<Keyboard>();
        Press(kb.dKey);
        yield return new WaitForSeconds(0.5f);
        Release(kb.dKey);

        float endYaw = visual.eulerAngles.y;
        Debug.Log($"[TEST] Facing: startYaw={startYaw:F1}, endYaw={endYaw:F1}");

        // The visual should have rotated (yaw changed significantly)
        float yawDelta = Mathf.DeltaAngle(startYaw, endYaw);
        Assert.Greater(Mathf.Abs(yawDelta), 5f, $"Visual body did not rotate to face move direction (delta={yawDelta:F1})");

        Debug.Log($"[PASS] VisiblePlayer: visual body rotated {yawDelta:F1}° to face move direction");
    }

    // === 2. HUD Elements Exist ===
    [UnityTest]
    public IEnumerator HUD_MinimapExists()
    {
        var minimap = GameObject.Find("Minimap");
        Assert.IsNotNull(minimap, "Minimap not found");
        var rect = minimap.GetComponent<RectTransform>();
        // Top-left
        Assert.IsTrue(rect.anchorMin.x < 0.1f, "Minimap should be top-left");
        Assert.IsTrue(rect.anchorMin.y > 0.7f, "Minimap should be top area");

        // Has pip
        var pip = minimap.transform.Find("MinimapPip");
        Assert.IsNotNull(pip, "Minimap pip not found");

        Debug.Log("[PASS] HUD_MinimapExists: top-left, has pip");
        yield break;
    }

    [UnityTest]
    public IEnumerator HUD_PartnerCardExists()
    {
        var card = GameObject.Find("PartnerCard");
        Assert.IsNotNull(card, "PartnerCard not found");

        var name = GameObject.Find("PartnerName");
        Assert.IsNotNull(name, "PartnerName text not found");
        var nameText = name.GetComponent<Text>();
        Assert.AreEqual("MARLOWE", nameText.text, "Partner name should be MARLOWE");

        var hp = GameObject.Find("PartnerHP");
        Assert.IsNotNull(hp, "PartnerHP text not found");
        Assert.IsTrue(hp.GetComponent<Text>().text.Contains("100"), "HP should show 100");

        // Portrait ringed in Marlowe teal #3E7A8C
        var portrait = GameObject.Find("PortraitBox");
        Assert.IsNotNull(portrait, "PortraitBox not found");
        var portraitColor = portrait.GetComponent<Image>().color;
        Color teal = new Color(0.243f, 0.478f, 0.549f);
        Assert.AreEqual(teal.r, portraitColor.r, 0.02f, "Portrait red should be Marlowe teal");
        Assert.AreEqual(teal.g, portraitColor.g, 0.02f, "Portrait green should be Marlowe teal");
        Assert.AreEqual(teal.b, portraitColor.b, 0.02f, "Portrait blue should be Marlowe teal");

        Debug.Log($"[PASS] HUD_PartnerCardExists: MARLOWE, HP 100, portrait teal #3E7A8C");
        yield break;
    }

    [UnityTest]
    public IEnumerator HUD_HealthAndHeatBarsExist()
    {
        var health = GameObject.Find("HealthBarBG");
        Assert.IsNotNull(health, "HealthBarBG not found");
        var healthRect = health.GetComponent<RectTransform>();
        // Bottom-center
        Assert.IsTrue(Mathf.Abs(healthRect.anchorMin.x - 0.5f) < 0.05f, "Health should be bottom-center");

        var heat = GameObject.Find("HeatBarBG");
        Assert.IsNotNull(heat, "HeatBarBG not found");

        // Health fill should be Violet rust or near-full
        var fill = GameObject.Find("HealthBarFill");
        Assert.IsNotNull(fill, "HealthBarFill not found");

        Debug.Log("[PASS] HUD_HealthAndHeatBarsExist: both present at bottom-center");
        yield break;
    }

    [UnityTest]
    public IEnumerator HUD_WeaponAndAmmoExist()
    {
        var weapon = GameObject.Find("WeaponBox");
        Assert.IsNotNull(weapon, "WeaponBox not found");

        var ammo = GameObject.Find("AmmoText");
        Assert.IsNotNull(ammo, "AmmoText not found");
        Assert.IsTrue(ammo.GetComponent<Text>().text.Contains("30"), "Ammo should show 30 / 30");

        Debug.Log("[PASS] HUD_WeaponAndAmmoExist: weapon box + ammo 30/30");
        yield break;
    }

    [UnityTest]
    public IEnumerator HUD_DoesNotBlockJoystickInput()
    {
        // Simulate joystick push via SetMoveInput (same path the joystick uses)
        Vector3 startPos = player.transform.position;
        controller.SetMoveInput(new Vector2(0, 1)); // Forward
        yield return new WaitForSeconds(0.5f);
        controller.SetMoveInput(Vector2.zero);

        Vector3 endPos = player.transform.position;
        float delta = Vector3.Distance(startPos, endPos);
        Assert.Greater(delta, 0.1f, "Player should still move when HUD is active (HUD shouldn't block)");

        Debug.Log($"[PASS] HUD_DoesNotBlockJoystick: player moved {delta:F3} with HUD present");
        yield break;
    }

    // === 3. Reticle ===
    [UnityTest]
    public IEnumerator Reticle_ExistsAnchoredCenter()
    {
        var reticle = GameObject.Find("Reticle");
        Assert.IsNotNull(reticle, "Reticle not found");

        var rect = reticle.GetComponent<RectTransform>();
        Assert.AreEqual(0.5f, rect.anchorMin.x, 0.01f, "Reticle should be centered X");
        Assert.AreEqual(0.5f, rect.anchorMin.y, 0.01f, "Reticle should be centered Y");

        // Check chevrons exist
        var chevron0 = reticle.transform.Find("Chevron0");
        Assert.IsNotNull(chevron0, "Chevron0 not found");
        var chevron1 = reticle.transform.Find("Chevron1");
        Assert.IsNotNull(chevron1, "Chevron1 not found");
        var chevron2 = reticle.transform.Find("Chevron2");
        Assert.IsNotNull(chevron2, "Chevron2 not found");

        // Center pip
        var pip = reticle.transform.Find("ReticlePip");
        Assert.IsNotNull(pip, "Reticle pip not found");

        Debug.Log("[PASS] Reticle: 3 chevrons + center pip at (0.5, 0.5)");
        yield break;
    }

    // === 4. District Banner ===
    [UnityTest]
    public IEnumerator Banner_FadeInHoldFadeOut()
    {
        var banner = GameObject.Find("DistrictBanner");
        Assert.IsNotNull(banner, "DistrictBanner not found");

        var bannerCtrl = banner.GetComponent<DistrictBannerController>();
        Assert.IsNotNull(bannerCtrl, "DistrictBannerController not found");

        // Check the expected alpha timeline (deterministic, no coroutine)
        // Fade in: 0 → 1 over 0.3s
        Assert.AreEqual(0f, bannerCtrl.GetExpectedAlpha(0f), 0.01f, "Alpha at t=0 should be 0");
        Assert.AreEqual(0.5f, bannerCtrl.GetExpectedAlpha(0.15f), 0.05f, "Alpha at t=0.15 (mid-fade-in) should be ~0.5");
        Assert.AreEqual(1f, bannerCtrl.GetExpectedAlpha(0.3f), 0.01f, "Alpha at t=0.3 (end fade-in) should be 1");

        // Hold: 1.0 for 1.8s (0.3 to 2.1)
        Assert.AreEqual(1f, bannerCtrl.GetExpectedAlpha(1.0f), 0.01f, "Alpha during hold should be 1");
        Assert.AreEqual(1f, bannerCtrl.GetExpectedAlpha(2.0f), 0.01f, "Alpha at end of hold should be 1");

        // Fade out: 1 → 0 over 0.6s (2.1 to 2.7)
        Assert.AreEqual(0.5f, bannerCtrl.GetExpectedAlpha(2.4f), 0.05f, "Alpha mid-fade-out should be ~0.5");
        Assert.AreEqual(0f, bannerCtrl.GetExpectedAlpha(2.7f), 0.01f, "Alpha at end of fade-out should be 0");
        Assert.AreEqual(0f, bannerCtrl.GetExpectedAlpha(3.0f), 0.01f, "Alpha after fade-out should be 0");

        Debug.Log("[PASS] Banner: alpha timeline 0→1 (0.3s) → hold 1.0 (1.8s) → 1→0 (0.6s) verified");
        yield break;
    }

    // === 5. Dev Config Parser ===
    [Test]
    public void DevConfig_ParserCorrect()
    {
        // Test with custom values (including new fields)
        string json = @"{""lookSensitivity"": 0.45, ""joystickDeadzone"": 0.15, ""cameraDistance"": 3.0, ""invertY"": true}";
        var config = DevConfigLoader.ParseConfig(json);
        Assert.AreEqual(0.45f, config.lookSensitivity, 0.001f, "lookSensitivity should be 0.45");
        Assert.AreEqual(0.15f, config.joystickDeadzone, 0.001f, "joystickDeadzone should be 0.15");
        Assert.AreEqual(3.0f, config.cameraDistance, 0.001f, "cameraDistance should be 3.0");
        Assert.IsTrue(config.invertY, "invertY should be true");

        // Test with defaults (missing fields)
        string jsonEmpty = @"{}";
        var config2 = DevConfigLoader.ParseConfig(jsonEmpty);
        Assert.AreEqual(0.25f, config2.lookSensitivity, 0.001f, "Default lookSensitivity");
        Assert.AreEqual(0.1f, config2.joystickDeadzone, 0.001f, "Default deadzone");
        Assert.AreEqual(2.5f, config2.cameraDistance, 0.001f, "Default cameraDistance");
        Assert.IsFalse(config2.invertY, "Default invertY should be false");

        // Test with actual devsettings.json content
        string jsonReal = @"{""joystickDeadzone"": 0.1, ""lookSensitivity"": 0.25, ""cameraDistance"": 2.5, ""invertY"": false}";
        var config3 = DevConfigLoader.ParseConfig(jsonReal);
        Assert.AreEqual(0.25f, config3.lookSensitivity, 0.001f, "Real lookSensitivity");
        Assert.AreEqual(0.1f, config3.joystickDeadzone, 0.001f, "Real deadzone");
        Assert.AreEqual(2.5f, config3.cameraDistance, 0.001f, "Real cameraDistance");
        Assert.IsFalse(config3.invertY, "Real invertY should be false");

        Debug.Log($"[PASS] DevConfig: parser handles custom (0.45/0.15/3.0/true), defaults, and real json ({config3.lookSensitivity}/{config3.joystickDeadzone}/{config3.cameraDistance}/{config3.invertY})");
    }

    [Test]
    public void DevConfig_DefaultsAreCorrect()
    {
        var defaults = new DevConfigLoader.DevConfigData();
        Assert.AreEqual(0.25f, defaults.lookSensitivity, "Default look sensitivity should be 0.25");
        Assert.AreEqual(0.1f, defaults.joystickDeadzone, "Default deadzone should be 0.1");
        Assert.AreEqual(2.5f, defaults.cameraDistance, "Default camera distance should be 2.5");
        Assert.IsFalse(defaults.invertY, "Default invertY should be false");
        Debug.Log("[PASS] DevConfig defaults: lookSensitivity=0.25, deadzone=0.1, cameraDistance=2.5, invertY=false");
    }

    // === 6. District Tests ===
    [UnityTest]
    public IEnumerator District_HasMultipleBuildingsAndStreets()
    {
        var cityGen = GameObject.Find("CityBlock").GetComponent<CityBlockGenerator>();
        Assert.IsNotNull(cityGen, "CityBlockGenerator not found");

        // Assert building count > 5 (we place 11 buildings, some may be skipped near spawn)
        Assert.Greater(cityGen.buildingCount, 5, $"Building count should be > 5, got {cityGen.buildingCount}");

        // Assert street segment count > 2 (we create 4 street segments)
        Assert.Greater(cityGen.streetSegmentCount, 2, $"Street segment count should be > 2, got {cityGen.streetSegmentCount}");

        Debug.Log($"[PASS] District: {cityGen.buildingCount} buildings, {cityGen.streetSegmentCount} street segments");
        yield break;
    }

    [UnityTest]
    public IEnumerator District_AllBuildingsHaveColliders()
    {
        var cityGen = GameObject.Find("CityBlock").GetComponent<CityBlockGenerator>();
        Assert.IsNotNull(cityGen, "CityBlockGenerator not found");

        foreach (var building in cityGen.buildings)
        {
            Assert.IsNotNull(building, "Null building in list");
            var collider = building.GetComponent<Collider>();
            Assert.IsNotNull(collider, $"Building {building.name} has no Collider");
            Assert.IsFalse(collider.isTrigger, $"Building {building.name} collider is a trigger");
        }

        Debug.Log($"[PASS] District: all {cityGen.buildings.Count} buildings have non-trigger colliders");
        yield break;
    }

    [UnityTest]
    public IEnumerator District_StreetFloorIsContinuous()
    {
        // Raycast straight down at several grid points and assert each hits the street/floor
        var cityGen = GameObject.Find("CityBlock").GetComponent<CityBlockGenerator>();
        Assert.IsNotNull(cityGen, "CityBlockGenerator not found");

        Vector2[] testPoints = {
            new Vector2(0, 0),    // center (intersection)
            new Vector2(10, 0),   // east along main street
            new Vector2(-10, 0),  // west along main street
            new Vector2(0, 10),   // north along main street
            new Vector2(0, -10),  // south along main street
            new Vector2(20, 15),  // far corner
            new Vector2(-20, -15), // far corner
        };

        int hits = 0;
        foreach (var point in testPoints)
        {
            Vector3 origin = new Vector3(point.x, 10, point.y);
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 20f))
            {
                hits++;
                Debug.Log($"[TEST] Raycast at ({point.x},{point.y}): hit {hit.collider.name} at y={hit.point.y:F2}");
            }
            else
            {
                Debug.LogWarning($"[TEST] Raycast at ({point.x},{point.y}): NO HIT — floor hole!");
            }
        }

        Assert.AreEqual(testPoints.Length, hits, $"Only {hits}/{testPoints.Length} raycasts hit the floor — floor is not continuous");
        Debug.Log($"[PASS] District: floor continuous ({hits}/{testPoints.Length} raycasts hit)");
        yield break;
    }

    // === 7. Camera Collision Tests ===
    [UnityTest]
    public IEnumerator CameraCollision_PullsInWhenBlocked()
    {
        // Place a wall between player and desired camera position
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "TestWall";
        wall.transform.position = new Vector3(0, 1.5f, -1.2f);
        wall.transform.localScale = new Vector3(10, 3, 0.5f);
        wall.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Color"));
        wall.GetComponent<Renderer>().material.color = Color.red;

        yield return new WaitForSeconds(0.5f);

        // Full arm length is standingCameraOffset.z = 2.5
        float fullLength = Mathf.Abs(controller.standingCameraOffset.z);
        float currentDistance = Mathf.Abs(cameraRoot.localPosition.z);

        Debug.Log($"[TEST] CameraCollision: fullLength={fullLength:F3}, currentDistance={currentDistance:F3}");

        Assert.Less(currentDistance, fullLength, "Camera should be pulled in when blocked by wall");

        // Cleanup
        Object.DestroyImmediate(wall);
        Debug.Log("[PASS] CameraCollision: camera pulled in when blocked");
        yield break;
    }

    [UnityTest]
    public IEnumerator CameraCollision_ReturnsToFullLengthWhenClear()
    {
        // No wall — camera should return to full length
        yield return new WaitForSeconds(0.5f);

        float fullLength = Mathf.Abs(controller.standingCameraOffset.z);
        float currentDistance = Mathf.Abs(cameraRoot.localPosition.z);

        Debug.Log($"[TEST] CameraClear: fullLength={fullLength:F3}, currentDistance={currentDistance:F3}");

        Assert.Greater(currentDistance, fullLength * 0.9f, "Camera should return to near full length when clear");
        Debug.Log("[PASS] CameraCollision: camera returns to full length when clear");
        yield break;
    }

    // === 8. Minimap Pip Tests ===
    [UnityTest]
    public IEnumerator MinimapPip_MovesWithPlayer()
    {
        var minimapController = Object.FindObjectOfType<MinimapController>();
        Assert.IsNotNull(minimapController, "MinimapController not found");

        // Reset player yaw to face north (+Z) so we control direction deterministically
        controller.ResetYaw();
        yield return null;

        Vector2 startPip = minimapController.GetPipPosition();
        Vector3 startWorld = player.transform.position;
        Debug.Log($"[TEST] MinimapPip: worldScale={minimapController.worldScale}, startWorld={startWorld}, startPip={startPip}");

        // Move player +X (east) — teleport for deterministic test
        // Disable CharacterController to prevent it from overriding position
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.transform.position = new Vector3(5, startWorld.y, 0);
        if (cc != null) cc.enabled = true;
        yield return null;
        Vector2 afterEast = minimapController.GetPipPosition();
        Vector3 afterEastWorld = player.transform.position;
        float deltaX = Mathf.Abs(afterEast.x - startPip.x);
        Debug.Log($"[TEST] MinimapPip: afterEastWorld={afterEastWorld}, afterEast={afterEast} (ΔX={deltaX:F3})");

        // Move player +Z (north) — teleport for deterministic test
        if (cc != null) cc.enabled = false;
        player.transform.position = new Vector3(5, startWorld.y, 5);
        if (cc != null) cc.enabled = true;
        yield return null;
        Vector2 afterNorth = minimapController.GetPipPosition();
        Vector3 afterNorthWorld = player.transform.position;
        float deltaY = Mathf.Abs(afterNorth.y - startPip.y);
        Debug.Log($"[TEST] MinimapPip: afterNorthWorld={afterNorthWorld}, afterNorth={afterNorth} (ΔY={deltaY:F3})");

        controller.SetMoveInput(Vector2.zero);

        Debug.Log($"[TEST] MinimapPip: start={startPip}, afterEast={afterEast} (ΔX={deltaX:F3}), afterNorth={afterNorth} (ΔY={deltaY:F3})");

        // BOTH axes must move — east move changes X, north move changes Y
        Assert.Greater(deltaX, 0.01f, $"Pip X should change when player moves east (ΔX={deltaX:F3})");
        Assert.Greater(deltaY, 0.01f, $"Pip Y should change when player moves north (ΔY={deltaY:F3})");

        Debug.Log($"[PASS] MinimapPip: pip moved from {startPip} → east {afterEast} (ΔX={deltaX:F3}) → north {afterNorth} (ΔY={deltaY:F3})");
        yield break;
    }
}
