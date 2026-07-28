using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Virtual joystick for mobile. Base Image has raycastTarget=true.
/// Applies joystickDeadzone from DevSettings. Writes to PlayerController.SetMoveInput.
/// </summary>
public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public RectTransform knob;
    public float maxRadius = 100f;
    public System.Action<Vector2> OnValueChanged;

    private RectTransform bg;
    private Vector2 knobStartPos;

    void Awake()
    {
        bg = GetComponent<RectTransform>();
        var img = GetComponent<Image>();
        if (img != null) img.raycastTarget = true;
        if (knob != null)
        {
            knobStartPos = knob.anchoredPosition;
            var knobImg = knob.GetComponent<Image>();
            if (knobImg != null) knobImg.raycastTarget = false;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (bg == null) return;
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(bg, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            Vector2 delta = localPoint - bg.rect.center;
            delta = Vector2.ClampMagnitude(delta, maxRadius);
            if (knob != null)
                knob.anchoredPosition = delta;

            Vector2 normalized = delta / maxRadius;
            // Apply deadzone
            float deadzone = DevSettings.JoystickDeadzone;
            if (normalized.magnitude < deadzone)
                normalized = Vector2.zero;

            OnValueChanged?.Invoke(normalized);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (knob != null)
            knob.anchoredPosition = knobStartPos;
        OnValueChanged?.Invoke(Vector2.zero);
    }
}

// TouchButton and CameraTouchDragZone moved to their own files
// (TouchButton.cs, CameraTouchDragZone.cs) for proper GUID serialization.

/// <summary>
/// Generates a filled-circle sprite at runtime for joystick/buttons.
/// </summary>
public static class CircleSpriteFactory
{
    private static Sprite _circle;

    public static Sprite GetCircle()
    {
        if (_circle != null) return _circle;

        int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        var pixels = new Color32[size * size];
        float center = size * 0.5f;
        float radius = center;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x + 0.5f - center;
                float dy = y + 0.5f - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist <= radius)
                    pixels[y * size + x] = new Color32(255, 255, 255, 255);
                else
                    pixels[y * size + x] = new Color32(0, 0, 0, 0);
            }
        }
        tex.SetPixels32(pixels);
        tex.Apply();
        _circle = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        return _circle;
    }
}
