using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EnterExitOutline : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private string s;
    private GameObject[] targets;
    private bool flag; // targets 是否有值的标志

    public void SetTargets(string tar)
    {
        s = tar;
        flag = false;
    }

    private void GetTargets()
    {
        targets = ModelManager.Instance.SplitStringGetObjects(s);
        flag = true;
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (!flag)
        {
            GetTargets();
        }
        SelectionManager.Instance.AddAllToOutlineList(targets);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        if (!flag)
        {
            GetTargets();
        }
        SelectionManager.Instance.MoveAllFromOutlineList(targets);
    }
}
