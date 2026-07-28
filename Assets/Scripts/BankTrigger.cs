using UnityEngine;

/// <summary>
/// Trigger volume for bank heist events.
/// Types: Entry (ENTERED_BANK), Vault (VAULT_REACHED), Extraction (SUCCESS/EXTRACTING).
/// Extraction requires sustained presence — a dwell timer prevents touch-and-go
/// re-entry from instantly completing the heist.
/// </summary>
public class BankTrigger : MonoBehaviour
{
    public enum TriggerType
    {
        Entry,
        Vault,
        Extraction
    }

    public TriggerType type = TriggerType.Entry;
    public HeistManager heistManager;

    [Header("Extraction")]
    [Tooltip("Seconds the player must stay inside the extraction zone with loot before the heist completes.")]
    public float extractionDwellTime = 2f;

    private float extractionDwellTimer = 0f;

    void OnTriggerEnter(Collider other)
    {
        if (heistManager == null) return;

        var player = other.GetComponent<PlayerController>();
        if (player == null) return;

        switch (type)
        {
            case TriggerType.Entry:
                heistManager.OnEnteredBank();
                break;
            case TriggerType.Vault:
                heistManager.OnVaultReached();
                break;
            case TriggerType.Extraction:
                extractionDwellTimer = 0f;
                if (!heistManager.HasLoot)
                {
                    heistManager.OnExtractionExitedWithoutLoot();
                }
                break;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (heistManager == null || type != TriggerType.Extraction) return;

        var player = other.GetComponent<PlayerController>();
        if (player == null) return;

        if (!heistManager.HasLoot)
        {
            extractionDwellTimer = 0f;
            return;
        }

        // First touch with loot: move LOOT_SECURED -> EXTRACTING and start the clock.
        if (heistManager.currentState == HeistManager.HeistState.LOOT_SECURED)
        {
            heistManager.OnExtractionReached();
            extractionDwellTimer = 0f;
        }

        if (heistManager.currentState != HeistManager.HeistState.EXTRACTING) return;

        extractionDwellTimer += Time.deltaTime;
        if (extractionDwellTimer >= extractionDwellTime)
        {
            heistManager.OnExtractionReached(); // EXTRACTING -> SUCCESS
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (type != TriggerType.Extraction) return;

        var player = other.GetComponent<PlayerController>();
        if (player == null) return;

        extractionDwellTimer = 0f;
    }
}
