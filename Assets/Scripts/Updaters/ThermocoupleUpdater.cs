using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ThermocoupleUpdater : MonoSingleton<ThermocoupleUpdater>
{
    public Dictionary<string, GameObject> name_gameobject = new Dictionary<string, GameObject>();

    //Mouse selection UI part
    private GameObject thermocouplePanel;
    private GameObject lastHitted;
    private TextMeshProUGUI infoText;
    private TextMeshProUGUI IDText;

    public void InitializeThermocouple()
    {
        thermocouplePanel = Instantiate(Resources.Load<GameObject>("Prefabs/ThermocouplePanel"), GameObject.Find("Canvas").transform);
        thermocouplePanel.SetActive(false);//hide the panel
        infoText = thermocouplePanel.transform.Find("Info").GetComponent<TextMeshProUGUI>();
        IDText = thermocouplePanel.transform.Find("ID").GetComponent<TextMeshProUGUI>();
    }

    public void DisplayHittedThermocoupleInfo(GameObject hittedThermocouple)
    {
        lastHitted = hittedThermocouple;

        //TODO : 如何获取这些数据并展示
        IDText.text = hittedThermocouple.name;
        // 目前是计算，可以考虑从数据库里读取
        float angle = (float)Math.Round(Mathf.Rad2Deg * Mathf.Atan2(hittedThermocouple.transform.position.z, hittedThermocouple.transform.position.x), 2) + 180;
        float height = (float)Math.Round(hittedThermocouple.transform.position.y, 2);
        infoText.text = "Temperature:" + "?".ToString() + "°C\n" + "Angle:" + angle.ToString() + "°\n" + "Height:" + height.ToString() + "m";

        if (!SelectionManager.Instance.GetOutlineBuilder().OutlineLayers.GetOrAddLayer(0).Contains(hittedThermocouple))
        {
            // Clear other selected object
            SelectionManager.Instance.ClearCertainLayerContents(0);
            SelectionManager.Instance.AddToOutlineList(hittedThermocouple);
        }
        else
        {
            SelectionManager.Instance.MoveFromOutlineList(hittedThermocouple);
        }
    }

    public string GetTempByName(string name)
    {
        GameObject obj = name_gameobject[name.Split('-')[0]];
        return obj.transform.Find("temperature").GetComponent<Text>().text;
    }

    public void UpdateUIPanel(bool reset = false)
    {
        if (reset) //如果当前selectionType不是standard，则清除outline效果
        {
            SelectionManager.Instance.ClearCertainLayerContents(0);
            lastHitted = null;
        }
        //当是SelectionType.Standard的时候且有选择热电偶(即OutlineLayers不为空)
        thermocouplePanel.SetActive(!reset && (SelectionManager.Instance.GetOutlineBuilder().OutlineLayers.GetOrAddLayer(0).Count != 0));
        if (lastHitted != null) // 当选中了热电偶时，才会计算对应的UI坐标
            thermocouplePanel.transform.localPosition = Util.ComputeUIPosition(Camera.main.WorldToScreenPoint(lastHitted.transform.position));
    }

    public bool UpdateThermocoupleData(string content)
    {
        content = content.Substring(1, content.Length - 2); // 去掉两边的大括号
        string[] str = content.Split(',');
        Dictionary<string, string> name_temperature = new Dictionary<string, string>();
        char[] MyChar = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' };
        foreach (string substr in str)
        {
            string[] single = substr.Split(':');
            string name = single[0].Substring(1, single[0].Length - 2).TrimEnd(MyChar); // 去掉双引号和末尾的字母
            string temperature = single[1];
            if (name_temperature.ContainsKey(name))
            {
                name_temperature[name] += " " + temperature;
            }
            else
            {
                name_temperature.Add(name, temperature);
            }


        }
        foreach (var item in name_gameobject)
        {
            item.Value.transform.Find("temperature").GetComponent<Text>().text = "";
            item.Value.GetComponent<Image>().color = Color.green;
        }
        foreach (var item in name_gameobject)
        {
            item.Value.transform.Find("temperature").GetComponent<Text>().text += name_temperature[item.Key] + " ";
            if (item.Value.name.StartsWith(item.Key))
            {
                float temp = float.Parse(name_temperature[item.Key].Split(' ')[0]);
                if (temp > 50)
                {
                    item.Value.GetComponent<Image>().color = Color.red;
                }
            }
        }
        return true;
    }
}
