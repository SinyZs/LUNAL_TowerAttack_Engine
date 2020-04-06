using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class PopButton : Button, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Save Data")]
    public int index = 0;

    public Action<int> OnBeginDragEvent, OnDragEvent, OnEndDragEvent;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (interactable)
        {
            // Call Event
            OnBeginDragEvent?.Invoke(index);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (interactable)
        {
            OnDragEvent?.Invoke(index);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (interactable)
        {
            OnEndDragEvent?.Invoke(index);
        }
    }
}
