using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using TMPro;

public class PuzzleTile : MonoBehaviour, IPointerClickHandler
{
    public int correctIndex;           // where this tile belongs
    public int currentIndex;           // where it is now (0..N*N-1)
    public bool isBlank;               // if true, image disabled
    public Action<PuzzleTile> onClick; // set by manager
    public Image image;                // assign in prefab
    public TMPro.TextMeshProUGUI numberText; // assign in prefab

    void Reset() { 
        image = GetComponent<Image>(); 
        numberText = GetComponentInChildren<TMPro.TextMeshProUGUI>();
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (!isBlank) onClick?.Invoke(this);
    }
}
