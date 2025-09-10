using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using TMPro;

// Ensure an Image exists so UI raycasts work
[RequireComponent(typeof(Image))]
public class PuzzleTile : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public int correctIndex;           // where this tile belongs
    public int currentIndex;           // where it is now (0..N*N-1)
    public bool isBlank;               // if true, image disabled
    public Action<PuzzleTile> onClick; // set by manager
    public Image image;                // assign in prefab
    public TMPro.TextMeshProUGUI numberText; // assign in prefab

    [Tooltip("Log pointer events for debugging missed clicks.")]
    public bool debugClicks = false;

    void Reset()
    {
        image = GetComponent<Image>();
        numberText = GetComponentInChildren<TMPro.TextMeshProUGUI>();
    }

    void Awake()
    {
        if (!image) image = GetComponent<Image>();
        // Make sure the tile's Image can receive raycasts
        if (image) image.raycastTarget = true;
        // Number text doesn't need to block clicks; let events fall through to parent
        if (numberText) numberText.raycastTarget = false;
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (debugClicks) Debug.Log($"[PuzzleTile] OnPointerClick index={currentIndex} blank={isBlank}");
        if (!isBlank) onClick?.Invoke(this);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (debugClicks) Debug.Log($"[PuzzleTile] PointerDown index={currentIndex}");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (debugClicks) Debug.Log($"[PuzzleTile] PointerUp index={currentIndex}");
    }

    // Optional fallback for world-space or unexpected UI blockers
    void OnMouseUpAsButton()
    {
        if (debugClicks) Debug.Log($"[PuzzleTile] OnMouseUpAsButton index={currentIndex}");
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            // Already handled by UI event system
            return;
        }
        if (!isBlank) onClick?.Invoke(this);
    }
}
