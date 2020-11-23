using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using System;
using TMPro;

public class HeatLoadUpdater : MonoSingleton<HeatLoadUpdater>
{
    public int xRes, yRes; // yRes = yAxisScaleFactor * xRes
    [Space(20)]
    public SliderManager powerUI;
    public SliderManager smoothinUI;
    public SliderManager yAxisScaleFactorUI;
    public SliderManager segmentUI;
    public CustomInputField miniTmp;
    public CustomInputField maxTmp;
    public Image gradientUI;
    public CustomGradient customGradient = new CustomGradient();
    public int gradientRes = 64;
    [Space]
    public Material targetMat;
    public Camera c;
    [Space]
    [SerializeField]
    private Texture2D texture;
    //[SerializeField]
    private Texture2D gradientTex;

    private Vector3 hitpoint = new Vector3(0, 0, 0);
    private float yMax;
    private List<Vector3> part_cooling_plate = new List<Vector3>(); // 最小高度、最大高度、总温度
    private List<Vector3> part_cooling_cross = new List<Vector3>(); // 最小高度、最大高度、总温度
    private List<Vector3> part = new List<Vector3>(); // 最小高度、最大高度、总温度
    private GameObject tuyerePanel;
    private TextMeshProUGUI temperatureText;
    private TextMeshProUGUI areaText;

    public void ApplyHeatLoadProperties()
    {
        Debug.Log("热负荷手动更新");
        GenerateHeatLoad();
    }

    public void SwitchHeatLoad(int i)
    {
        if (i == 0)
        {
            part = part_cooling_plate;
        }
        else if (i == 1)
        {
            part = part_cooling_cross;
        }
        GenerateHeatLoad();
        
    }

    private Color GetColor(float position)
    {
        float realposition = position * yMax / xRes;
        float temp = 0;
        foreach (Vector3 item in part)
        {
            if (realposition >= item.x && realposition <= item.y)
            {
                temp = item.z;
                break;
            }
        }
        float a = temp / float.Parse(maxTmp.inputText.text);
        return new Color(a, a, a);
    }

    private void GenerateHeatLoad()
    {
        texture = new Texture2D(xRes, yRes);

        Color[] colours = new Color[xRes];
        for (int i = 0; i < xRes; i++)
        {
            colours[i] = GetColor(i);
        }
        texture.SetPixels(colours);
        texture.Apply();

        targetMat.SetTexture("Heatload", texture);
    }

    private string GetAreaByHeight(float height)
    {
        // TODO:获取实际区域名字，需要修改为字典 区域和其对应的范围
        // 第一次打开热负荷 不应该显示 信息面板
        // 非热负荷区域 不应该显示信息面板
        string area = "";
        foreach (var p in part)
        {
            if (p.x <= height && p.y >= height)
            {
                area = "area:cp10-30";
                break;
            }
        }
        return area;
    }

    public void ClickAndShowHeatLoadDetail(Vector3 hit)
    {
        hitpoint = hit;
        float temp = GetTempByHeight(hitpoint.y);
        string area = GetAreaByHeight(hitpoint.y);
        temperatureText.text = temp.ToString() + "°C";
        areaText.text = area;
        tuyerePanel.transform.localPosition = Util.ComputeUIPosition(Camera.main.WorldToScreenPoint(hitpoint));
    }

    internal void UpdateUIPanel(bool notHeatLoad)
    {
        tuyerePanel.SetActive(!notHeatLoad);
        tuyerePanel.transform.localPosition = Util.ComputeUIPosition(Camera.main.WorldToScreenPoint(hitpoint));
    }

    public float GetTempByHeight(float height)
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
        return temp;
    }

    public bool UpdateHeatLoad(string content)
    {
        part_cooling_plate.Clear();
        part_cooling_cross.Clear();
        //Dictionary<string, Dictionary<string, float>> heatload = new Dictionary<string, Dictionary<string, float>>(); // 用来保存content中的所有信息

        //JToken items = JObject.Parse(content);
        //foreach (JProperty item in items)  // 解析content，存放到heatload
        //{
        //    heatload.Add(item.Name, new Dictionary<string, float>());
        //    foreach (JProperty data in item.Value)
        //    {
        //        heatload[item.Name].Add(data.Name, float.Parse(data.Value.ToString()));
        //    }
        //}

        //foreach (var item in heatload) // 从heatload中取出一部分数据，最小高度、最大高度、总温度
        //{
        //    float min_height = item.Value["min_height"];
        //    float max_height = item.Value["max_height"];
        //    float total = item.Value["total"];

        //    if (item.Key.StartsWith("CP"))
        //    {
        //        part_cooling_plate.Add(new Vector3(min_height, max_height, total));
        //    }
        //    else if (item.Key.StartsWith("small_cooling_stave"))
        //    {
        //        part_cooling_cross.Add(new Vector3(min_height, max_height, total));
        //    }
        //    else
        //    {
        //        part_cooling_plate.Add(new Vector3(min_height, max_height, total));
        //        part_cooling_cross.Add(new Vector3(min_height, max_height, total));
        //    }
        //}
        part_cooling_plate.Add(new Vector3(5f, 10f, 50));
        part_cooling_plate.Add(new Vector3(15f, 20f, 40));
        part_cooling_plate.Add(new Vector3(25f, 30f, 30));
        part_cooling_plate.Add(new Vector3(40f, 44f, 20));

        part_cooling_cross.Add(new Vector3(5f, 10f, 50));
        part_cooling_cross.Add(new Vector3(15f, 35f, 20));
        part_cooling_cross.Add(new Vector3(40f, 44f, 20));

        GenerateHeatLoad();
        ClickAndShowHeatLoadDetail(hitpoint);
        return true;
    }

    public void InitializeHeatLoad()
    {
        yMax = Util.ReadModelProperty("max_height");
        gradientTex = customGradient.GetTexture(gradientRes, (int)float.Parse(segmentUI.valueText.text));
        targetMat.SetTexture("_HeatLoadGradient", gradientTex);
        gradientUI.sprite = Sprite.Create(gradientTex, new Rect(0, 0, gradientTex.width, gradientTex.height), new Vector2(0.5f, 0.5f));
        part = part_cooling_plate;

        string prefab = "TuyerePanel";
        tuyerePanel = Instantiate((GameObject)Resources.Load("Prefabs/" + prefab), GameObject.Find("Canvas").transform);
        tuyerePanel.name = prefab;
        temperatureText = tuyerePanel.transform.Find("Temperature").GetComponent<TextMeshProUGUI>();
        areaText = tuyerePanel.transform.Find("Position").GetComponent<TextMeshProUGUI>();
        tuyerePanel.SetActive(false);
    }
}
