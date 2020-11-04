using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;

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
    //[SerializeField]
    private RenderTexture texture;
    //[SerializeField]
    private Texture2D gradientTex;

    private int kernel;

    public void SwitchHeatMap()
    {
        ModalWindowManager windowManager = GameObject.Find("HeatMapWindow").GetComponent<ModalWindowManager>();
        targetMat.SetFloat("_RenderType", 1);
        windowManager.OpenWindow();
    }

    public void ApplyHeatMapProperties()
    {
        Debug.Log("热力图手动更新");
        StartCoroutine(DataServiceManager.Instance.GetHeatmap(UpdateHeatmap));
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
        texture = new RenderTexture(xRes, yRes, 24)
        {
            enableRandomWrite = true
        };
        texture.Create();

        shader.SetTexture(kernel, "Result", texture);
        shader.SetBuffer(kernel, "points", buffer);
        shader.Dispatch(kernel, xRes / 16, yRes / 16, 1);
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
        float yMax = Util.ReadModelProperty("max_height") * yAxisScaleFactor;
        buffer = new ComputeBuffer(data.Count, Marshal.SizeOf(typeof(Vector3)));
        buffer.SetData(data);

        kernel = shader.FindKernel("CSMain");
        shader.SetFloat("xRes", xRes);
        shader.SetFloat("yRes", yRes);
        shader.SetFloat("power", float.Parse(powerUI.valueText.text));
        shader.SetFloat("smoothin", float.Parse(smoothinUI.valueText.text));
        shader.SetInt("len", data.Count);
        shader.SetFloat("yHeight", yMax);
        shader.SetFloat("minTemperature", float.Parse(miniTmp.inputText.text));
        shader.SetFloat("maxTemperature", float.Parse(maxTmp.inputText.text));

        targetMat.SetFloat("yFactor", yAxisScaleFactor);
        targetMat.SetFloat("yHeight", yMax);

        GenerateHeatMap();
        buffer.Release();
        return true;
    }

    public void InitializeHeatMap()
    {
        gradientTex = customGradient.GetTexture(gradientRes, (int)float.Parse(segmentUI.valueText.text));
        targetMat.SetTexture("_CustomGradient", gradientTex);
        gradientUI.sprite = Sprite.Create(gradientTex, new Rect(0, 0, gradientTex.width, gradientTex.height), new Vector2(0.5f, 0.5f));
    }

    private void OnDestroy()
    {
        if (buffer != null)
            buffer.Dispose();
    }
}
