using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HeistPlayModeTests
{
    private GameObject player;
    private PlayerController controller;
    private HeistManager heistManager;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        SceneManager.LoadScene("WalkTheBlock", LoadSceneMode.Single);
        yield return null;
        yield return new WaitForSeconds(0.5f);

        player = GameObject.Find("Player");
        controller = player.GetComponent<PlayerController>();
        heistManager = Object.FindObjectOfType<HeistManager>();

        Assert.IsNotNull(controller, "PlayerController not found");
        Assert.IsNotNull(heistManager, "HeistManager not found");
    }

    [UnityTearDown]
    public IEnumerator UnityTearDown()
    {
        yield return null;
    }

    // === 1. State machine transitions ===
    [UnityTest]
    public IEnumerator Heist_StateTransitionsInOrder()
    {
        Assert.AreEqual(HeistManager.HeistState.NOT_STARTED, heistManager.currentState, "Should start at NOT_STARTED");

        // Enter bank
        heistManager.OnEnteredBank();
        Assert.AreEqual(HeistManager.HeistState.ENTERED_BANK, heistManager.currentState, "Should be ENTERED_BANK");

        // Reach vault
        heistManager.OnVaultReached();
        Assert.AreEqual(HeistManager.HeistState.VAULT_REACHED, heistManager.currentState, "Should be VAULT_REACHED");

        // Secure loot (need a CarryItem)
        var sack = GameObject.Find("CarrotSack");
        Assert.IsNotNull(sack, "CarrotSack not found");
        var carryItem = sack.GetComponent<CarryItem>();
        Assert.IsNotNull(carryItem, "CarryItem not found on CarrotSack");

        heistManager.OnLootSecured(carryItem);
        Assert.AreEqual(HeistManager.HeistState.LOOT_SECURED, heistManager.currentState, "Should be LOOT_SECURED");

        // Extraction reached (first time = EXTRACTING)
        heistManager.OnExtractionReached();
        Assert.AreEqual(HeistManager.HeistState.EXTRACTING, heistManager.currentState, "Should be EXTRACTING");

        // Extraction reached again (second time = SUCCESS)
        heistManager.OnExtractionReached();
        Assert.AreEqual(HeistManager.HeistState.SUCCESS, heistManager.currentState, "Should be SUCCESS");

        Debug.Log("[PASS] Heist: state transitions NOT_STARTED → ENTERED_BANK → VAULT_REACHED → LOOT_SECURED → EXTRACTING → SUCCESS");
        yield break;
    }

    // === 2. Objective text per state ===
    [UnityTest]
    public IEnumerator Heist_ObjectiveTextUpdatesPerState()
    {
        var objectiveText = GameObject.Find("ObjectiveText");
        Assert.IsNotNull(objectiveText, "ObjectiveText not found");
        var text = objectiveText.GetComponent<Text>();
        Assert.IsNotNull(text, "Text component not found on ObjectiveText");

        // NOT_STARTED
        Assert.AreEqual("Get inside the bank", text.text, "NOT_STARTED objective text");

        // ENTERED_BANK
        heistManager.OnEnteredBank();
        Assert.AreEqual("Reach the vault", text.text, "ENTERED_BANK objective text");

        // VAULT_REACHED
        heistManager.OnVaultReached();
        Assert.AreEqual("Grab the carrots", text.text, "VAULT_REACHED objective text");

        // LOOT_SECURED
        var sack = GameObject.Find("CarrotSack");
        var carryItem = sack.GetComponent<CarryItem>();
        heistManager.OnLootSecured(carryItem);
        Assert.AreEqual("Carry them to extraction!", text.text, "LOOT_SECURED objective text");

        // SUCCESS
        heistManager.OnExtractionReached();
        heistManager.OnExtractionReached();
        Assert.AreEqual("HEIST COMPLETE", text.text, "SUCCESS objective text");

        Debug.Log("[PASS] Heist: objective text updates per state");
        yield break;
    }

    // === 3. Carry attach + slowdown ===
    [UnityTest]
    public IEnumerator Heist_CarryAttachesAndReducesSpeed()
    {
        var sack = GameObject.Find("CarrotSack");
        var carryItem = sack.GetComponent<CarryItem>();
        Assert.IsNotNull(carryItem, "CarryItem not found");

        float originalWalkSpeed = controller.walkSpeed;

        // Advance state machine to VAULT_REACHED so OnLootSecured is accepted
        heistManager.OnEnteredBank();
        heistManager.OnVaultReached();

        // Pick up + secure loot
        carryItem.PickUp(player.transform);
        Assert.IsTrue(carryItem.IsCarried, "Sack should be carried after PickUp");

        // Check it's a child of player
        Assert.AreEqual(player.transform, carryItem.transform.parent, "Sack should be parented to player");

        // Secure loot — this applies speed reduction
        heistManager.OnLootSecured(carryItem);
        Assert.Less(controller.walkSpeed, originalWalkSpeed, "Walk speed should be reduced while carrying");

        Debug.Log($"[TEST] Carry: originalWalk={originalWalkSpeed:F3}, carryWalk={controller.walkSpeed:F3}, isCarried={carryItem.IsCarried}");
        Debug.Log("[PASS] Heist: carry attaches as child + reduces walk speed");
        yield break;
    }

    // === 4. Drop detaches ===
    [UnityTest]
    public IEnumerator Heist_DropDetaches()
    {
        var sack = GameObject.Find("CarrotSack");
        var carryItem = sack.GetComponent<CarryItem>();

        // Pick up
        carryItem.PickUp(player.transform);
        Assert.IsTrue(carryItem.IsCarried, "Sack should be carried");

        // Drop
        carryItem.Drop();
        Assert.IsFalse(carryItem.IsCarried, "Sack should not be carried after Drop");

        Debug.Log("[PASS] Heist: drop detaches sack from player");
        yield break;
    }

    // === 5. Extraction WITH loot = SUCCESS ===
    [UnityTest]
    public IEnumerator Heist_ExtractionWithLootFiresSuccess()
    {
        var sack = GameObject.Find("CarrotSack");
        var carryItem = sack.GetComponent<CarryItem>();

        // Progress to EXTRACTING state
        heistManager.OnEnteredBank();
        heistManager.OnVaultReached();
        heistManager.OnLootSecured(carryItem);
        heistManager.OnExtractionReached();
        Assert.AreEqual(HeistManager.HeistState.EXTRACTING, heistManager.currentState);

        // Extraction again = SUCCESS
        heistManager.OnExtractionReached();
        Assert.AreEqual(HeistManager.HeistState.SUCCESS, heistManager.currentState, "Should be SUCCESS after extraction with loot");

        Debug.Log("[PASS] Heist: extraction WITH loot fires SUCCESS");
        yield break;
    }

    // === 6. Extraction WITHOUT loot does NOT fire SUCCESS ===
    [UnityTest]
    public IEnumerator Heist_ExtractionWithoutLootDoesNotFireSuccess()
    {
        // Stay at NOT_STARTED (no loot)
        heistManager.OnExtractionExitedWithoutLoot();
        Assert.AreNotEqual(HeistManager.HeistState.SUCCESS, heistManager.currentState, "Should NOT be SUCCESS without loot");

        // Even at ENTERED_BANK, extraction without loot does nothing
        heistManager.OnEnteredBank();
        heistManager.OnExtractionExitedWithoutLoot();
        Assert.AreNotEqual(HeistManager.HeistState.SUCCESS, heistManager.currentState, "Should NOT be SUCCESS at ENTERED_BANK without loot");

        Debug.Log("[PASS] Heist: extraction WITHOUT loot does NOT fire SUCCESS");
        yield break;
    }
}