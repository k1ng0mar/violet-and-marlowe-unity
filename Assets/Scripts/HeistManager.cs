using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Heist state machine: NOT_STARTED → ENTERED_BANK → VAULT_REACHED → LOOT_SECURED → EXTRACTING → SUCCESS
/// Drives objective HUD text per state.
/// </summary>
public class HeistManager : MonoBehaviour
{
    public enum HeistState
    {
        NOT_STARTED,
        ENTERED_BANK,
        VAULT_REACHED,
        LOOT_SECURED,
        EXTRACTING,
        SUCCESS
    }

    [Header("State")]
    public HeistState currentState = HeistState.NOT_STARTED;

    [Header("Objective HUD")]
    public Text objectiveText;
    public DistrictBannerController banner;

    [Header("Carry")]
    public CarryItem carriedItem;
    public float carrySpeedMultiplier = 0.6f;

    [Header("References")]
    public PlayerController playerController;

    private float originalWalkSpeed;
    private float originalRunSpeed;
    private bool speedsCaptured = false;

    void Start()
    {
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();

        UpdateObjectiveText();
    }

    /// <summary>
    /// Capture speeds lazily on first use instead of in Start(). Start ordering
    /// with DevConfigLoader is undefined — a Start-time snapshot can pick up
    /// untuned defaults and RestoreSpeed() would discard dev-configured values.
    /// </summary>
    void CaptureSpeedsIfNeeded()
    {
        if (speedsCaptured || playerController == null) return;
        originalWalkSpeed = playerController.walkSpeed;
        originalRunSpeed = playerController.runSpeed;
        speedsCaptured = true;
    }

    public void OnEnteredBank()
    {
        if (currentState == HeistState.NOT_STARTED)
        {
            currentState = HeistState.ENTERED_BANK;
            UpdateObjectiveText();
            Debug.Log("[Heist] State: ENTERED_BANK");
        }
    }

    public void OnVaultReached()
    {
        if (currentState == HeistState.ENTERED_BANK)
        {
            currentState = HeistState.VAULT_REACHED;
            UpdateObjectiveText();
            Debug.Log("[Heist] State: VAULT_REACHED");
        }
    }

    public void OnLootSecured(CarryItem item)
    {
        if (currentState == HeistState.VAULT_REACHED)
        {
            currentState = HeistState.LOOT_SECURED;
            carriedItem = item;
            ApplyCarrySpeed();
            UpdateObjectiveText();
            Debug.Log("[Heist] State: LOOT_SECURED");
        }
    }

    public void OnItemDropped()
    {
        if (currentState == HeistState.LOOT_SECURED || currentState == HeistState.EXTRACTING)
        {
            carriedItem = null;
            RestoreSpeed();
            if (currentState == HeistState.EXTRACTING)
            {
                currentState = HeistState.LOOT_SECURED;
                UpdateObjectiveText();
                Debug.Log("[Heist] Item dropped — back to LOOT_SECURED");
            }
        }
    }

    public void OnExtractionReached()
    {
        if (currentState == HeistState.LOOT_SECURED)
        {
            currentState = HeistState.EXTRACTING;
            UpdateObjectiveText();
            Debug.Log("[Heist] State: EXTRACTING");
        }
        else if (currentState == HeistState.EXTRACTING)
        {
            currentState = HeistState.SUCCESS;
            UpdateObjectiveText();
            ShowSuccessBanner();
            Debug.Log("[Heist] State: SUCCESS — HEIST COMPLETE");
        }
    }

    public void OnExtractionExitedWithoutLoot()
    {
        // Entering extraction WITHOUT loot does nothing
        if (currentState == HeistState.NOT_STARTED || currentState == HeistState.ENTERED_BANK || currentState == HeistState.VAULT_REACHED)
        {
            // No state change
        }
    }

    void ApplyCarrySpeed()
    {
        if (playerController != null)
        {
            CaptureSpeedsIfNeeded();
            playerController.walkSpeed = originalWalkSpeed * carrySpeedMultiplier;
            playerController.runSpeed = originalRunSpeed * carrySpeedMultiplier;
        }
    }

    void RestoreSpeed()
    {
        if (playerController != null)
        {
            CaptureSpeedsIfNeeded();
            playerController.walkSpeed = originalWalkSpeed;
            playerController.runSpeed = originalRunSpeed;
        }
    }

    void UpdateObjectiveText()
    {
        if (objectiveText == null) return;

        switch (currentState)
        {
            case HeistState.NOT_STARTED:
                objectiveText.text = "Get inside the bank";
                break;
            case HeistState.ENTERED_BANK:
                objectiveText.text = "Reach the vault";
                break;
            case HeistState.VAULT_REACHED:
                objectiveText.text = "Grab the carrots";
                break;
            case HeistState.LOOT_SECURED:
                objectiveText.text = "Carry them to extraction!";
                break;
            case HeistState.EXTRACTING:
                objectiveText.text = "Carry them to extraction!";
                break;
            case HeistState.SUCCESS:
                objectiveText.text = "HEIST COMPLETE";
                break;
        }
    }

    void ShowSuccessBanner()
    {
        if (banner != null)
            banner.Show();
    }

    // --- Public getters for tests ---
    public bool HasLoot => carriedItem != null;
    public bool IsCarrying => currentState == HeistState.EXTRACTING;
    public bool IsSuccess => currentState == HeistState.SUCCESS;
}