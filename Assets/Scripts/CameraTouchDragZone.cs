using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Camera drag zone. Passes raw pixel delta to PlayerController.ApplyLookDelta.
/// Moved to its own file so Unity can resolve its script GUID independently.
/// </summary>
public class CameraTouchDragZone : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public PlayerController playerController;

    private Vector2 lastPos;

    void Awake()
    {
        var img = GetComponent<Image>();
        if (img != null) img.raycastTarget = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        lastPos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta = eventData.position - lastPos;
        lastPos = eventData.position;
        if (playerController != null)
        {
            playerController.ApplyLookDelta(delta);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }
}
