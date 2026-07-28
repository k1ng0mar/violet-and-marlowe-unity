using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Updates the minimap pip position based on the player's world XZ position.
/// Maps world coordinates into the minimap RectTransform's local space.
/// No rotating map, no terrain — just the player dot relative to the block center.
/// </summary>
public class MinimapController : MonoBehaviour
{
    [Header("References")]
    public RectTransform minimapRect;
    public RectTransform pipRect;
    public Transform player;

    [Header("Mapping")]
    public Vector2 worldCenter = Vector2.zero; // center of the district in XZ
    public float worldScale = 0.1f; // 1 world unit = 0.1 UI units (100:1)
    public Vector2 worldSize = new Vector2(80, 80); // district size in world units

    private float minimapHalfSize;

    void Start()
    {
        if (minimapRect != null)
            minimapHalfSize = minimapRect.rect.width > 0 ? minimapRect.rect.width * 0.5f : minimapRect.sizeDelta.x * 0.5f;
    }

    void Update()
    {
        if (player == null || pipRect == null || minimapRect == null) return;

        // Convention: world +X = pip right, world +Z = pip UP (north on minimap)
        Vector2 playerXZ = new Vector2(player.position.x, player.position.z);
        Vector2 offset = playerXZ - worldCenter;
        Vector2 scaled = offset * worldScale;

        if (minimapHalfSize > 0)
        {
            float bound = minimapHalfSize - 10f;
            scaled.x = Mathf.Clamp(scaled.x, -bound, bound);
            scaled.y = Mathf.Clamp(scaled.y, -bound, bound);
        }

        pipRect.anchoredPosition = scaled;
    }

    /// <summary>
    /// Get the current pip anchored position (for tests).
    /// </summary>
    public Vector2 GetPipPosition()
    {
        if (pipRect == null) return Vector2.zero;
        return pipRect.anchoredPosition;
    }
}