using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using TMPro;

public class HeatmapDatabaseUpdater : MonoSingleton<HeatmapDatabaseUpdater>
{
    public Material material;
    // Input
    public CustomInputField minT;
    public CustomInputField maxT;
    [Space]
    public Image gradientUI;
    public SliderManager segmentUI;
    [SerializeField]
    private int gradientRes = 64;
    [SerializeField]
    private CustomGradient customGradient = new CustomGradient();
    private Texture2D gradientTex;
    // Heat map
    [Space]
    [SerializeField]
    private Texture2D heatmap;
    private int w, h; //这些参数从数据库中读取

    // Mouse selection UI part
    private GameObject temperaturePanel; // 温度面板
    private Vector3 lastPos; // 上次点击的位置
    private TextMeshProUGUI temperatureText; // 温度面板上的温度文本
    private TextMeshProUGUI positionText; // 温度面板位置

    public void InitializeHeatmap()
    {
        // Generate Gradient Texture
        gradientTex = customGradient.GetTexture(gradientRes, (int)float.Parse(segmentUI.valueText.text));
        material.SetTexture("_CustomGradient", gradientTex);
        gradientUI.sprite = Sprite.Create(gradientTex, new Rect(0, 0, gradientTex.width, gradientTex.height), new Vector2(0.5f, 0.5f));

        UpdateHeatmap();
        material.SetFloat("yHeight", Util.MAX_HEIGHT);

        //初始化UI部分
        temperaturePanel = Instantiate(Resources.Load<GameObject>("Prefabs/TemperaturePanel"), GameObject.Find("Canvas").transform);
        temperaturePanel.SetActive(false);//hide the panel
        temperatureText = temperaturePanel.transform.Find("Temperature").GetComponent<TextMeshProUGUI>();
        positionText = temperaturePanel.transform.Find("Position").GetComponent<TextMeshProUGUI>();
    }

    // TODO: wait for YiXian to push backend to server
    public void UpdateHeatmap()
    {
        StartCoroutine(DataServiceManager.Instance.GetHeatMapPic(FetchHeatmap));
    }

    private bool FetchHeatmap(Texture2D tex)
    {
        w = tex.width;
        h = tex.height;
        heatmap = tex;
        material.SetTexture("Heatmap", heatmap);
        return true;
    }

    public void UpdateGradient()
    {
        customGradient.UpdateTexture(ref gradientTex, gradientRes, (int)float.Parse(segmentUI.valueText.text));
        gradientUI.sprite = Sprite.Create(gradientTex, new Rect(0, 0, gradientTex.width, gradientTex.height), new Vector2(0.5f, 0.5f));
    }

    public void SwitchGradientMode(int i)
    {
        customGradient.blendMode = (i == 0) ? CustomGradient.BlendMode.Linear : CustomGradient.BlendMode.Discrete;
        gradientTex = customGradient.GetTexture(gradientRes, (int)float.Parse(segmentUI.valueText.text));
        material.SetTexture("_CustomGradient", gradientTex);
        gradientUI.sprite = Sprite.Create(gradientTex, new Rect(0, 0, gradientTex.width, gradientTex.height), new Vector2(0.5f, 0.5f));
        UpdateGradient();
    }

    #region UI
    public void InvertSamplingFromRayCast(Vector3 hitPoint)
    {
        float angle = Util.ComputeThermocoupleAngle(hitPoint);
        float height = hitPoint.y;
        int x = Mathf.RoundToInt(angle / 360f * w);
        int y = Mathf.RoundToInt(height / Util.MAX_HEIGHT * h);

        if (heatmap != null)
        {
            float temperature = heatmap.GetPixel(x, y).r;

            float mint = float.Parse(minT.inputText.text);
            float maxt = float.Parse(maxT.inputText.text);

            lastPos = hitPoint;
            temperatureText.text = "";
            if (temperature == 1f)
                temperatureText.text += "≥";
            temperatureText.text += Math.Round(temperature * (maxt - mint) + mint, 2).ToString() + "°C";
            positionText.text = "角度:" + angle.ToString() + "°\n" + "高度:" + height.ToString() + "m";
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

    #endregion
}
