using UnityEngine;

/// <summary>
/// Trigger volume for bank heist events.
/// Types: Entry (ENTERED_BANK), Vault (VAULT_REACHED), Extraction (SUCCESS/EXTRACTING).
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
                if (heistManager.HasLoot)
                {
                    heistManager.OnExtractionReached();
                }
                else
                {
                    heistManager.OnExtractionExitedWithoutLoot();
                }
                break;
        }
    }
}