using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class MobileControlsTests
{
    private GameObject player;
    private PlayerController controller;
    private CharacterController charController;
    private Keyboard keyboard;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        SceneManager.LoadScene("WalkTheBlock");
        yield return null; // Wait one frame for scene to load

        player = GameObject.Find("Player");
        controller = player.GetComponent<PlayerController>();
        charController = player.GetComponent<CharacterController>();
        Assert.IsNotNull(controller, "PlayerController not found");
        Assert.IsNotNull(charController, "CharacterController not found");
    }

    [UnityTest]
    public IEnumerator MobileControls_CanvasExists()
    {
        var canvas = GameObject.Find("MobileControlsCanvas");
        Assert.IsNotNull(canvas, "MobileControlsCanvas not found in scene");

        var canvasComponent = canvas.GetComponent<Canvas>();
        Assert.IsNotNull(canvasComponent, "Canvas component missing");
        Assert.AreEqual(RenderMode.ScreenSpaceOverlay, canvasComponent.renderMode, "Canvas should be ScreenSpace-Overlay");

        var scaler = canvas.GetComponent<CanvasScaler>();
        Assert.IsNotNull(scaler, "CanvasScaler missing");
        Assert.AreEqual(CanvasScaler.ScaleMode.ScaleWithScreenSize, scaler.uiScaleMode, "CanvasScaler should scale with screen size");

        Debug.Log("[PASS] MobileControls_CanvasExists: Canvas is ScreenSpace-Overlay with responsive scaler");
        yield break;
    }

    [UnityTest]
    public IEnumerator MobileControls_JoystickExists()
    {
        var joystick = GameObject.Find("MovementJoystick");
        Assert.IsNotNull(joystick, "MovementJoystick not found");

        var virtualJoystick = joystick.GetComponent<VirtualJoystick>();
        Assert.IsNotNull(virtualJoystick, "VirtualJoystick component missing");

        var knob = joystick.transform.Find("JoystickKnob");
        Assert.IsNotNull(knob, "Joystick knob missing");

        // Check position: bottom-left
        var rect = joystick.GetComponent<RectTransform>();
        Assert.IsTrue(rect.anchoredPosition.x < Screen.width * 0.4f, "Joystick should be in left zone");
        Assert.IsTrue(rect.anchoredPosition.y < Screen.height * 0.3f, "Joystick should be in bottom zone");

        Debug.Log($"[PASS] MobileControls_JoystickExists: joystick at {rect.anchoredPosition}, knob={knob.name}");
        yield break;
    }

    [UnityTest]
    public IEnumerator MobileControls_CameraDragZoneExists()
    {
        var dragZone = GameObject.Find("CameraDragZone");
        Assert.IsNotNull(dragZone, "CameraDragZone not found");

        var handler = dragZone.GetComponent<CameraTouchDragZone>();
        Assert.IsNotNull(handler, "CameraTouchDragZone component missing");
        Assert.IsNotNull(handler.playerController, "Camera drag zone not wired to PlayerController");

        // Check position: right half of screen
        var rect = dragZone.GetComponent<RectTransform>();
        Assert.IsTrue(rect.anchorMin.x >= 0.5f, "Camera drag zone should be in right half");

        Debug.Log($"[PASS] MobileControls_CameraDragZoneExists: drag zone anchored at {rect.anchorMin}, wired to PlayerController");
        yield break;
    }

    [UnityTest]
    public IEnumerator MobileControls_ActionButtonsExist()
    {
        var jumpBtn = GameObject.Find("JumpButton");
        Assert.IsNotNull(jumpBtn, "JumpButton not found");
        var jumpTouchBtn = jumpBtn.GetComponent<TouchButton>();
        Assert.IsNotNull(jumpTouchBtn, "TouchButton component missing on JumpButton");

        var crouchBtn = GameObject.Find("CrouchButton");
        Assert.IsNotNull(crouchBtn, "CrouchButton not found");
        var crouchTouchBtn = crouchBtn.GetComponent<TouchButton>();
        Assert.IsNotNull(crouchTouchBtn, "TouchButton component missing on CrouchButton");

        var dashBtn = GameObject.Find("DashButton");
        Assert.IsNotNull(dashBtn, "DashButton not found");
        var dashTouchBtn = dashBtn.GetComponent<TouchButton>();
        Assert.IsNotNull(dashTouchBtn, "TouchButton component missing on DashButton");

        Debug.Log("[PASS] MobileControls_ActionButtonsExist: Jump, Crouch, Dash buttons all present with TouchButton components");
        yield break;
    }

    [UnityTest]
    public IEnumerator MobileControls_EventSystemHasInputModule()
    {
        var eventSystem = GameObject.Find("EventSystem");
        Assert.IsNotNull(eventSystem, "EventSystem not found");

        var inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        Assert.IsNotNull(inputModule, "InputSystemUIInputModule missing — touch controls won't work without it");

        Debug.Log("[PASS] MobileControls_EventSystemHasInputModule: InputSystemUIInputModule present");
        yield break;
    }

    [UnityTest]
    public IEnumerator MobileControls_JoystickMovesPlayer()
    {
        // Simulate joystick by directly calling SetMoveInput (same callback the joystick uses)
        // Use HORIZONTAL input (X axis) to prove real movement, not floor settle artifact
        Vector3 startPos = player.transform.position;

        controller.SetMoveInput(new Vector2(1, 0)); // East (horizontal)
        yield return new WaitForSeconds(0.5f);
        controller.SetMoveInput(Vector2.zero);

        Vector3 endPos = player.transform.position;
        float horizontalDelta = Mathf.Abs(endPos.x - startPos.x);
        float totalDelta = Vector3.Distance(startPos, endPos);

        Debug.Log($"[TEST] JoystickMove: start={startPos}, end={endPos}, horizontalDelta={horizontalDelta:F3}, totalDelta={totalDelta:F3}");
        Assert.Greater(horizontalDelta, 0.1f, "Joystick input did not move the player horizontally (X delta too small — may be floor settle artifact)");
        Assert.Greater(totalDelta, 0.1f, "Joystick input did not move the player");
        Debug.Log($"[PASS] MobileControls_JoystickMovesPlayer: player moved horizontally {horizontalDelta:F3} (total {totalDelta:F3}) via SetMoveInput");
    }
}
