using UnityEngine;
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
    private GameObject EnterExitInfo;
    private Vector2 ThermocouplePanel_Width_Height;


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

        EnterExitInfo = GameObject.Find("EnterExitInfo");
        EnterExitInfo.SetActive(false);
        Invoke("GenerateThermoUI", 3);
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
        ThermocouplePanel_Width_Height = root.GetComponent<RectTransform>().sizeDelta;

        float xRatio = root.GetComponent<RectTransform>().sizeDelta.x / 360;
        float yRatio = root.GetComponent<RectTransform>().sizeDelta.y / 55;

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
            UIobj.name = Util.MergeThermocoupleName(thermo.name);
            string[] names = UIobj.name.Split('-');
            foreach (string name in names)
            {
                ThermocoupleUpdater.Instance.name_gameobject.Add(name, UIobj);
            }

            UIobj.transform.Find("height").GetComponent<Text>().text = position.y.ToString("0.###") + "m";
            UIobj.transform.Find("angle").GetComponent<Text>().text = angle.ToString("0") + "°";
            UIobj.transform.Find("temperature").GetComponent<Text>().text = "0 0 0 0 0";
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
        print(count.ToString() + "个 UI 温度按钮");
        print(ThermocoupleUpdater.Instance.name_gameobject.Count + "实际热电偶个数，但是同一位置只生成了一个，所以会出现多对一，多个编号对应一个热电偶按钮");
    }

    private void OnClick(GameObject thermo, GameObject UIobj)
    {
        print("dianji");
    }

    private void ShowInfoDetail(GameObject obj, bool flag)
    {
        if (flag)
        {
            EnterExitInfo.SetActive(true);
            string content = "编号：" + obj.name + "\n温度：" + obj.transform.Find("temperature").GetComponent<Text>().text +
                "\n高度：" + obj.transform.Find("height").GetComponent<Text>().text + "\n角度：" + obj.transform.Find("angle").GetComponent<Text>().text;
            EnterExitInfo.GetComponent<Text>().text = content;
            float x = obj.GetComponent<RectTransform>().localPosition.x;
            float y = obj.GetComponent<RectTransform>().localPosition.y;

            if (x < ThermocouplePanel_Width_Height.x / 2)
            {
                // 左边的热电偶信息 要显示在 该热电偶的右边，因为显示在左边的话，边界热电偶信息显示就有问题。
                x += EnterExitInfo.GetComponent<RectTransform>().sizeDelta.x + 20;

            }
            EnterExitInfo.GetComponent<RectTransform>().localPosition = new Vector2(x, y);
        }
        else
        {
            EnterExitInfo.SetActive(false);
        }
    }


}