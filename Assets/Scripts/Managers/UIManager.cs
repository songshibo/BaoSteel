using UnityEngine;
using Michsky.UI.ModernUIPack;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;


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
            
            // 得到该 toggle 对应的所有 GameObjects
            GameObject[] tar = SplitStringGetObjects(config[1]);
            obj.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => ShowPartItemEvent(tar, value));
            obj.AddComponent<EnterExitOutline>();
            obj.GetComponent<EnterExitOutline>().SetTargets(tar);
        }

    }

    private GameObject[] SplitStringGetObjects(string s)
    {
        List<GameObject> dst = new List<GameObject>();
        string[] infos = s.Split(' ');
        foreach (string info in infos)
        {
            string[] items = info.Trim().Split('?');

            if (items.Length == 1)
            {
                dst.AddRange(GameObject.FindGameObjectsWithTag(items[0]));
            }
            else
            {
                string[] heights = items[1].Split('-');
                float min = float.Parse(heights[0]);
                float max = float.Parse(heights[1]);
                dst.AddRange(ModelManager.Instance.FindByHeight(items[0], min, max));
            }
        }
        return dst.ToArray();
    }

    /// <summary>
    /// shouw part of model dropdown event function
    /// </summary>
    private void ShowPartItemEvent(GameObject[] targets, bool ison)
    {
        if (ison)
        {
            // 进入高亮层
            LayerManager.Instance.AddAllToHighlight(targets);
        }
        else
        {
            // 进入默认层
            LayerManager.Instance.MoveAllFromHighlight(targets);
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