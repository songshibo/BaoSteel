using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PointerEnterExitUI : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
{

    public void OnPointerEnter(PointerEventData eventData)
    {
        FocusController.Instance.CameraLock(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        FocusController.Instance.CameraLock(false);
    }
}
