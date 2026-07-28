using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class MobileControlsSetup
{
    /// <summary>
    /// Fix script reference for a component that was added via AddComponent<T>
    /// but whose script lives in an asmdef. Unity may serialize these with a
    /// local fileID instead of the proper GUID+11500000, causing the component
    /// to deserialize as a missing script at runtime.
    /// </summary>
    static void FixScriptReference<T>(GameObject go) where T : MonoBehaviour
    {
        var comp = go.GetComponent<T>();
        if (comp == null) return;
        // Walk Assets/Scripts to find files containing this type name
        var scriptDir = Application.dataPath + "/Scripts";
        var dirInfo = new System.IO.DirectoryInfo(scriptDir);
        foreach (var csFile in dirInfo.GetFiles("*.cs", System.IO.SearchOption.AllDirectories))
        {
            var content = System.IO.File.ReadAllText(csFile.FullName);
            if (!content.Contains($"public class {typeof(T).Name}")) continue;
            var relativePath = "Assets/Scripts/" + csFile.Name;
            var ms = AssetDatabase.LoadAssetAtPath<MonoScript>(relativePath);
            if (ms != null)
            {
                var so = new SerializedObject(comp);
                var sp = so.FindProperty("m_Script");
                sp.objectReferenceValue = ms;
                so.ApplyModifiedProperties();
                Debug.Log($"[MobileControlsSetup] Fixed script reference for {typeof(T).Name} via {relativePath}");
                return;
            }
        }
        Debug.LogWarning($"[MobileControlsSetup] Could not find MonoScript for {typeof(T).Name}");
    }
    public static void SetupMobileControls()
    {
        var player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogError("Player not found in scene");
            return;
        }

        var playerController = player.GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerController not found on Player");
            return;
        }

        var circleSprite = CircleSpriteFactory.GetCircle();

        // === Canvas ===
        var canvasObj = new GameObject("MobileControlsCanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // === Safe Area container ===
        var safeAreaObj = new GameObject("SafeArea");
        safeAreaObj.transform.SetParent(canvasObj.transform, false);
        var safeAreaRect = safeAreaObj.AddComponent<RectTransform>();
        var safeArea = safeAreaObj.AddComponent<SafeAreaFitter>();
        FixScriptReference<SafeAreaFitter>(safeAreaObj);
        safeAreaRect.anchorMin = Vector2.zero;
        safeAreaRect.anchorMax = Vector2.one;
        safeAreaRect.offsetMin = Vector2.zero;
        safeAreaRect.offsetMax = Vector2.zero;

        // === EventSystem ===
        var eventSystem = GameObject.Find("EventSystem");
        if (eventSystem == null)
        {
            eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
        }
        if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
        {
            var oldModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (oldModule != null) Object.DestroyImmediate(oldModule);
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }
        // Ensure GraphicRaycaster on the canvas works with Input System
        // The InputSystemUIInputModule must be serialized correctly in the scene
        Debug.Log("[MobileControls] EventSystem has InputSystemUIInputModule: " + (eventSystem.GetComponent<InputSystemUIInputModule>() != null));

        // === Virtual Joystick (Bottom-Left, inside safe area) ===
        var stickBgObj = new GameObject("MovementJoystick");
        stickBgObj.transform.SetParent(safeAreaObj.transform, false);
        var stickBgImg = stickBgObj.AddComponent<Image>();
        stickBgImg.sprite = circleSprite;
        stickBgImg.color = new Color(1, 1, 1, 0.2f);
        stickBgImg.raycastTarget = true;
        var stickBgRect = stickBgObj.GetComponent<RectTransform>();
        stickBgRect.anchorMin = new Vector2(0.12f, 0.14f);
        stickBgRect.anchorMax = new Vector2(0.12f, 0.14f);
        stickBgRect.pivot = new Vector2(0.5f, 0.5f);
        stickBgRect.sizeDelta = new Vector2(260, 260);

        var joystick = stickBgObj.AddComponent<VirtualJoystick>();
        joystick.maxRadius = 110f;

        // Knob
        var knobObj = new GameObject("JoystickKnob");
        knobObj.transform.SetParent(stickBgObj.transform, false);
        var knobImg = knobObj.AddComponent<Image>();
        knobImg.sprite = circleSprite;
        knobImg.color = new Color(1, 1, 1, 0.6f);
        knobImg.raycastTarget = false; // Don't block touches to the base
        var knobRect = knobObj.GetComponent<RectTransform>();
        knobRect.anchorMin = new Vector2(0.5f, 0.5f);
        knobRect.anchorMax = new Vector2(0.5f, 0.5f);
        knobRect.pivot = new Vector2(0.5f, 0.5f);
        knobRect.sizeDelta = new Vector2(90, 90);
        joystick.knob = knobRect;

        // Wire joystick → PlayerController.SetMoveInput (SAME moveInput keyboard uses)
        joystick.OnValueChanged = (Vector2 value) =>
        {
            playerController.SetMoveInput(value);
        };

        // === Camera Drag Zone (Right portion, above buttons) ===
        var dragZoneObj = new GameObject("CameraDragZone");
        dragZoneObj.transform.SetParent(safeAreaObj.transform, false);
        var dragZoneImg = dragZoneObj.AddComponent<Image>();
        dragZoneImg.color = new Color(0, 0, 0, 0); // transparent but raycastable
        dragZoneImg.raycastTarget = true;
        var dragZoneRect = dragZoneObj.GetComponent<RectTransform>();
        dragZoneRect.anchorMin = new Vector2(0.5f, 0.4f);
        dragZoneRect.anchorMax = new Vector2(0.88f, 1f);
        dragZoneRect.pivot = new Vector2(0.5f, 0.5f);
        dragZoneRect.offsetMin = Vector2.zero;
        dragZoneRect.offsetMax = Vector2.zero;

        var dragZone = dragZoneObj.AddComponent<CameraTouchDragZone>();
        FixScriptReference<CameraTouchDragZone>(dragZoneObj);
        dragZone.playerController = playerController;

        // === Buttons (Bottom-Right, pulled in from edges) ===
        // Jump — anchor 0.82, size 150
        var jumpBtn = CreateButton(safeAreaObj.transform, "JumpButton",
            new Color(0.2f, 0.6f, 1f, 0.5f),
            new Vector2(0.82f, 0.14f), new Vector2(150, 150), "JUMP", circleSprite);
        jumpBtn.OnPressed = () => playerController.TriggerJump();

        // Crouch — anchor 0.68, size 130, toggle behavior
        var crouchBtn = CreateButton(safeAreaObj.transform, "CrouchButton",
            new Color(0.8f, 0.4f, 0.2f, 0.5f),
            new Vector2(0.68f, 0.14f), new Vector2(130, 130), "CROUCH", circleSprite);
        bool isCrouching = false;
        crouchBtn.OnPressed = () =>
        {
            isCrouching = !isCrouching;
            playerController.SetCrouch(isCrouching);
        };
        crouchBtn.OnReleased = () => { }; // toggle stays until tapped again

        // Dash — anchor 0.82, higher up (0.30), size 120
        var dashBtn = CreateButton(safeAreaObj.transform, "DashButton",
            new Color(0.8f, 0.2f, 0.8f, 0.5f),
            new Vector2(0.82f, 0.30f), new Vector2(120, 120), "DASH", circleSprite);
        dashBtn.OnPressed = () => playerController.TriggerDash();

        Debug.Log("[MobileControls] Setup complete: joystick, camera drag, jump/crouch/dash buttons, safe area, circle sprites");
    }

    static TouchButton CreateButton(Transform parent, string name, Color color, Vector2 anchor, Vector2 size, string label, Sprite circleSprite)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var img = obj.AddComponent<Image>();
        img.sprite = circleSprite;
        img.color = color;
        img.raycastTarget = true;
        var rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;

        // Label — centered inside the button
        var labelObj = new GameObject("Label");
        labelObj.transform.SetParent(obj.transform, false);
        var text = labelObj.AddComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 24;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        var labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.pivot = new Vector2(0.5f, 0.5f);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        var tb = obj.AddComponent<TouchButton>();
        FixScriptReference<TouchButton>(obj);
        return tb;
    }
}

/// <summary>
/// Adjusts a RectTransform to respect Screen.safeArea.
/// Parent of all mobile controls so nothing is clipped by notch/gesture insets.
/// </summary>
public class SafeAreaFitter : MonoBehaviour
{
    private RectTransform rect;
    private Rect lastSafeArea;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        lastSafeArea = Screen.safeArea;
        ApplySafeArea();
    }

    void Update()
    {
        if (Screen.safeArea != lastSafeArea)
        {
            lastSafeArea = Screen.safeArea;
            ApplySafeArea();
        }
    }

    void ApplySafeArea()
    {
        if (rect == null) return;
        var safe = Screen.safeArea;
        var parentRect = rect.parent.GetComponent<RectTransform>();

        // Convert safe area to parent's local space
        Vector2 anchorMin = safe.position;
        Vector2 anchorMax = safe.position + safe.size;
        anchorMin.x /= parentRect.rect.width;
        anchorMin.y /= parentRect.rect.height;
        anchorMax.x /= parentRect.rect.width;
        anchorMax.y /= parentRect.rect.height;

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
