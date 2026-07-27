using UnityEditor;
using UnityEngine;

/// <summary>
/// Builds the bank building as part of the scene.
/// Place at (30, 0, 0) — east side, clear of spawn plaza (8-unit radius)
/// and clear of movement-tests' forward path (origin +Z).
///
/// Layout:
///   [Exterior walls with doorway] → [Interior room] → [Vault room at back] → [Vault door]
///   Carrot sack sits in the vault.
///
/// Color law: grey institutional walls (#6E6E73), warm floor (#F2762E family).
/// All geometry has colliders.
/// </summary>
public static class BankBuilder
{
    private static readonly Color GREY = new Color(0.431f, 0.431f, 0.451f); // #6E6E73
    private static readonly Color WARM = new Color(0.949f, 0.463f, 0.180f); // #F2762E

    public static void BuildBank(GameObject cityBlock, Material warmMat, Material greyMat)
    {
        var bankRoot = new GameObject("Bank");
        bankRoot.transform.SetParent(cityBlock.transform);
        bankRoot.transform.position = new Vector3(30, 0, 0);

        // === Exterior walls (grey, with doorway) ===
        // Front wall (facing west, toward the street)
        var frontWall = CreateWall(bankRoot.transform, new Vector3(0, 2.5f, -5f), new Vector3(8, 5, 0.5f), greyMat, "Bank_FrontWall");
        // Back wall
        var backWall = CreateWall(bankRoot.transform, new Vector3(0, 2.5f, 5f), new Vector3(8, 5, 0.5f), greyMat, "Bank_BackWall");
        // Left wall
        var leftWall = CreateWall(bankRoot.transform, new Vector3(-4f, 2.5f, 0f), new Vector3(0.5f, 5, 10.5f), greyMat, "Bank_LeftWall");
        // Right wall
        var rightWall = CreateWall(bankRoot.transform, new Vector3(4f, 2.5f, 0f), new Vector3(0.5f, 5, 10.5f), greyMat, "Bank_RightWall");

        // === Doorway (gap in front wall, no geometry) ===
        // Front wall is 8 wide, doorway is 3 wide centered at x=0
        // The wall cube already has a gap since we don't fill it — but we used a solid cube
        // So we need to create the front wall as two pieces
        Object.DestroyImmediate(frontWall);
        CreateWall(bankRoot.transform, new Vector3(-2.75f, 2.5f, -5f), new Vector3(2.25f, 5, 0.5f), greyMat, "Bank_FrontWall_Left");
        CreateWall(bankRoot.transform, new Vector3(2.75f, 2.5f, -5f), new Vector3(2.25f, 5, 0.5f), greyMat, "Bank_FrontWall_Right");

        // === Floor (warm) ===
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Bank_Floor";
        floor.transform.SetParent(bankRoot.transform);
        floor.transform.localScale = new Vector3(7.5f, 0.1f, 10f);
        floor.transform.position = new Vector3(0, -0.05f, 0f);
        floor.GetComponent<Renderer>().material = warmMat;
        floor.GetComponent<BoxCollider>().isTrigger = false;

        // === Interior room divider (vault doorway) ===
        // Vault is at the back, behind a divider with a doorway
        var vaultDivider = CreateWall(bankRoot.transform, new Vector3(0, 2.5f, 2f), new Vector3(5, 5, 0.5f), greyMat, "Bank_VaultDivider");
        // Create doorway in divider — two pieces
        Object.DestroyImmediate(vaultDivider);
        CreateWall(bankRoot.transform, new Vector3(-1.75f, 2.5f, 2f), new Vector3(1.25f, 5, 0.5f), greyMat, "Bank_VaultDivider_Left");
        CreateWall(bankRoot.transform, new Vector3(1.75f, 2.5f, 2f), new Vector3(1.25f, 5, 0.5f), greyMat, "Bank_VaultDivider_Right");

        // === Vault room (behind divider) ===
        var vaultFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        vaultFloor.name = "Bank_VaultFloor";
        vaultFloor.transform.SetParent(bankRoot.transform);
        vaultFloor.transform.localScale = new Vector3(3f, 0.1f, 3f);
        vaultFloor.transform.position = new Vector3(0, -0.05f, 4f);
        vaultFloor.GetComponent<Renderer>().material = warmMat;
        vaultFloor.GetComponent<BoxCollider>().isTrigger = false;

        // === Vault door (rotating door, warm) ===
        var vaultDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        vaultDoor.name = "Bank_VaultDoor";
        vaultDoor.transform.SetParent(bankRoot.transform);
        vaultDoor.transform.localScale = new Vector3(2f, 4f, 0.5f);
        vaultDoor.transform.position = new Vector3(0, 2f, 3.5f);
        vaultDoor.GetComponent<Renderer>().material = greyMat;
        vaultDoor.GetComponent<BoxCollider>().isTrigger = false;

        // === Carrot sack (in vault) ===
        var sack = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sack.name = "CarrotSack";
        sack.transform.SetParent(bankRoot.transform);
        sack.transform.localScale = new Vector3(1f, 1f, 1f);
        sack.transform.position = new Vector3(0, 0.5f, 4f);
        sack.GetComponent<Renderer>().material = warmMat;
        var carryItem = sack.AddComponent<CarryItem>();
        carryItem.carryOffset = new Vector3(0, 1.5f, 0.5f);
        // Remove default collider, add trigger for pickup
        var sackCollider = sack.GetComponent<BoxCollider>();
        if (sackCollider != null)
        {
            sackCollider.isTrigger = true;
        }

        // === Trigger volumes ===
        // Bank entry trigger (at doorway)
        var entryTrigger = CreateTrigger(bankRoot.transform, new Vector3(0, 1f, -4.5f), new Vector3(3f, 2f, 0.5f), "Bank_EntryTrigger");
        var entryTriggerComp = entryTrigger.GetComponent<BankTrigger>();
        entryTriggerComp.type = BankTrigger.TriggerType.Entry;

        // Vault reached trigger (at vault doorway)
        var vaultTrigger = CreateTrigger(bankRoot.transform, new Vector3(0, 1f, 1.5f), new Vector3(3f, 2f, 0.5f), "Bank_VaultTrigger");
        var vaultTriggerComp = vaultTrigger.GetComponent<BankTrigger>();
        vaultTriggerComp.type = BankTrigger.TriggerType.Vault;

        // Extraction zone (outside bank, warm floor patch)
        var extractionZone = CreateTrigger(bankRoot.transform, new Vector3(0, 0.01f, -7f), new Vector3(4f, 0.1f, 4f), "Bank_ExtractionZone");
        var extractionTriggerComp = extractionZone.GetComponent<BankTrigger>();
        extractionTriggerComp.type = BankTrigger.TriggerType.Extraction;
        // Visual marker for extraction
        var extractionMarker = GameObject.CreatePrimitive(PrimitiveType.Quad);
        extractionMarker.name = "Bank_ExtractionMarker";
        extractionMarker.transform.SetParent(bankRoot.transform);
        extractionMarker.transform.localScale = new Vector3(4f, 1f, 4f);
        extractionMarker.transform.position = new Vector3(0, 0.02f, -7f);
        extractionMarker.transform.rotation = Quaternion.Euler(-90, 0, 0);
        extractionMarker.GetComponent<Renderer>().material = warmMat;

        // === HeistManager ===
        var heistManagerObj = new GameObject("HeistManager");
        heistManagerObj.transform.SetParent(bankRoot.transform);
        var heistManager = heistManagerObj.AddComponent<HeistManager>();
        heistManager.carrySpeedMultiplier = 0.6f;

        // Wire HeistManager to PlayerController
        var player = GameObject.Find("Player");
        if (player != null)
        {
            var pc = player.GetComponent<PlayerController>();
            if (pc != null) heistManager.playerController = pc;
        }

        // Wire triggers to HeistManager
        entryTriggerComp.heistManager = heistManager;
        vaultTriggerComp.heistManager = heistManager;
        extractionTriggerComp.heistManager = heistManager;

        Debug.Log("[BankBuilder] Bank built: exterior walls, doorway, interior, vault, vault door, carrot sack, triggers, HeistManager");
    }

    static GameObject CreateWall(Transform parent, Vector3 position, Vector3 scale, Material material, string name)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent, true);
        wall.transform.localScale = scale;
        wall.transform.position = position;
        wall.GetComponent<Renderer>().material = material;
        wall.GetComponent<BoxCollider>().isTrigger = false;
        return wall;
    }

    static GameObject CreateTrigger(Transform parent, Vector3 position, Vector3 scale, string name)
    {
        var trigger = new GameObject(name);
        trigger.transform.SetParent(parent, true);
        trigger.transform.position = position;
        trigger.transform.localScale = scale;
        var boxCollider = trigger.AddComponent<BoxCollider>();
        boxCollider.isTrigger = true;
        var bankTrigger = trigger.AddComponent<BankTrigger>();
        return trigger;
    }
}