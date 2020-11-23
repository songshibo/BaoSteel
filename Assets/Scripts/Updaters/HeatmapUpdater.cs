using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using TMPro;

public class HeatmapUpdater : MonoSingleton<HeatmapUpdater>
{
    public ComputeShader shader;
    private ComputeBuffer buffer;


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

    private int kernel;

    //Mouse selection UI part
    private GameObject temperaturePanel;
    private Vector3 lastPos;
    private TextMeshProUGUI temperatureText;
    private TextMeshProUGUI positionText;

    public void ApplyHeatMapProperties()
    {
        Debug.Log("热力图手动更新");
        StartCoroutine(DataServiceManager.Instance.GetHeatmap(UpdateHeatmap));
    }

    public void InvertSamplingFromRayCast(Vector3 hitPoint)
    {
        float angle = (float)Math.Round(Mathf.Rad2Deg * Mathf.Atan2(hitPoint.z, hitPoint.x), 2) + 180;
        float height = (float)Math.Round(hitPoint.y, 2);
        int x = Mathf.RoundToInt(angle / 360 * xRes);
        int y = Mathf.RoundToInt(height / Util.ReadModelProperty("max_height") * yRes);

        if (texture != null)
        {
            float temperature = texture.GetPixel(x, y).r;

            float mint = float.Parse(miniTmp.inputText.text);
            float maxt = float.Parse(maxTmp.inputText.text);

            lastPos = hitPoint;
            temperatureText.text = "";
            if (temperature == 1f)
                temperatureText.text += "≥";
            temperatureText.text += Math.Round(temperature * (maxt - mint) + mint, 2).ToString() + "°C";
            positionText.text = "Angle:" + angle.ToString() + "°\n" + "Height:" + height.ToString() + "m";
        }
        else
        {
            Debug.LogWarning("Heat map has not been generated, pls wait auto-generation or press the apply button");
        }
    }

    public void UpdateUIPanel(bool reset = false)
    {
        if (lastPos.y > 0.0f) // lastPos初始值为0，0，0；所以当且仅当选中了炉体表面的点时(即位置信息有效)才会处理后续的ui显示。这里并没有清除lastPos，因为可以在下次打开heatmap时保留上次的位置
        {
            temperaturePanel.SetActive(!reset);
            temperaturePanel.transform.localPosition = Util.ComputeUIPosition(Camera.main.WorldToScreenPoint(lastPos));
        }
    }

    //作为Listener添加给GradientMode的horizontal selector
    public void SwitchGradientMode(int i)
    {
        customGradient.blendMode = (i == 0) ? CustomGradient.BlendMode.Linear : CustomGradient.BlendMode.Discrete;
        gradientTex = customGradient.GetTexture(gradientRes, (int)float.Parse(segmentUI.valueText.text));
        targetMat.SetTexture("_CustomGradient", gradientTex);
        gradientUI.sprite = Sprite.Create(gradientTex, new Rect(0, 0, gradientTex.width, gradientTex.height), new Vector2(0.5f, 0.5f));
        StartCoroutine(DataServiceManager.Instance.GetHeatmap(UpdateHeatmap));
    }

    private void GenerateHeatMap()
    {
        RenderTexture rTex = new RenderTexture(xRes, yRes, 24)
        {
            enableRandomWrite = true
        };
        rTex.Create();

        shader.SetTexture(kernel, "Result", rTex);
        shader.SetBuffer(kernel, "points", buffer);
        shader.Dispatch(kernel, xRes / 16, yRes / 16, 1);

        texture = Util.RenderTextureToTexture2D(rTex);
        targetMat.SetTexture("Heatmap", texture);
    }

    public bool UpdateHeatmap(string jsondata)
    {
        // parameter setup
        float yAxisScaleFactor = float.Parse(yAxisScaleFactorUI.valueText.text);
        List<Vector3> data = new List<Vector3>();
        Regex regex = new Regex(@"{""angle"":(?<angle>\d*\.*\d*),""height"":(?<height>\d*\.*\d*),""temperature"":(?<temperature>\d*\.*\d*)}", RegexOptions.IgnoreCase);
        if (regex.IsMatch(jsondata))
        {
            MatchCollection matchs = regex.Matches(jsondata);
            foreach (Match match in matchs)
            {
                float angle = (float)Math.Round(double.Parse(match.Groups["angle"].Value.ToString()), 2);
                float height = (float)Math.Round(double.Parse(match.Groups["height"].Value.ToString()), 2);
                string s = match.Groups["temperature"].Value.ToString();
                double temperaturef = double.Parse(s);

                float temperature = (float)Math.Round(temperaturef, 2);
                Vector3 single_data = new Vector3(angle, height * yAxisScaleFactor, temperature);
                data.Add(single_data);
                if (angle == 0)
                {
                    data.Add(new Vector3(360, height * yAxisScaleFactor, temperature));
                }
            }
        }
        else
        {
            Debug.LogWarning("HeatMapData Not Matched");
        }

        // read max height from configurations
        float yMax = Util.ReadModelProperty("max_height");
        buffer = new ComputeBuffer(data.Count, Marshal.SizeOf(typeof(Vector3)));
        buffer.SetData(data);

        kernel = shader.FindKernel("CSMain");
        shader.SetFloat("xRes", xRes);
        shader.SetFloat("yRes", yRes);
        shader.SetFloat("power", float.Parse(powerUI.valueText.text));
        shader.SetFloat("smoothin", float.Parse(smoothinUI.valueText.text));
        shader.SetInt("len", data.Count);
        shader.SetFloat("yHeight", yMax * yAxisScaleFactor);
        shader.SetFloat("minTemperature", float.Parse(miniTmp.inputText.text));
        shader.SetFloat("maxTemperature", float.Parse(maxTmp.inputText.text));
        targetMat.SetFloat("yHeight", yMax);

        GenerateHeatMap();
        buffer.Release();

        // Update selected point
        InvertSamplingFromRayCast(lastPos);
        return true;
    }

    public void InitializeHeatMap()
    {
        gradientTex = customGradient.GetTexture(gradientRes, (int)float.Parse(segmentUI.valueText.text));
        targetMat.SetTexture("_CustomGradient", gradientTex);
        gradientUI.sprite = Sprite.Create(gradientTex, new Rect(0, 0, gradientTex.width, gradientTex.height), new Vector2(0.5f, 0.5f));

        //初始化UI部分
        temperaturePanel = Instantiate(Resources.Load<GameObject>("Prefabs/TemperaturePanel"), GameObject.Find("Canvas").transform);
        temperaturePanel.SetActive(false);//hide the panel
        temperatureText = temperaturePanel.transform.Find("Temperature").GetComponent<TextMeshProUGUI>();
        positionText = temperaturePanel.transform.Find("Position").GetComponent<TextMeshProUGUI>();
    }

    private void OnDestroy()
    {
        if (buffer != null)
            buffer.Dispose();
    }
}
