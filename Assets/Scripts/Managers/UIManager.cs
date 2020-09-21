﻿using UnityEngine;
using Michsky.UI.ModernUIPack;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEditor;
using System;

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
            item.itemName = config[0].Split('?')[0];
            layerDropDown.dropdownItems.Add(item);
            //layerDropDown.SetItemTitle(config[0]);
            //layerDropDown.CreateNewItem();
        }
        layerDropDown.SetupDropdown();

        for (int i = 0; i < configShowPart.Length; i++)
        {
            string[] config = configShowPart[i].Split(':');
            GameObject obj = layerDropDown.transform.Find("Content/Item List/Scroll Area/List/dropdown" + i.ToString()).gameObject;

            GameObject target = GameObject.Find(layerDropDown.dropdownItems[i].itemName + "_parent");
            obj.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => ShowPartItemEvent(target, value));
            EventTrigger eventTrigger = obj.AddComponent<EventTrigger>();

            // Point enter event
            EventTrigger.Entry pointerEnter = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            pointerEnter.callback.AddListener((e) => SelectionManager.Instance.AddToOutlineList(target, 1));
            eventTrigger.triggers.Add(pointerEnter);
            // Point exit event
            EventTrigger.Entry pointerExit = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerExit
            };
            pointerExit.callback.AddListener((e) => SelectionManager.Instance.MoveFromOutlineList(target, 1));
            eventTrigger.triggers.Add(pointerExit);

            //obj.AddComponent<EnterExitOutline>();
            //obj.GetComponent<EnterExitOutline>().SetTargets(config[1]);
        }


        Invoke("GenerateThermoUI", 5);
    }

    /// <summary>
    /// shouw part of model dropdown event function
    /// </summary>
    private void ShowPartItemEvent(GameObject target, bool ison)
    {
        GameObject[] children = Util.FindChildren(target);
        if (ison)
        {
            // 进入高亮层
            LayerManager.Instance.AddAllToHighlight(children);
        }
        else
        {
            // 进入默认层
            LayerManager.Instance.MoveAllFromHighlight(children);
        }
    }

    /// <summary>
    /// clip dropdown event function
    /// </summary>
    private void ClipItemEvent(int i)
    {
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

    private void GenerateThermoUI()
    {
        GameObject prefab = (GameObject)Resources.Load("Prefabs/button");
        GameObject root = GameObject.Find("TemperaturePanel");
        float xRatio = root.GetComponent<RectTransform>().sizeDelta.x / 360;
        float yRatio = root.GetComponent<RectTransform>().sizeDelta.y / 55;
        print(root.GetComponent<RectTransform>().sizeDelta.x);
        GameObject[] thermos = GameObject.FindGameObjectsWithTag("thermocouple");
        float count = 0;
        foreach (GameObject thermo in thermos)
        {
            GameObject UIobj = Instantiate(prefab, root.transform);
            Vector3 position = thermo.transform.position;
            float angle = Mathf.Atan2(position.x, position.z) * 180 / Mathf.PI;
            if (angle < 0)
            {
                angle += 360;
            }
            float x = angle * xRatio + 10;
            float y = position.y * yRatio;
            UIobj.GetComponent<RectTransform>().localPosition = new Vector2(x, y);
            UIobj.name = thermo.name;
            UIobj.transform.Find("height").GetComponent<Text>().text = position.y.ToString("0.###") + "m";
            UIobj.transform.Find("angle").GetComponent<Text>().text = angle.ToString("0") + "°";

            UIobj.GetComponent<Button>().onClick.AddListener(delegate () { OnClick(thermo, UIobj); });

            EventTrigger eventTrigger = UIobj.AddComponent<EventTrigger>();
            // Point enter event
            EventTrigger.Entry pointerEnter = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            pointerEnter.callback.AddListener((e) => ShowInfoDetail(UIobj, true));
            eventTrigger.triggers.Add(pointerEnter);
            // Point exit event
            EventTrigger.Entry pointerExit = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerExit
            };
            pointerExit.callback.AddListener((e) => ShowInfoDetail(UIobj, false));
            eventTrigger.triggers.Add(pointerExit);

            count += 1;
        }
        print(count);
    }

    private void OnClick(GameObject thermo, GameObject UIobj)
    {
        print("dianji");
    }

    private void ShowInfoDetail(GameObject obj, bool flag)
    {
        if (flag)
        {
            print("show");
        }
        else
        {
            print("hide");
        }
    }
}