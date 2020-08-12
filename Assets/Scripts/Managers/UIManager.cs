using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;

public class UIManager : MonoSingleton<UIManager>
{
    public CustomDropdown clipDropDown;
    public DropdownMultiSelect layerDropDown;

    public void InitializeUI()
    {
        // Initialize clip dropdown list
        string spritePath = "Textures/Border/Circles/";
        Sprite clipIcon = Resources.Load<Sprite>(spritePath + "Circle Outline - Stroke 20px");

        for (int i = 0; i < 2; i++)
            clipDropDown.CreateNewItem("Clip Angle:" + (i * 180).ToString(), clipIcon);
    }


    private void ChangeDropDownHeight(Transform dropdown, float height)
    {
        RectTransform rect = dropdown.Find("Content/Item List").GetComponent<RectTransform>();
        //rect.sizeDelta = new Vector2(0, height);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        rect.ForceUpdateRectTransforms();
    }

    private void LateUpdate()
    {
        ChangeDropDownHeight(clipDropDown.transform, clipDropDown.dropdownItems.Count * 53f + 15f);
        ChangeDropDownHeight(layerDropDown.transform, layerDropDown.dropdownItems.Count * 53f + 15f);
    }
}