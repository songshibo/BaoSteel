using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

public class HeatmapUpdater : MonoSingleton<HeatmapUpdater>
{
    public Color[] keys;
    public ComputeShader shader;
    private ComputeBuffer buffer;

    [Space(20)]
    public int xRes, yRes; // yRes = yAxisScaleFactor * xRes
    public float power;
    public float smoothin;
    public float yAxisScaleFactor;
    public float maxTemp = 50;
    [Space]
    public Material targetMat;

    [Space]
    [SerializeField]
    private RenderTexture texture;
    [SerializeField]
    private Texture2D gradient;
    private int kernel;

    public void SwitchHeatMap()
    {
        targetMat.SetFloat("_RenderHeatMap", targetMat.GetFloat("_RenderHeatMap") == 0f ? 1 : 0);
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
        Debug.Log("热力图更新");
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

        float yMax = 47.8f * yAxisScaleFactor;
        buffer = new ComputeBuffer(data.Count, Marshal.SizeOf(typeof(Vector3)));
        buffer.SetData(data);

        kernel = shader.FindKernel("CSMain");
        shader.SetFloat("xRes", xRes);
        shader.SetFloat("yRes", yRes);
        shader.SetFloat("power", power);
        shader.SetFloat("smoothin", smoothin);
        shader.SetInt("len", data.Count);
        shader.SetFloat("yHeight", yMax);
        shader.SetFloat("maxTemperture", maxTemp);

        targetMat.SetFloat("yFactor", yAxisScaleFactor);
        targetMat.SetFloat("yHeight", yMax);

        GenerateHeatMap();
        buffer.Release();
        return true;
    }

    public void InitializeHeatMapGradient()
    {
        if (keys.Length <= 0)
        {
            Debug.LogError("The number of keys for gradient texture can not be" + keys.Length.ToString());
            return;
        }
        gradient = Util.GenerateGradient(keys);
        targetMat.SetTexture("_CustomGradient", gradient);
    }

    private void OnDestroy()
    {
        if (buffer != null)
            buffer.Dispose();
    }
}
