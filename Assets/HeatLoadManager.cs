using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class HeatLoadManager : MonoSingleton<HeatLoadManager>
{
    

    public bool HeatLoadUpdater(string content)
    {
        Dictionary<string, Dictionary<string, float>> heatload = new Dictionary<string, Dictionary<string, float>>(); // 用来保存content中的所有信息
        List<Vector3> part = new List<Vector3>(); // 最小高度、最大高度、总温度

        JToken items = JObject.Parse(content);
        foreach (JProperty item in items)  // 解析content，存放到heatload
        {
            heatload.Add(item.Name, new Dictionary<string, float>());
            foreach (JProperty data in item.Value)
            {
                heatload[item.Name].Add(data.Name, float.Parse(data.Value.ToString()));
            }
        }

        foreach (var item in heatload) // 从heatload中取出一部分数据，最小高度、最大高度、总温度
        {
            float min_height = item.Value["min_height"];
            float max_height = item.Value["max_height"];
            float total = item.Value["total"];
            part.Add(new Vector3(min_height, max_height, total));
            print(item.Key + " " + min_height.ToString() + " " + max_height.ToString() + " " + total.ToString());
        }
        return true;
    }
}
