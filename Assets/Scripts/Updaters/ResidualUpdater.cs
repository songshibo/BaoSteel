using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using System;
using TMPro;

public class ResidualUpdater : MonoSingleton<ResidualUpdater>
{
    float yRes;
    float xRes;

    private void Start()
    {
        yRes = 1024;
        xRes = 2.6f * yRes;
    }

    public bool UpdateResidual(string content)
    {
        Dictionary<string, Dictionary<string, float>> furnace_core = new Dictionary<string, Dictionary<string, float>>(); // 保存炉底的残厚信息
        Dictionary<string, Dictionary<string, float>> furnace_wall = new Dictionary<string, Dictionary<string, float>>();
        JToken items = JToken.Parse(content);
        foreach (JProperty item in items)
        {
            if (item.Name.Equals("furnace_core"))
            {
                foreach (JProperty data in item.Value)
                {
                    furnace_core.Add(data.Name, new Dictionary<string, float>());
                    foreach (JProperty info in data.Value)
                    {
                        furnace_core[data.Name].Add(info.Name, float.Parse(info.Value.ToString()));
                    }
                }
            }
            else if (item.Name.Equals("furnace_wall"))
            {
                foreach (JProperty data in item.Value)
                {
                    furnace_wall.Add(data.Name, new Dictionary<string, float>());
                    foreach (JProperty info in data.Value)
                    {
                        furnace_wall[data.Name].Add(info.Name, float.Parse(info.Value.ToString()));
                    }
                }
            }
        }



        print(content);
        return true;
    }
}
