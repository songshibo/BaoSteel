using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThermocoupleUpdater : MonoSingleton<ThermocoupleUpdater>
{
    public Dictionary<string, GameObject> name_gameobject = new Dictionary<string, GameObject>();

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
