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

    private Vector3 hitpoint;
    private float yMax;
    private Dictionary<string, Vector3> part_cooling_plate = new Dictionary<string, Vector3>(); // 最小高度、最大高度、总温度
    private Dictionary<string, Vector3> part_cooling_cross = new Dictionary<string, Vector3>(); // 最小高度、最大高度、总温度
    private Dictionary<string, Vector3> part = new Dictionary<string, Vector3>(); // 最小高度、最大高度、总温度
    private GameObject tuyerePanel;
    private TextMeshProUGUI temperatureText;
    private TextMeshProUGUI areaText;

    public Texture2D Heatload
    {
        get
        {
            return texture;
        }
    }

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
        CancelPanel();
        GenerateHeatLoad();

    }

    private Color GetColor(float position)
    {
        float realposition = position * yMax / xRes;
        float temp = 0;
        foreach (string key in part.Keys)
        {
            Vector3 item = part[key];
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
        Color[] colours = new Color[xRes];
        for (int i = 0; i < xRes; i++)
        {
            colours[i] = GetColor(i);
        }
        texture.SetPixels(colours);
        texture.Apply();

        targetMat.SetTexture("Heatload", texture);
    }

    private string[] GetAreaAndTempByHeight(float height)
    {
        // TODO:获取实际区域名字，需要修改为字典 区域和其对应的范围
        string[] info = new string[] { "", "" };
        foreach (string key in part.Keys)
        {
            Vector3 p = part[key];
            if (p.x <= height && p.y >= height)
            {
                info[0] = key;
                info[1] = p.z.ToString();
                break;
            }
        }
        return info;
    }

    public void ClickAndShowHeatLoadDetail(Vector3 hit)
    {
        string[] info = GetAreaAndTempByHeight(hit.y);
        if (info[0] != "")
        {
            hitpoint = hit;
            temperatureText.text = info[1] + "°C";
            areaText.text = info[0];
            tuyerePanel.transform.localPosition = Util.ComputeUIPosition(Camera.main.WorldToScreenPoint(hitpoint));
        }
    }

    internal void UpdateUIPanel(bool notHeatLoad)
    {
        tuyerePanel.SetActive(!notHeatLoad);
        tuyerePanel.transform.localPosition = Util.ComputeUIPosition(Camera.main.WorldToScreenPoint(hitpoint));
    }

    public float GetTempByHeight(float height)
    {
        float temp = 0;
        foreach (string key in part.Keys)
        {
            Vector3 item = part[key];
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
        //        part_cooling_plate.Add(item.Key, new Vector3(min_height, max_height, total));
        //    }
        //    else if (item.Key.StartsWith("small_cooling_stave"))
        //    {
        //        part_cooling_cross.Add(item.Key, new Vector3(min_height, max_height, total));
        //    }
        //    else
        //    {
        //        part_cooling_plate.Add(item.Key, new Vector3(min_height, max_height, total));
        //        part_cooling_cross.Add(item.Key, new Vector3(min_height, max_height, total));
        //    }
        //}

        part_cooling_plate.Add("area1", new Vector3(5f, 10f, 50));
        part_cooling_plate.Add("area2", new Vector3(15f, 20f, 40));
        part_cooling_plate.Add("area3", new Vector3(25f, 30f, 30));
        part_cooling_plate.Add("area4", new Vector3(40f, 44f, 20));

        part_cooling_cross.Add("area5", new Vector3(5f, 10f, 50));
        part_cooling_cross.Add("area6", new Vector3(15f, 35f, 20));
        part_cooling_cross.Add("area7", new Vector3(40f, 44f, 20));

        GenerateHeatLoad();
        ClickAndShowHeatLoadDetail(hitpoint);
        return true;
    }

    public void CancelPanel()
    {
        hitpoint = new Vector3(8.8f, 8.8f, 0);
    }

    public void InitializeHeatLoad()
    {
        yMax = Util.MAX_HEIGHT;
        gradientTex = customGradient.GetTexture(gradientRes, (int)float.Parse(segmentUI.valueText.text));
        targetMat.SetTexture("_HeatLoadGradient", gradientTex);
        gradientUI.sprite = Sprite.Create(gradientTex, new Rect(0, 0, gradientTex.width, gradientTex.height), new Vector2(0.5f, 0.5f));
        part = part_cooling_plate;

        hitpoint = new Vector3(8.8f, 8.8f, 0);
        string prefab = "HeatLoadPanel";
        tuyerePanel = Instantiate((GameObject)Resources.Load("Prefabs/" + prefab), GameObject.Find("Canvas").transform);
        tuyerePanel.transform.Find("TemperatureBackgroud/cancel").GetComponent<Button>().onClick.AddListener(CancelPanel);
        tuyerePanel.name = prefab;
        temperatureText = tuyerePanel.transform.Find("TemperatureBackgroud/Temperature").GetComponent<TextMeshProUGUI>();
        areaText = tuyerePanel.transform.Find("Position").GetComponent<TextMeshProUGUI>();
        tuyerePanel.SetActive(false);
        texture = new Texture2D(xRes, yRes);
    }
}
