using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Touch button. Image raycastTarget=true. Calls real controller methods.
/// Moved to its own file so Unity can resolve its script GUID independently.
/// </summary>
public class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public System.Action OnPressed;
    public System.Action OnReleased;
    [SerializeField] private bool isPressed;

    void Awake()
    {
        var img = GetComponent<Image>();
        if (img != null) img.raycastTarget = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        OnPressed?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        OnReleased?.Invoke();
    }
}
