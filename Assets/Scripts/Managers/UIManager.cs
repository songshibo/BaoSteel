using UnityEngine;
using Michsky.UI.ModernUIPack;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEditor;
using System;
using XCharts;
using TMPro;

public class UIManager : MonoSingleton<UIManager>
{
    public CustomDropdown clipDropDown;
    public DropdownMultiSelect layerDropDown;
    public CustomDropdown renderType;
    public HorizontalSelector heatMapGradientSelector;
    public HorizontalSelector heatLoadGradientSelector;

    private GameObject EnterExitTuyereInfo;
    private GameObject EnterExitInfo;
    private Vector2 ThermocouplePanel_Width_Height;
    private ModalWindowManager heatmapWindowManager;
    private ModalWindowManager heatloadWindowManager;
    private ModalWindowManager TuyereWindowManager;
    private ModalWindowManager optionWindowManager;

    private FocusController focus; // 控制相机的跳转
    private float offset = 8.0f; // 相机与热电偶的距离

    public void Initialize(string[] configClip, string[] configShowPart)
    {
        // 获取控制相机跳转的组件
        focus = GameObject.FindObjectOfType(typeof(FocusController)) as FocusController;

        // * Initialize clip dropdown
        string[] clipConfig = configClip[0].Split(' ');
        string spritePath = "Textures/Border/Circles/";
        Sprite clipIcon = Resources.Load<Sprite>(spritePath + "Circle Outline - Stroke 20px");
        for (int i = 0; i < clipConfig.Length; i++)
        {
            clipDropDown.CreateNewItem("剖角:" + clipConfig[i], clipIcon);
        }
        clipDropDown.dropdownEvent.AddListener(ClipItemEvent);
        clipDropDown.SetupDropdown();


        // * Initialize layer dropdown list
        List<string> englishName = new List<string>();
        // layer dropdown initialize
        foreach (string row in configShowPart)
        {
            string[] config = row.Split(':');
            DropdownMultiSelect.Item item = new DropdownMultiSelect.Item();
            englishName.Add(config[0].Split('?')[0].Split('*')[0]);
            item.itemName = config[0].Split('?')[0].Split('*')[1];
            layerDropDown.dropdownItems.Add(item);
            //layerDropDown.SetItemTitle(config[0]);
            //layerDropDown.CreateNewItem();
        }
        layerDropDown.SetupDropdown();

        for (int i = 0; i < configShowPart.Length; i++)
        {
            string[] config = configShowPart[i].Split(':');
            GameObject obj = layerDropDown.transform.Find("Content/Item List/Scroll Area/List/dropdown" + i.ToString()).gameObject;

            GameObject target = GameObject.Find(englishName[i] + "_parent");
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

        // * Other UI initialization
        EnterExitInfo = GameObject.Find("EnterExitInfo");
        EnterExitInfo.SetActive(false);
        Invoke("GenerateThermoUI", 3);

        EnterExitTuyereInfo = GameObject.Find("EnterExitTuyereInfo");
        //EnterExitTuyereInfo.SetActive(false);
        Invoke("GenerateTuyereUI", 3);

        // 热力图gradient的mode设置
        heatMapGradientSelector.selectorEvent.AddListener((int value) => HeatmapDatabaseUpdater.Instance.SwitchGradientMode(value));
        // heatMapGradientSelector.selectorEvent.AddListener((int value) => HeatmapUpdater.Instance.SwitchGradientMode(value));
        // 热负荷的gradient的mode设置
        heatLoadGradientSelector.selectorEvent.AddListener((int value) => HeatLoadUpdater.Instance.SwitchHeatLoad(value));
        // RenderType
        heatmapWindowManager = GameObject.Find("HeatMapWindow").GetComponent<ModalWindowManager>();
        heatloadWindowManager = GameObject.Find("HeatLoadWindow").GetComponent<ModalWindowManager>();
        TuyereWindowManager = GameObject.Find("TuyereWindow").GetComponent<ModalWindowManager>();
        renderType.CreateNewItem("标准模式", clipIcon);
        renderType.CreateNewItem("热力图模式", clipIcon);
        renderType.CreateNewItem("热负荷模式", clipIcon);
        renderType.dropdownEvent.AddListener(RenderTypeEvent);
        renderType.SetupDropdown();
        // Options
        optionWindowManager = GameObject.Find("OptionWindow").GetComponent<ModalWindowManager>();
    }

    public void ShowTuyereUI()
    {
        TuyereWindowManager.OpenWindow();
    }

    private void RenderTypeEvent(int i)
    {
        Resources.Load<Material>("ClippingMaterials/heatmap").SetFloat("_RenderType", i);
        if (i == 1) // heat map
        {
            heatmapWindowManager.OpenWindow();
            heatloadWindowManager.CloseWindow();
            SelectionManager.Instance.selectionType = SelectionManager.SelectionType.heatmap;
        }
        else if (i == 2) // heat load
        {
            heatmapWindowManager.CloseWindow();
            heatloadWindowManager.OpenWindow();
            SelectionManager.Instance.selectionType = SelectionManager.SelectionType.heatload;  // 鼠标进入某段高度，GUI 显示某段高度的热负荷值
        }
        else //standard
        {
            heatmapWindowManager.CloseWindow();
            heatloadWindowManager.CloseWindow();
            SelectionManager.Instance.selectionType = SelectionManager.SelectionType.standard;
        }
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
            // show inside-stove panel
            InsideStoveManager.Instance.ControlPanel(angle);
            ResidualUpdater.Instance.SwitchProfile(true);
            BatchLayerUpdater.Instance.Rotate(angle);
        }
        else
        {
            CullingController.Instance.ResetMaterialProperties();
            FindObjectOfType<FocusController>().FaceClipSurface();
            // hide inside-stove panel
            InsideStoveManager.Instance.ControlPanel(0);
            ResidualUpdater.Instance.SwitchProfile(false);
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

    private void GenerateTuyereUI()
    {
        GameObject prefab = (GameObject)Resources.Load("Prefabs/TuyereUISingle");
        GameObject root = GameObject.Find("TuyereUI/TuyereUIBackground");
        TuyereUpdater.Instance.areaRatio = root.transform.Find("ratio").gameObject;
        float radius = root.GetComponent<RectTransform>().sizeDelta.x / 2;

        GameObject[] tuyeres = GameObject.FindGameObjectsWithTag("tuyere");
        foreach (GameObject tuyere in tuyeres)
        {
            string name = tuyere.name;
            float angle = (-1) * tuyere.transform.localEulerAngles.y; // 这里的负号是保证风口生成顺序与炉缸俯视图一致。
            float x = radius * Mathf.Cos(Mathf.Deg2Rad * angle);
            float y = radius * Mathf.Sin(Mathf.Deg2Rad * angle);

            GameObject UIobj = Instantiate(prefab, root.transform);
            UIobj.name = name;
            UIobj.transform.localEulerAngles = new Vector3(0, 0, angle);
            UIobj.transform.localPosition = new Vector2(x, y);
            UIobj.GetComponent<Button>().onClick.AddListener(delegate () { TuyereUIOnClick(tuyere); });
            TuyereUpdater.Instance.tuyereUISingles.Add(UIobj);

            EventTrigger eventTrigger = UIobj.AddComponent<EventTrigger>();
            // Point enter event
            EventTrigger.Entry pointerEnter = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            pointerEnter.callback.AddListener((e) => ShowTuyereInfoDetail(UIobj, true));
            eventTrigger.triggers.Add(pointerEnter);
            // Point exit event
            EventTrigger.Entry pointerExit = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerExit
            };
            pointerExit.callback.AddListener((e) => ShowTuyereInfoDetail(UIobj, false));
            eventTrigger.triggers.Add(pointerExit);
        }
    }

    public void TuyereUIOnClick(GameObject tuyere)
    {
        // 相机跳转
        focus.LocateThermoCouple(tuyere.transform.position, offset);

    }

    private void GenerateThermoUI()
    {
        GameObject prefab = (GameObject)Resources.Load("Prefabs/thermocoupleButton");
        float translate = prefab.GetComponent<RectTransform>().sizeDelta.x / 2;  // 热电偶的平移量
        GameObject root = GameObject.Find("ThermoTemperaturePanel");

        // 修改热电偶面板的高度与 viewport 相同，减去20是因为水平滑动杆的高度
        float parent_width = root.transform.parent.GetComponent<RectTransform>().rect.width;
        float parent_height = root.transform.parent.GetComponent<RectTransform>().rect.height;
        root.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(parent_width - 20, parent_height - 20);

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
            float x = angle * xRatio + translate - ThermocouplePanel_Width_Height.x / 2;
            float y = position.y * yRatio - ThermocouplePanel_Width_Height.y / 2;
            UIobj.GetComponent<RectTransform>().localPosition = new Vector2(x, y);
            UIobj.name = Util.MergeThermocoupleName(thermo.name);
            string[] names = UIobj.name.Split('-');
            foreach (string name in names)
            {
                ThermocoupleUpdater.Instance.name_gameobject.Add(name, UIobj);
            }

            UIobj.transform.Find("height").GetComponent<Text>().text = position.y.ToString("0.###") + "m";
            UIobj.transform.Find("angle").GetComponent<Text>().text = angle.ToString("0") + "°";
            UIobj.transform.Find("temperature").GetComponent<TMP_Text>().text = "-";
            UIobj.GetComponent<Button>().onClick.AddListener(delegate () { ThermoUIOnClick(thermo, UIobj); });

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

    private void ThermoUIOnClick(GameObject thermo, GameObject UIobj)
    {
        // 相机跳转
        focus.LocateThermoCouple(thermo.transform.position, offset);

        // 之前高亮的热电偶取消高亮，选中的热电偶高亮
        SelectionManager.Instance.ClearCertainLayerContents(0);
        ThermocoupleUpdater.Instance.DisplayHittedThermocoupleInfo(thermo);
    }

    private void ShowInfoDetail(GameObject obj, bool flag)
    {
        if (flag)
        {
            EnterExitInfo.SetActive(true);
            string content = "编号：" + obj.name + "\n温度：" + obj.transform.Find("temperature").GetComponent<TMP_Text>().text +
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

    private void ShowTuyereInfoDetail(GameObject obj, bool flag)
    {
        if (flag)
        {
            //EnterExitTuyereInfo.SetActive(true);
            string content = obj.transform.Find("info").GetComponent<TMP_Text>().text;
            EnterExitTuyereInfo.GetComponent<TMP_Text>().text = content;
        }
        else
        {
            //EnterExitTuyereInfo.SetActive(false);
            //EnterExitTuyereInfo.GetComponent<TMP_Text>().text = "";
        }
    }
}