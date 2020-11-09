using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;

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

    [Space]
    [SerializeField]
    private Texture2D texture;
    //[SerializeField]
    private Texture2D gradientTex;
    private float yMax;

    private bool openHeatLoad = false;  // 是否打开了热负荷
    private bool openTemp = false;  // 鼠标是否进入炉体，是否显示温度
    private float height = 30f;  // 射线与物体的交点的高度
    private int RayCastLayer { get; set; } = 1 << 9; // default layer: highlight
    private List<string> objs;  // 需要监听的物体，炉体的五个部分
    private List<Vector3> part_cooling_plate = new List<Vector3>(); // 最小高度、最大高度、总温度
    private List<Vector3> part_cooling_cross = new List<Vector3>(); // 最小高度、最大高度、总温度

    public void ApplyHeatLoadProperties()
    {
        Debug.Log("热负荷手动更新");
        StartCoroutine(DataServiceManager.Instance.GetHeatmap(UpdateHeatLoad));
    }

    public void SwitchGradientMode(int i)
    {
        customGradient.blendMode = (i == 0) ? CustomGradient.BlendMode.Linear : CustomGradient.BlendMode.Discrete;
        gradientTex = customGradient.GetTexture(gradientRes, (int)float.Parse(segmentUI.valueText.text));
        targetMat.SetTexture("_HeatLoadGradient", gradientTex);
        gradientUI.sprite = Sprite.Create(gradientTex, new Rect(0, 0, gradientTex.width, gradientTex.height), new Vector2(0.5f, 0.5f));
        StartCoroutine(DataServiceManager.Instance.GetHeatmap(UpdateHeatLoad));
    }

    private Color GetColor(float position)
    {
        float realposition = position * yMax / xRes;
        float temp = 0;
        foreach (Vector3 item in part_cooling_plate)
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
    public void ShowHeatLoad(bool flag)
    {
        openHeatLoad = flag;
    }

    void OnGUI()
    {
        if (openTemp)
        {
            float temp = 0;
            foreach (Vector3 item in part_cooling_plate)
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

    public bool UpdateHeatLoad(string content)
    {
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
        part_cooling_plate.Add(new Vector3(35f, 40f, 30));
        GenerateHeatLoad();
        return true;
    }

    public void InitializeHeatLoad()
    {
        yMax = Util.ReadModelProperty("max_height");
        gradientTex = customGradient.GetTexture(gradientRes, (int)float.Parse(segmentUI.valueText.text));
        targetMat.SetTexture("_HeatLoadGradient", gradientTex);
        gradientUI.sprite = Sprite.Create(gradientTex, new Rect(0, 0, gradientTex.width, gradientTex.height), new Vector2(0.5f, 0.5f));
    }
}
