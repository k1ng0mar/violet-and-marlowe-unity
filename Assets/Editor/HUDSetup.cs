using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem;
using System.Collections;

public static class HUDSetup
{
    // Palette (Build Guide 6.2):
    // Violet rust #B84A3E (0.722, 0.290, 0.243) — player character
    // Marlowe teal #3E7A8C (0.243, 0.478, 0.549) — partner
    // Carrot orange #F2762E (0.949, 0.463, 0.180) — community/hero accent
    // Institutional grey #6E6E73 (0.431, 0.431, 0.451) — buildings
    // Light neutral #E8E8E8 (0.910, 0.910, 0.910) — text/reticle

    public static void SetupHUD()
    {
        var circleSprite = CircleSpriteFactory.GetCircle();

        // === HUD Canvas (separate from controls canvas, raycastTarget=false) ===
        var hudCanvasObj = new GameObject("HUDCanvas");
        var canvas = hudCanvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10; // above controls canvas
        var scaler = hudCanvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        hudCanvasObj.AddComponent<GraphicRaycaster>();
        // Note: HUD canvas has GraphicRaycaster but individual HUD elements have raycastTarget=false

        // === Safe Area wrapper ===
        var safeAreaObj = new GameObject("HUDSafeArea");
        safeAreaObj.transform.SetParent(hudCanvasObj.transform, false);
        var safeAreaRect = safeAreaObj.AddComponent<RectTransform>();
        safeAreaRect.anchorMin = Vector2.zero;
        safeAreaRect.anchorMax = Vector2.one;
        safeAreaRect.offsetMin = Vector2.zero;
        safeAreaRect.offsetMax = Vector2.zero;
        var safeArea = safeAreaObj.AddComponent<SafeAreaFitter>();

        // === 1. Minimap (Top-Left) — bordered square + center pip ===
        var minimapObj = new GameObject("Minimap");
        minimapObj.transform.SetParent(safeAreaObj.transform, false);
        var minimapImg = minimapObj.AddComponent<Image>();
        minimapImg.color = new Color(0.1f, 0.1f, 0.1f, 0.7f); // dark bg
        minimapImg.raycastTarget = false;
        var minimapRect = minimapObj.GetComponent<RectTransform>();
        minimapRect.anchorMin = new Vector2(0.02f, 0.78f);
        minimapRect.anchorMax = new Vector2(0.02f, 0.78f);
        minimapRect.pivot = new Vector2(0, 1);
        minimapRect.sizeDelta = new Vector2(160, 160);
        // Border
        var borderObj = new GameObject("MinimapBorder");
        borderObj.transform.SetParent(minimapObj.transform, false);
        var borderImg = borderObj.AddComponent<Image>();
        borderImg.color = new Color(0.431f, 0.431f, 0.451f, 0.8f); // Institutional grey #6E6E73
        borderImg.raycastTarget = false;
        var borderRect = borderObj.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.pivot = new Vector2(0.5f, 0.5f);
        borderRect.offsetMin = new Vector2(-4, -4);
        borderRect.offsetMax = new Vector2(4, 4);
        // Pip (you-are-here) — PLACEHOLDER, no real map
        var pipObj = new GameObject("MinimapPip");
        pipObj.transform.SetParent(minimapObj.transform, false);
        var pipImg = pipObj.AddComponent<Image>();
        pipImg.sprite = circleSprite;
        pipImg.color = new Color(0.949f, 0.463f, 0.180f, 0.9f); // Carrot orange #F2762E
        pipImg.raycastTarget = false;
        var pipRect = pipObj.GetComponent<RectTransform>();
        pipRect.anchorMin = new Vector2(0.5f, 0.5f);
        pipRect.anchorMax = new Vector2(0.5f, 0.5f);
        pipRect.pivot = new Vector2(0.5f, 0.5f);
        pipRect.sizeDelta = new Vector2(20, 20);

        // MinimapController — updates pip position based on player world XZ
        var minimapController = minimapObj.AddComponent<MinimapController>();
        minimapController.minimapRect = minimapRect;
        minimapController.pipRect = pipRect;
        minimapController.worldSize = new Vector2(80, 80);
        minimapController.worldScale = 2.0f; // 1 world unit = 2px on minimap

        // === 2. Partner Card (Top-Right) — "MARLOWE", HP "100", portrait ringed in teal ===
        var partnerObj = new GameObject("PartnerCard");
        partnerObj.transform.SetParent(safeAreaObj.transform, false);
        var partnerBg = partnerObj.AddComponent<Image>();
        partnerBg.color = new Color(0.1f, 0.1f, 0.1f, 0.6f);
        partnerBg.raycastTarget = false;
        var partnerRect = partnerObj.GetComponent<RectTransform>();
        partnerRect.anchorMin = new Vector2(0.98f, 0.78f);
        partnerRect.anchorMax = new Vector2(0.98f, 0.78f);
        partnerRect.pivot = new Vector2(1, 1);
        partnerRect.sizeDelta = new Vector2(200, 120);
        // Portrait box ringed in Marlowe teal
        var portraitObj = new GameObject("PortraitBox");
        portraitObj.transform.SetParent(partnerObj.transform, false);
        var portraitImg = portraitObj.AddComponent<Image>();
        portraitImg.color = new Color(0.243f, 0.478f, 0.549f, 0.7f); // Marlowe teal #3E7A8C
        portraitImg.raycastTarget = false;
        var portraitRect = portraitObj.GetComponent<RectTransform>();
        portraitRect.anchorMin = new Vector2(0, 0.5f);
        portraitRect.anchorMax = new Vector2(0, 0.5f);
        portraitRect.pivot = new Vector2(0, 0.5f);
        portraitRect.sizeDelta = new Vector2(70, 70);
        portraitRect.anchoredPosition = new Vector2(10, 0);
        // Name text "MARLOWE"
        var nameObj = new GameObject("PartnerName");
        nameObj.transform.SetParent(partnerObj.transform, false);
        var nameText = nameObj.AddComponent<Text>();
        nameText.text = "MARLOWE";
        nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        nameText.fontSize = 22;
        nameText.alignment = TextAnchor.UpperLeft;
        nameText.color = new Color(0.910f, 0.910f, 0.910f, 0.9f); // Light neutral
        nameText.raycastTarget = false;
        var nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.4f, 1);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.pivot = new Vector2(0, 1);
        nameRect.sizeDelta = new Vector2(0, 30);
        nameRect.anchoredPosition = new Vector2(0, -10);
        // HP text "100"
        var hpObj = new GameObject("PartnerHP");
        hpObj.transform.SetParent(partnerObj.transform, false);
        var hpText = hpObj.AddComponent<Text>();
        hpText.text = "HP: 100";
        hpText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hpText.fontSize = 18;
        hpText.alignment = TextAnchor.UpperLeft;
        hpText.color = new Color(0.910f, 0.910f, 0.910f, 0.8f); // Light neutral
        hpText.raycastTarget = false;
        var hpRect = hpObj.GetComponent<RectTransform>();
        hpRect.anchorMin = new Vector2(0.4f, 0.5f);
        hpRect.anchorMax = new Vector2(1, 0.5f);
        hpRect.pivot = new Vector2(0, 0.5f);
        hpRect.sizeDelta = new Vector2(0, 25);
        hpRect.anchoredPosition = new Vector2(0, -5);

        // === 3. Health Bar (Bottom-Center) — full bar ===
        var healthBgObj = new GameObject("HealthBarBG");
        healthBgObj.transform.SetParent(safeAreaObj.transform, false);
        var healthBgImg = healthBgObj.AddComponent<Image>();
        healthBgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.6f);
        healthBgImg.raycastTarget = false;
        var healthBgRect = healthBgObj.GetComponent<RectTransform>();
        healthBgRect.anchorMin = new Vector2(0.5f, 0.04f);
        healthBgRect.anchorMax = new Vector2(0.5f, 0.04f);
        healthBgRect.pivot = new Vector2(0.5f, 0);
        healthBgRect.sizeDelta = new Vector2(300, 24);
        // Fill
        var healthFillObj = new GameObject("HealthBarFill");
        healthFillObj.transform.SetParent(healthBgObj.transform, false);
        var healthFillImg = healthFillObj.AddComponent<Image>();
        healthFillImg.color = new Color(0.722f, 0.290f, 0.243f, 0.85f); // Violet rust #B84A3E
        healthFillImg.raycastTarget = false;
        var healthFillRect = healthFillObj.GetComponent<RectTransform>();
        healthFillRect.anchorMin = Vector2.zero;
        healthFillRect.anchorMax = Vector2.one;
        healthFillRect.pivot = new Vector2(0, 0.5f);
        healthFillRect.offsetMin = new Vector2(2, 2);
        healthFillRect.offsetMax = new Vector2(-2, -2);

        // === 4. Heat/Alarm Bar (Bottom-Center, above health) — segmented, empty ===
        var heatBgObj = new GameObject("HeatBarBG");
        heatBgObj.transform.SetParent(safeAreaObj.transform, false);
        var heatBgImg = heatBgObj.AddComponent<Image>();
        heatBgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.4f);
        heatBgImg.raycastTarget = false;
        var heatBgRect = heatBgObj.GetComponent<RectTransform>();
        heatBgRect.anchorMin = new Vector2(0.5f, 0.09f);
        heatBgRect.anchorMax = new Vector2(0.5f, 0.09f);
        heatBgRect.pivot = new Vector2(0.5f, 0);
        heatBgRect.sizeDelta = new Vector2(300, 16);
        // 5 segment outlines
        for (int i = 0; i < 5; i++)
        {
            var segObj = new GameObject($"HeatSeg{i}");
            segObj.transform.SetParent(heatBgObj.transform, false);
            var segImg = segObj.AddComponent<Image>();
            segImg.color = new Color(0.431f, 0.431f, 0.451f, 0.3f); // Grey, empty
            segImg.raycastTarget = false;
            var segRect = segObj.GetComponent<RectTransform>();
            float segWidth = 56;
            segRect.anchorMin = new Vector2(0, 0.5f);
            segRect.anchorMax = new Vector2(0, 0.5f);
            segRect.pivot = new Vector2(0, 0.5f);
            segRect.sizeDelta = new Vector2(segWidth, 12);
            segRect.anchoredPosition = new Vector2(4 + i * 60, 0);
        }

        // === 5. Weapon Box + Ammo (Bottom-Right, inside safe area, above controls) ===
        var weaponObj = new GameObject("WeaponBox");
        weaponObj.transform.SetParent(safeAreaObj.transform, false);
        var weaponImg = weaponObj.AddComponent<Image>();
        weaponImg.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        weaponImg.raycastTarget = false;
        var weaponRect = weaponObj.GetComponent<RectTransform>();
        weaponRect.anchorMin = new Vector2(0.88f, 0.45f);
        weaponRect.anchorMax = new Vector2(0.88f, 0.45f);
        weaponRect.pivot = new Vector2(1, 0.5f);
        weaponRect.sizeDelta = new Vector2(120, 80);
        // Ammo text
        var ammoObj = new GameObject("AmmoText");
        ammoObj.transform.SetParent(weaponObj.transform, false);
        var ammoText = ammoObj.AddComponent<Text>();
        ammoText.text = "30 / 30";
        ammoText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        ammoText.fontSize = 20;
        ammoText.alignment = TextAnchor.MiddleCenter;
        ammoText.color = new Color(0.910f, 0.910f, 0.910f, 0.9f); // Light neutral
        ammoText.raycastTarget = false;
        var ammoRect = ammoObj.GetComponent<RectTransform>();
        ammoRect.anchorMin = Vector2.zero;
        ammoRect.anchorMax = Vector2.one;
        ammoRect.pivot = new Vector2(0.5f, 0.5f);
        ammoRect.offsetMin = Vector2.zero;
        ammoRect.offsetMax = Vector2.zero;

        // === 6. Reticle (Center — 3 chevrons @ 120° + center pip) ===
        SetupReticle(safeAreaObj.transform, circleSprite);

        // === 7. District Banner (Center, fades in/hold/out) ===
        SetupBanner(safeAreaObj.transform);

        // === 8. Debug Overlay (top edge, HUD canvas) ===
        var debugObj = new GameObject("DebugOverlay");
        debugObj.transform.SetParent(hudCanvasObj.transform, false);
        var debugRect = debugObj.AddComponent<RectTransform>();
        debugRect.anchorMin = new Vector2(0, 1);
        debugRect.anchorMax = new Vector2(1, 1);
        debugRect.pivot = new Vector2(0.5f, 1);
        debugRect.sizeDelta = new Vector2(0, 32);
        debugRect.anchoredPosition = new Vector2(0, 0);
        var debugOverlay = debugObj.AddComponent<DebugOverlay>();

        // === Objective Text (below debug overlay) ===
        var objectiveObj = new GameObject("ObjectiveText");
        objectiveObj.transform.SetParent(hudCanvasObj.transform, false);
        var objectiveRect = objectiveObj.AddComponent<RectTransform>();
        objectiveRect.anchorMin = new Vector2(0.5f, 1);
        objectiveRect.anchorMax = new Vector2(0.5f, 1);
        objectiveRect.pivot = new Vector2(0.5f, 1);
        objectiveRect.sizeDelta = new Vector2(400, 40);
        objectiveRect.anchoredPosition = new Vector2(0, -40);
        var objectiveText = objectiveObj.AddComponent<Text>();
        objectiveText.text = "Get inside the bank";
        objectiveText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        objectiveText.fontSize = 24;
        objectiveText.alignment = TextAnchor.MiddleCenter;
        objectiveText.color = new Color(0.949f, 0.463f, 0.180f, 1f); // Carrot orange #F2762E
        objectiveText.raycastTarget = false;

        // === 9. DevConfigLoader ===
        var devConfigObj = new GameObject("DevConfigLoader");
        var devConfig = devConfigObj.AddComponent<DevConfigLoader>();

        // === 10. BannerController ===
        var bannerObj = GameObject.Find("DistrictBanner");
        if (bannerObj != null)
        {
            var banner = bannerObj.GetComponent<DistrictBannerController>();
            if (banner != null) devConfig.playerController = null; // not needed for dev config
        }

        // Wire DevConfigLoader to PlayerController
        var player = GameObject.Find("Player");
        if (player != null)
        {
            var pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                devConfig.playerController = pc;
                debugOverlay.player = pc;
            }

            // Wire player to MinimapController
            var minimapCtrl = Object.FindObjectOfType<MinimapController>();
            if (minimapCtrl != null)
            {
                minimapCtrl.player = player.transform;
            }

            // Wire objective text to HeistManager
            var heistManager = Object.FindObjectOfType<HeistManager>();
            if (heistManager != null)
            {
                heistManager.objectiveText = objectiveText;
                var districtBanner = GameObject.Find("DistrictBanner");
                if (districtBanner != null)
                    heistManager.banner = districtBanner.GetComponent<DistrictBannerController>();
            }
        }

        Debug.Log("[HUD] Setup complete: minimap, partner card, health/heat bars, weapon/ammo, reticle, banner, debug overlay, dev config");
    }

    static void SetupReticle(Transform parent, Sprite circleSprite)
    {
        var reticleObj = new GameObject("Reticle");
        reticleObj.transform.SetParent(parent, false);
        var reticleRect = reticleObj.AddComponent<RectTransform>();
        reticleRect.anchorMin = new Vector2(0.5f, 0.5f);
        reticleRect.anchorMax = new Vector2(0.5f, 0.5f);
        reticleRect.pivot = new Vector2(0.5f, 0.5f);
        reticleRect.sizeDelta = new Vector2(100, 100);
        reticleRect.anchoredPosition = Vector2.zero;

        // Center pip
        var pipObj = new GameObject("ReticlePip");
        pipObj.transform.SetParent(reticleObj.transform, false);
        var pipImg = pipObj.AddComponent<Image>();
        pipImg.sprite = circleSprite;
        pipImg.color = new Color(0.910f, 0.910f, 0.910f, 0.8f); // Light neutral, 80% alpha
        pipImg.raycastTarget = false;
        var pipRect = pipObj.GetComponent<RectTransform>();
        pipRect.anchorMin = new Vector2(0.5f, 0.5f);
        pipRect.anchorMax = new Vector2(0.5f, 0.5f);
        pipRect.pivot = new Vector2(0.5f, 0.5f);
        pipRect.sizeDelta = new Vector2(8, 8);

        // 3 chevrons at 120 degrees
        for (int i = 0; i < 3; i++)
        {
            float angle = i * 120f;
            var chevronObj = new GameObject($"Chevron{i}");
            chevronObj.transform.SetParent(reticleObj.transform, false);
            // Chevron = a thin rectangular strip rotated to point outward
            var chevronImg = chevronObj.AddComponent<Image>();
            chevronImg.color = new Color(0.910f, 0.910f, 0.910f, 0.8f); // Light neutral, 80%
            chevronImg.raycastTarget = false;
            var chevronRect = chevronObj.GetComponent<RectTransform>();
            chevronRect.anchorMin = new Vector2(0.5f, 0.5f);
            chevronRect.anchorMax = new Vector2(0.5f, 0.5f);
            chevronRect.pivot = new Vector2(0.5f, 0.5f);
            chevronRect.sizeDelta = new Vector2(3, 18);
            // Position at 120° intervals around center, 35px out
            float rad = angle * Mathf.Deg2Rad;
            chevronRect.anchoredPosition = new Vector2(Mathf.Cos(rad) * 35, Mathf.Sin(rad) * 35);
            chevronRect.localEulerAngles = new Vector3(0, 0, angle + 90);
        }

        Debug.Log("[HUD] Reticle: 3 chevrons @ 120° + center pip, anchored center (0.5, 0.5)");
    }

    static void SetupBanner(Transform parent)
    {
        var bannerObj = new GameObject("DistrictBanner");
        bannerObj.transform.SetParent(parent, false);
        var bannerRect = bannerObj.AddComponent<RectTransform>();
        bannerRect.anchorMin = new Vector2(0.5f, 0.5f);
        bannerRect.anchorMax = new Vector2(0.5f, 0.5f);
        bannerRect.pivot = new Vector2(0.5f, 0.5f);
        bannerRect.sizeDelta = new Vector2(800, 120);
        bannerRect.anchoredPosition = new Vector2(0, 60); // slightly above center

        // CanvasGroup for alpha control
        var cg = bannerObj.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        // Text
        var textObj = new GameObject("BannerText");
        textObj.transform.SetParent(bannerObj.transform, false);
        var text = textObj.AddComponent<Text>();
        text.text = "DISTRICT 1 — THE PROCESSING PLANT";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 36;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.949f, 0.463f, 0.180f, 1f); // Carrot orange #F2762E
        text.raycastTarget = false;
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // Controller
        var controller = bannerObj.AddComponent<DistrictBannerController>();

        Debug.Log("[HUD] DistrictBanner: created with CanvasGroup, fade coroutine will run on start");
    }
}
