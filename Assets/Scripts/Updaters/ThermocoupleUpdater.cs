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

    private string ComputeDisplayInfo()
    {
        string mergedName = Util.MergeThermocoupleName(lastHitted.name);
        // 目前是计算，可以考虑从数据库里读取
        float angle = Util.ComputeThermocoupleAngle(lastHitted.transform.position);
        float height = (float)Math.Round(lastHitted.transform.position.y, 2);
        // 获取热电偶的温度
        infoText.text = "Temperature:\n" + GetTempByName(mergedName) + "°C\n" + "Angle:" + angle.ToString() + "° " + "Height:" + height.ToString() + "m";
        return mergedName;
    }

    public void DisplayHittedThermocoupleInfo(GameObject hittedThermocouple)
    {
        lastHitted = hittedThermocouple;

        int count = hittedThermocouple.name.Split('_')[0].Split('-').Length;
        string mergedName = ComputeDisplayInfo();
        IDText.text = mergedName;
        // 根据几点热电偶显示(A:?)的后缀
        IDText.text += count > 1 ? "(A:" + Util.endChar[count - 1] + ")" : "";

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
        string temperatures = obj.transform.Find("temperature").GetComponent<Text>().text.TrimEnd(' ');
        return temperatures.Replace(" ", "°C-");
    }

    public void UpdateUIPanel(bool reset = false)
    {
        // 不是标准模式，为true
        if (reset) //如果当前selectionType不是standard，则清除outline效果
        {
            SelectionManager.Instance.ClearCertainLayerContents(0);
            lastHitted = null;
        }
        //当是SelectionType.Standard的时候且有选择热电偶(即OutlineLayers不为空)
        thermocouplePanel.SetActive(!reset && (SelectionManager.Instance.GetOutlineBuilder().OutlineLayers.GetOrAddLayer(0).Count != 0));
        if (lastHitted != null) // 当选中了热电偶时，才会计算对应的UI坐标
        {
            thermocouplePanel.transform.localPosition = Util.ComputeUIPosition(Camera.main.WorldToScreenPoint(lastHitted.transform.position));
        }
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
            try
            {
                item.Value.transform.Find("temperature").GetComponent<Text>().text += name_temperature[item.Key] + " ";
            }
            catch (KeyNotFoundException)
            {
                Debug.LogError(item.Key + "没有找到");
                Debug.Log(DicStringString(name_temperature));
                Debug.Log(DicStringGameobject(name_gameobject));
            }

            if (item.Value.name.StartsWith(item.Key))
            {
                float temp = float.Parse(name_temperature[item.Key].Split(' ')[0]);
                if (temp > 50)
                {
                    item.Value.GetComponent<Image>().color = Color.red;
                }
            }
        }
        // update UI panel for the selected thermocouple
        if (lastHitted != null)
        {
            string mergedName = Util.MergeThermocoupleName(lastHitted.name);
            ComputeDisplayInfo();
        }
        return true;
    }
    private string DicStringString(Dictionary<string, string> dic)
    {
        string result = "";
        foreach (var item in dic)
        {
            result += item.Key + ": " + item.Value + ", ";
        }

        return result.Substring(0, result.Length - 2);
    }
    private string DicStringGameobject(Dictionary<string, GameObject> dic)
    {
        string result = "";
        foreach (var item in dic)
        {
            result += item.Key + ": " + item.Value.name + ", ";
        }

        return result.Substring(0, result.Length - 2);
    }
}
