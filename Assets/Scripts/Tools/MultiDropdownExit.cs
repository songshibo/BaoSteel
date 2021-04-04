using UnityEngine;
using UnityEngine.EventSystems;
using Michsky.UI.ModernUIPack;

public class MultiDropdownExit : MonoBehaviour, IPointerExitHandler
{
    public DropdownMultiSelect dropdown;
    public void OnPointerExit(PointerEventData eventData)
    {
        if (dropdown.outOnPointerExit == true)
        {
            if (dropdown.isOn == true)
            {
                dropdown.Animate();
                dropdown.isOn = false;
            }
        }
    }
}
