using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EnterExitOutline : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private string targets;

    public void SetTargets(string tar)
    {
        targets = tar;
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        SelectionManager.Instance.AddAllToOutlineList(ModelManager.Instance.SplitStringGetObjects(targets));
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        SelectionManager.Instance.MoveAllFromOutlineList(ModelManager.Instance.SplitStringGetObjects(targets));
    }
}
