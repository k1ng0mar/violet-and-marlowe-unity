using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 7f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.25f;
    public float dashCooldown = 1.5f;

    [Header("Crouch")]
    public float crouchHeight = 1f;
    public float standingHeight = 2f;
    public float crouchTransitionSpeed = 8f;

    [Header("Camera")]
    public Transform cameraRoot;
    public Vector3 standingCameraOffset = new Vector3(0, 1.6f, -2.5f);
    public Vector3 crouchingCameraOffset = new Vector3(0, 0.8f, -1.5f);
    [Tooltip("Degrees per pixel of touch/mouse delta. 0.25f = ~30-60 deg swipe on a typical phone.")]
    public float lookSensitivity = 0.25f;
    [Header("Camera Collision")]
    public bool enableCameraCollision = true;
    public LayerMask cameraCollisionLayers = 1; // Default
    public float cameraCollisionBuffer = 0.2f;
    private Camera mainCamera;

    [Header("Visual")]
    [Tooltip("Child visual body. Auto-created as proxy if null.")]
    public Transform proxyBody;
    [Tooltip("Real character model prefab. If set, replaces proxy body at runtime. No collider (CharacterController is the collider).")]
    public GameObject characterModelPrefab;
    [Tooltip("Uniform instance scale for the character model. Post-skin transform — the only safe scaling method for this asset (bindpose reconciliation is scale-dependent).")]
    public float characterModelScale = 142.07f; // 140 × 1.8/1.7737, tuned to hit ~1.8m skinned
    public float visualTurnSpeed = 10f;
    [Tooltip("AnimatorController for the character model (VioletAnimator). Assigned in inspector or falls back to none.")]
    public RuntimeAnimatorController characterAnimatorController;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isRunning;
    private bool isCrouching;
    private bool isDashing;
    private float dashTimer;
    private float lastDashTime;
    private Vector2 moveInput;
    private bool usingTouchInput; // when true, UpdateInput() won't overwrite moveInput from keyboard
    private float yaw;
    private float pitch;
    private Animator animator;
    private Animator modelAnimator;
    private static readonly int AnimSpeed = Animator.StringToHash("Speed");
    private static readonly int AnimJump = Animator.StringToHash("Jump");

    // --- Public read-only state for debug overlay ---
    public Vector2 MoveInput => moveInput;
    public Vector2 LastLookDelta { get; private set; } = Vector2.zero;
    public bool IsGrounded => isGrounded;
    public bool IsJumping => !isGrounded && velocity.y > 0;
    public bool IsCrouching => isCrouching;
    public bool IsDashing => isDashing;
    public float VelocityY => velocity.y;
    public float Yaw => yaw;
    public float Pitch => pitch;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cameraRoot == null)
            cameraRoot = new GameObject("CameraRoot").transform;

        yaw = transform.eulerAngles.y;
        pitch = cameraRoot.localEulerAngles.x;

        if (proxyBody == null)
        {
            if (characterModelPrefab != null)
            {
                proxyBody = CreateCharacterModel();
            }
            else
            {
                proxyBody = CreateProxyBody();
            }
        }
    }

    /// <summary>
    /// Create a simple proxy body: cylinder (torso) + sphere (head), Violet rust #B84A3E.
    /// NO collider — the CharacterController is the collider.
    /// </summary>
    Transform CreateProxyBody()
    {
        var body = new GameObject("PlayerVisual");
        body.transform.SetParent(transform, false);
        body.transform.localPosition = new Vector3(0, 0, 0);

        // Torso: capsule mesh, NO collider (CharacterController is the collider)
        var torso = new GameObject("Torso");
        torso.transform.SetParent(body.transform, false);
        torso.transform.localScale = new Vector3(0.6f, 0.8f, 0.6f);
        torso.transform.localPosition = new Vector3(0, 1.0f, 0);
        var torsoMesh = torso.AddComponent<MeshRenderer>();
        var torsoFilter = torso.AddComponent<MeshFilter>();
        torsoFilter.mesh = CreateCapsuleMesh();
        // NO collider added — CharacterController handles collision

        // Head: sphere mesh, NO collider
        var head = new GameObject("Head");
        head.transform.SetParent(body.transform, false);
        head.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        head.transform.localPosition = new Vector3(0, 1.8f, 0);
        var headMesh = head.AddComponent<MeshRenderer>();
        var headFilter = head.AddComponent<MeshFilter>();
        headFilter.mesh = CreateSphereMesh();
        // NO collider added

        // Apply Violet rust material #B84A3E (0.722, 0.290, 0.243)
        Color violetRust = new Color(0.722f, 0.290f, 0.243f);
        var mat = new Material(Shader.Find("Unlit/Color"));
        mat.color = violetRust;
        torsoMesh.material = mat;
        headMesh.material = mat;

        Debug.Log("[PlayerController] Proxy body created with material color: (" + mat.color.r.ToString("F3") + ", " + mat.color.g.ToString("F3") + ", " + mat.color.b.ToString("F3") + ") = #B84A3E");

        return body.transform;
    }

    /// <summary>
    /// Create character model from prefab. Scales to ~1.8m, adds Animator with Humanoid avatar. NO collider.
    /// </summary>
    Transform CreateCharacterModel()
    {
        if (characterModelPrefab == null)
            return CreateProxyBody();

        var instance = Instantiate(characterModelPrefab, transform);
        instance.name = "PlayerVisual";
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one * characterModelScale;

        // Add Animator component — it will auto-detect the Humanoid avatar on import
        var animator = instance.GetComponentInChildren<Animator>();
        if (animator == null)
            animator = instance.AddComponent<Animator>();

        // Wire the animation controller (Mixamo clips retargeted to violet_tbpAvatar)
        if (characterAnimatorController != null)
            animator.runtimeAnimatorController = characterAnimatorController;
        modelAnimator = animator;

        // Ensure no colliders on the model — CharacterController is the collision
        var colliders = instance.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
            DestroyImmediate(col);

        Debug.Log("[PlayerController] Character model instantiated, scale=" + characterModelScale + ", animator.isHuman=" + animator.isHuman);
        return instance.transform;
    }

    /// <summary>
    /// Create a simple capsule mesh procedurally (avoids CreatePrimitive which adds colliders).
    /// </summary>
    Mesh CreateCapsuleMesh()
    {
        // Use a primitive Capsule but strip the collider before it's added
        var temp = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        var mesh = temp.GetComponent<MeshFilter>().sharedMesh;
        Object.DestroyImmediate(temp);
        return mesh;
    }

    /// <summary>
    /// Create a simple sphere mesh procedurally (avoids CreatePrimitive which adds colliders).
    /// </summary>
    Mesh CreateSphereMesh()
    {
        var temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var mesh = temp.GetComponent<MeshFilter>().sharedMesh;
        Object.DestroyImmediate(temp);
        return mesh;
    }

    void Update()
    {
        UpdateGrounded();
        UpdateInput();
        UpdateMovement();
        UpdateCrouch();
        UpdateDash();
        UpdateCamera();
        UpdateVisualFacing();
        UpdateAnimation();
    }

    /// <summary>
    /// Feed the animator: Speed (damped horizontal planar speed) and Jump trigger on liftoff.
    /// </summary>
    void UpdateAnimation()
    {
        if (modelAnimator == null) return;
        Vector3 planarVel = controller.velocity;
        planarVel.y = 0;
        modelAnimator.SetFloat(AnimSpeed, planarVel.magnitude, 0.1f, Time.deltaTime);
        if (IsJumping)
            modelAnimator.SetTrigger(AnimJump);
    }

    void UpdateGrounded()
    {
        if (!controller.isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;
    }

    void UpdateInput()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        // If touch input is being used, don't overwrite moveInput from keyboard
        if (!usingTouchInput)
        {
            float h = 0, v = 0;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) h -= 1;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h += 1;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) v += 1;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) v -= 1;
            moveInput = new Vector2(h, v);
        }

        isRunning = kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed;

        if (kb.spaceKey.wasPressedThisFrame && isGrounded && !isCrouching && !isDashing)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        isCrouching = kb.leftCtrlKey.isPressed || kb.rightCtrlKey.isPressed || kb.cKey.isPressed;

        if ((kb.qKey.wasPressedThisFrame || kb.eKey.wasPressedThisFrame) && !isDashing && !isCrouching && Time.time - lastDashTime >= dashCooldown)
        {
            isDashing = true;
            dashTimer = dashDuration;
            lastDashTime = Time.time;
        }

        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            ApplyLookDelta(mouseDelta);
        }
    }

    void UpdateMovement()
    {
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        Vector3 move = forward * moveInput.y + right * moveInput.x;

        float targetSpeed = isRunning ? runSpeed : walkSpeed;
        if (isDashing)
            targetSpeed = dashSpeed;

        Vector3 moveVelocity = move * targetSpeed;
        moveVelocity.y = velocity.y;

        controller.Move(moveVelocity * Time.deltaTime);
    }

    void UpdateCrouch()
    {
        if (isCrouching)
        {
            controller.height = Mathf.Lerp(controller.height, crouchHeight, Time.deltaTime * crouchTransitionSpeed);
        }
        else
        {
            controller.height = Mathf.Lerp(controller.height, standingHeight, Time.deltaTime * crouchTransitionSpeed);
        }
    }

    void UpdateDash()
    {
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
                isDashing = false;
        }
    }

    void UpdateCamera()
    {
        if (cameraRoot == null) return;
        transform.eulerAngles = new Vector3(0, yaw, 0);
        cameraRoot.localEulerAngles = new Vector3(pitch, 0, 0);
        Vector3 targetOffset = isCrouching ? crouchingCameraOffset : standingCameraOffset;

        if (enableCameraCollision && cameraRoot != null)
        {
            // Raycast from head height toward desired camera position
            Vector3 headPos = transform.position + Vector3.up * 1.8f;
            Vector3 desiredCamPos = cameraRoot.TransformPoint(targetOffset);
            Vector3 dir = desiredCamPos - headPos;
            float distance = dir.magnitude;

            if (Physics.Raycast(headPos, dir.normalized, out RaycastHit hit, distance + cameraCollisionBuffer, cameraCollisionLayers))
            {
                // Pull camera in to just before the hit
                float hitDist = hit.distance - cameraCollisionBuffer * 0.5f;
                hitDist = Mathf.Max(hitDist, 0.1f); // don't clip into player
                Vector3 pulledOffset = dir.normalized * hitDist;
                cameraRoot.localPosition = Vector3.Lerp(cameraRoot.localPosition, pulledOffset, Time.deltaTime * crouchTransitionSpeed);
            }
            else
            {
                // Return to full arm length
                cameraRoot.localPosition = Vector3.Lerp(cameraRoot.localPosition, targetOffset, Time.deltaTime * crouchTransitionSpeed);
            }
        }
        else
        {
            cameraRoot.localPosition = Vector3.Lerp(cameraRoot.localPosition, targetOffset, Time.deltaTime * crouchTransitionSpeed);
        }
    }

    /// <summary>
    /// Rotate the visual body to face the horizontal move direction while moving.
    /// Does NOT change movement math — this is cosmetic only.
    /// </summary>
    void UpdateVisualFacing()
    {
        if (proxyBody == null) return;

        Vector3 horizontalMove = new Vector3(moveInput.x, 0, moveInput.y);
        if (horizontalMove.sqrMagnitude > 0.01f)
        {
            // Get the world-space move direction (camera-relative)
            Vector3 worldMoveDir = transform.forward * moveInput.y + transform.right * moveInput.x;
            worldMoveDir.y = 0;
            if (worldMoveDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(worldMoveDir.normalized, Vector3.up);
                proxyBody.rotation = Quaternion.Slerp(proxyBody.rotation, targetRot, Time.deltaTime * visualTurnSpeed);
            }
        }
    }

    // --- Public API ---
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        ApplyLookDelta(value.Get<Vector2>());
    }

    public void ApplyLookDelta(Vector2 delta)
    {
        LastLookDelta = delta;
        yaw += delta.x * lookSensitivity;
        // Apply invertY from dev settings
        float yDelta = delta.y * lookSensitivity;
        if (DevSettings.InvertY) yDelta = -yDelta;
        pitch -= yDelta;
        pitch = Mathf.Clamp(pitch, -70f, 80f);
    }

    public void OnRun(InputValue value)
    {
        isRunning = value.isPressed;
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded && !isCrouching && !isDashing)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    public void OnCrouch(InputValue value)
    {
        isCrouching = value.isPressed;
    }

    public void OnDash(InputValue value)
    {
        if (value.isPressed && !isDashing && !isCrouching && Time.time - lastDashTime >= dashCooldown)
        {
            isDashing = true;
            dashTimer = dashDuration;
            lastDashTime = Time.time;
        }
    }

    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
        usingTouchInput = input != Vector2.zero; // touch is active when non-zero
    }

    public void TriggerJump()
    {
        if (isGrounded && !isCrouching && !isDashing)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    public void SetCrouch(bool crouching)
    {
        isCrouching = crouching;
    }

    public void TriggerDash()
    {
        if (!isDashing && !isCrouching && Time.time - lastDashTime >= dashCooldown)
        {
            isDashing = true;
            dashTimer = dashDuration;
            lastDashTime = Time.time;
        }
    }

    public void SetRun(bool running)
    {
        isRunning = running;
    }

    /// <summary>
    /// Reset yaw to 0 (facing +Z). Used by tests for deterministic direction.
    /// </summary>
    public void ResetYaw()
    {
        yaw = 0;
        pitch = 0;
    }

    /// <summary>
    /// Apply settings from DevConfig (called at startup). Used by DevConfigLoader.
    /// </summary>
    public void ApplyDevSettings(float sens, float deadzone)
    {
        lookSensitivity = sens;
        // Deadzone is stored for VirtualJoystick to read
        DevSettings.JoystickDeadzone = deadzone;
        Debug.Log($"[PlayerController] Dev settings applied: lookSensitivity={sens}, joystickDeadzone={deadzone}");
    }
}
