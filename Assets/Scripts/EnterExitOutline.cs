using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EnterExitOutline : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private GameObject[] targets;

    public void SetTargets(GameObject[] tar)
    {
        targets = tar;
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        SelectionManager.Instance.AddAllToOutlineList(targets);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        SelectionManager.Instance.MoveAllFromOutlineList(targets);
    }
}
