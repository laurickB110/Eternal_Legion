using UnityEngine;
using UnityEngine.EventSystems;

public class CardHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalScale;
    private Vector3 targetScale;

    private Vector3 originalPosition;
    private Vector3 targetPosition;

    private int originalSiblingIndex;

    private float hoverScale = 1.05f;
    private float hoverOffsetY = 80f;

    private static bool isDragging = false;

    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;

        originalPosition = transform.localPosition;
        targetPosition = originalPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDragging) return;


        targetScale = originalScale * hoverScale;

        originalPosition = transform.localPosition;
        targetPosition = originalPosition + new Vector3(0, hoverOffsetY, 0);

        transform.localScale = targetScale;
        transform.localPosition = targetPosition;

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDragging) return;


        targetScale = originalScale;
        targetPosition = originalPosition;

        transform.localScale = originalScale;
        transform.localPosition = originalPosition;

    }

    // Méthodes appelées par le script de drag
    public void OnDragStart()
    {
        isDragging = true;
        targetScale = originalScale;
        targetPosition = originalPosition;
    }

    public void OnDragEnd()
    {
        targetScale = originalScale;
        targetPosition = originalPosition;
        transform.localScale = originalScale;
        transform.localPosition = originalPosition;
        isDragging = false;
    }
}

