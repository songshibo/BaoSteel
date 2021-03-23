using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System;

public class ConfigurationManager : MonoBehaviour
{
    private Dictionary<string, float[]> times;

    private void Awake()
    {
        // No-Async
        InitilizeTiming();
        InitializeDataServiceManager();
        InitializeCamera();
        LayerManager.Instance.SetBackgroundColorMaskWeight(0);
        ThermocoupleUpdater.Instance.InitializeThermocouple();
        HeatmapUpdater.Instance.InitializeHeatMap();
        HeatLoadUpdater.Instance.InitializeHeatLoad();
        InsideStoveManager.Instance.Initialize();
        ResidualUpdater.Instance.Initialize();

        // Async
        InitializeModelManager();
        InitilizeUI();

        // Special Settings
        //InitilizeTuyere();
        CullingController.Instance.ResetMaterialProperties();
        // 单独处理heatmap材质，将其设置为正常渲染模式
        Resources.Load<Material>("ClippingMaterials/heatmap").SetFloat("_RenderType", 0);

    }

    private void InitilizeTuyere()
    {
        // 风口更新器里需要初始化风口的大小，包括长宽高
        // 但是不确定风口什么时候生成，所以此处代码写在 ModelManager 里
        // 待风口生成好后，由 ModelManager 调用风口更新器里的 GetTuyereSize

    }

    private void InitializeCamera()
    {
        string filename = "camera.txt";
        string config = Util.ReadConfigFile(filename);

        string[] lines = config.Split('\n');
        Debug.Log("Camera parameters:\n" +
                  "zoom speed:" + lines[0] + "\n" +
                  "rotate speed:" + lines[1] + "\n" +
                  "drag speed:" + lines[2] + "\n" +
                  "limit distance:" + lines[3]
                  );

        FindObjectOfType<FocusController>().SetCameraParameters(float.Parse(lines[0]), float.Parse(lines[1]), float.Parse(lines[2]), float.Parse(lines[3]));
    }

    private void InitializeModelManager()
    {
        string filename = "ModelManager.txt";
        string config = Util.ReadConfigFile(filename);

        string[] lines = Util.RemoveComments(config.Split('\n'));

        // GeneratePipeline: add tags, then generate models.
        ModelManager.Instance.GeneratePipeline(lines);
    }

    private void InitializeDataServiceManager()
    {
        string filename = "DataServiceManagerConfig.txt";
        string configString = Util.ReadConfigFile(filename);
        Dictionary<string, string> config = new Dictionary<string, string>();
        Regex regex = new Regex(@"(?<key>\S+)\s*:\s*(?<item>\S+)", RegexOptions.IgnoreCase);
        if (regex.IsMatch(configString))
        {

            MatchCollection matchs = regex.Matches(configString);
            foreach (Match match in matchs)
            {
                config.Add(match.Groups["key"].Value.ToString(), match.Groups["item"].Value.ToString());
            }
        }

        DataServiceManager.Instance.initialize(config);

        string log = "";
        foreach (KeyValuePair<string, string> c in config)
        {
            log += c.Key + ":" + c.Value + "\n";
        }
        Debug.Log(log);
    }

    private void InitilizeUI()
    {
        string filenameClip = "ui.txt";
        string configClip = Util.ReadConfigFile(filenameClip);
        string[] linesClip = configClip.Split('\n');

        string filenameShowPart = "ModelManager.txt";
        string configShowPart = Util.ReadConfigFile(filenameShowPart);
        string[] linesShowPart = Util.RemoveComments(configShowPart.Split('\n'));

        UIManager.Instance.InitializeUI(linesClip, linesShowPart);
    }

    private void InitilizeTiming()
    {
        string filetiming = "timing.txt";
        string configTiming = Util.ReadConfigFile(filetiming);
        string[] linesTiming = configTiming.Split('\n');

        times = new Dictionary<string, float[]>();
        foreach (string line in linesTiming)
        {
            string[] temp = line.Split(':');
            times[temp[0]] = new float[] { 0, float.Parse(temp[1]) };
        }
    }

    // 添加计时器需要做两步
    // 第一，在 times.txt 中添加计时器
    // 第二，在 Update() 中添加该调用的方法
    private void LateUpdate()
    {
        foreach (var item in times)
        {
            if (item.Value[0] >= item.Value[1])
            {
                if (item.Key.Equals("thermocouple_timing"))
                {
                    Debug.Log("热电偶温度定时更新");
                    StartCoroutine(DataServiceManager.Instance.GetThermocoupleTemperature(ThermocoupleUpdater.Instance.UpdateThermocoupleData));
                }
                else if (item.Key.Equals("tuyere_timing"))
                {
                    Debug.Log("风口定时更新");
                    StartCoroutine(DataServiceManager.Instance.GetTuyereSize(TuyereUpdater.Instance.UpdateTuyereData));
                }
                else if (item.Key.Equals("batch_timing"))
                {
                    // TODO
                    BatchLayerUpdater.Instance.NewLayer(item.Value[1]);
                }
                else if (item.Key.Equals("heatload_timing"))
                {
                    Debug.Log("热负荷图定时更新");
                    StartCoroutine(DataServiceManager.Instance.GetHeatLoad(HeatLoadUpdater.Instance.UpdateHeatLoad));
                }
                else if (item.Key.Equals("heatmap_timing"))
                {
                    Debug.Log("热力图定时更新");
                    StartCoroutine(DataServiceManager.Instance.GetHeatmap(HeatmapUpdater.Instance.UpdateHeatmap));
                }
                else if (item.Key.Equals("residual_timing"))
                {
                    Debug.Log("残厚定时更新");
                    StartCoroutine(DataServiceManager.Instance.GetResidual(ResidualUpdater.Instance.UpdateResidual));
                }

                item.Value[0] = 0;
            }
            item.Value[0] += Time.deltaTime;
        }
    }
}