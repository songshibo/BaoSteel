using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.EventSystems;


public class HeatLoadUpdater : MonoSingleton<HeatLoadUpdater>
{
    private bool openHeatLoad = false;  // 是否打开了热负荷
    private bool openTemp = false;  // 鼠标是否进入炉体，是否显示温度
    private float height = 30f;  // 射线与物体的交点的高度
    private int RayCastLayer { get; set; } = 1 << 9; // default layer: highlight
    private List<string> objs;  // 需要监听的物体，炉体的五个部分
    private List<Vector3> part = new List<Vector3>(); // 最小高度、最大高度、总温度


    private void Start()
    {
        objs = new List<string>();
        objs.Add("hearth");
        objs.Add("bosh");
        objs.Add("waist");
        objs.Add("body");
        objs.Add("throat");
    }

    private void Update()
    {
        if (openHeatLoad)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, RayCastLayer))
            {
                if (objs.Contains(hit.transform.gameObject.name))
                {
                    height = hit.point.y;
                    openTemp = true;
                }
            }
            else
            {
                openTemp = false;
            }
        }
    }

    // 热负荷开关
    public void ShowHeatLoad()
    {
        openHeatLoad = !openHeatLoad;
    }

    void OnGUI()
    {
        if (openTemp)
        {
            float temp = 0;
            foreach (Vector3 item in part)
            {
                if (item.x < height && item.y > height)
                {
                    temp = item.z;
                    break;
                }
            }

            GUIStyle style1 = new GUIStyle();
            style1.fontSize = 30;
            style1.normal.textColor = Color.black;
            GUI.Label(new Rect(Input.mousePosition.x + 20, Screen.height - Input.mousePosition.y, 400, 50), temp.ToString("0.#") + "°C", style1);
        }
    }

    public bool UpdateHeatLoadData(string content)
    {
        Dictionary<string, Dictionary<string, float>> heatload = new Dictionary<string, Dictionary<string, float>>(); // 用来保存content中的所有信息

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
