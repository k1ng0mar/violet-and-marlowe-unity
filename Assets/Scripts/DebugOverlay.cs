using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Debug overlay: single-line strip at the very top edge.
/// Reads from the SAME PlayerController fields the game consumes.
/// </summary>
public class DebugOverlay : MonoBehaviour
{
    public PlayerController player;
    public int fontSize = 20;

    private Text text;

    void Awake()
    {
        // Background strip at very top
        var bgGo = new GameObject("DebugBG");
        bgGo.transform.SetParent(transform, false);
        var bgImg = bgGo.AddComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0.6f);
        var bgRect = bgGo.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 1);
        bgRect.anchorMax = new Vector2(1, 1);
        bgRect.pivot = new Vector2(0.5f, 1);
        bgRect.sizeDelta = new Vector2(0, 32);
        bgRect.anchoredPosition = new Vector2(0, 0);
        bgGo.transform.SetAsFirstSibling();

        // Text on top of bg
        var go = new GameObject("DebugText");
        go.transform.SetParent(transform, false);
        text = go.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.UpperCenter;
        text.color = new Color(1, 1, 0, 0.95f);
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.sizeDelta = new Vector2(0, 30);
        rect.anchoredPosition = new Vector2(0, -1);
    }

    void Update()
    {
        if (player == null)
        {
            var p = GameObject.Find("Player");
            if (p != null)
                player = p.GetComponent<PlayerController>();
            if (player == null) return;
        }

        text.text = $"STICK=({player.MoveInput.x:F2},{player.MoveInput.y:F2}) LOOK=({player.LastLookDelta.x:F0},{player.LastLookDelta.y:F0}) J={player.IsJumping} C={player.IsCrouching} D={player.IsDashing} g={player.IsGrounded} vy={player.VelocityY:F2} yaw={player.Yaw:F1}";
    }
}
