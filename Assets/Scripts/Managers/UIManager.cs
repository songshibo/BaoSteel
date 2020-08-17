﻿using UnityEngine;
using Michsky.UI.ModernUIPack;
using UnityEngine.Events;
using UnityEngine.UI;


public class UIManager : MonoSingleton<UIManager>
{
    public CustomDropdown clipDropDown;
    public DropdownMultiSelect layerDropDown;

    public void InitializeUI(string[] configClip, string[] configShowPart)
    {
        // first line : clip dropdown
        string[] clipConfig = configClip[0].Split(' ');
        // Initialize clip dropdown list
        string spritePath = "Textures/Border/Circles/";
        Sprite clipIcon = Resources.Load<Sprite>(spritePath + "Circle Outline - Stroke 20px");
        for (int i = 0; i < clipConfig.Length; i++)
        {
            clipDropDown.CreateNewItem("Clip Angle:" + clipConfig[i], clipIcon);
        }
        clipDropDown.dropdownEvent.AddListener(ClipItemEvent);
        clipDropDown.SetupDropdown();


        // layer dropdown initialize
        foreach (string row in configShowPart)
        {
            string[] config = row.Split(':');
            DropdownMultiSelect.Item item = new DropdownMultiSelect.Item();
            item.itemName = config[0];
            layerDropDown.dropdownItems.Add(item);
            //layerDropDown.SetItemTitle(config[0]);
            //layerDropDown.CreateNewItem();
        }
        layerDropDown.SetupDropdown();

        for (int i = 0; i < configShowPart.Length; i++)
        {
            string[] config = configShowPart[i].Split(':');
            GameObject obj = layerDropDown.transform.Find("Content/Item List/Scroll Area/List/dropdown" + i.ToString()).gameObject;
            obj.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => ShowPartItemEvent(config[1], value));

        }

    }

    /// <summary>
    /// shouw part of model dropdown event function
    /// </summary>
    private void ShowPartItemEvent(string s, bool ison)
    {
        string[] infos = s.Split(' ');
        foreach (string info in infos)
        {
            string item = info.Trim();

        }
    }

    /// <summary>
    /// clip dropdown event function
    /// </summary>
    private void ClipItemEvent(int i)
    {
        Debug.Log(i);
        string[] info = clipDropDown.dropdownItems[i].itemName.Split(':');
        if (info.Length == 2)
        {
            float angle = float.Parse(info[1]);
            CullingController.Instance.ClipMaterialsAtAngle(angle);
            FindObjectOfType<FocusController>().FaceClipSurface(angle);
        }
        else
        {
            CullingController.Instance.ResetMaterialProperties();
            FindObjectOfType<FocusController>().FaceClipSurface();
        }
    }

    private void ChangeDropDownHeight(Transform dropdown, float height)
    {
        RectTransform rect = dropdown.Find("Content/Item List").GetComponent<RectTransform>();
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }

    private void LateUpdate()
    {
        // dynamically change dropdown height
        ChangeDropDownHeight(clipDropDown.transform, clipDropDown.dropdownItems.Count * 53f + 15f);
        ChangeDropDownHeight(layerDropDown.transform, layerDropDown.dropdownItems.Count * 53f + 15f);
    }
}