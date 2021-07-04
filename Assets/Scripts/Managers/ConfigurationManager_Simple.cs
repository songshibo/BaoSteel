#define INIT_FROM_DATABASE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System;

public class ConfigurationManager_Simple : MonoBehaviour
{
    public string xiange = "";
    public string school = "";
    public string baosteel = "";
    [SerializeField]
    private string ip = "";
    [SerializeField]
    private string port = "";
    private Dictionary<string, float[]> times;

    private void Awake()
    {
#if (INIT_FROM_DATABASE)
        Debug.Log("Initialized from data base");
#else
        Debug.Log("Intialized from local configurations");
#endif
        times = new Dictionary<string, float[]>();
        // No-Async
        InitializeDataServiceManager();
        
        // Initialize all in one Coroutine
        StartCoroutine(DataServiceManager.Instance.GetUnityConfig(InitializeAll, "all"));

        LayerManager.Instance.SetBackgroundColorMaskWeight(0);
        ThermocoupleUpdater.Instance.InitializeThermocouple();
        HeatmapDatabaseUpdater.Instance.InitializeHeatmap();

        ResidualUpdater.Instance.Initialize();
        SelectionManager.Instance.Initialize();
        UIManager.Instance.isSimple = true;

        // Special Settings
        //InitilizeTuyere();
        CullingController.Instance.ResetMaterialProperties();
        // 单独处理heatmap材质，将其设置为正常渲染模式
        Resources.Load<Material>("ClippingMaterials/heatmap").SetFloat("_RenderType", 0);
    }

    private bool InitializeAll(string input)
    {
        string[] separators = new string[] { "@@@" };
        // timing/camera/model/ui
        string[] configs = input.Split(separators, StringSplitOptions.None);

        InitilizeTiming(configs[0]);
        InitializeCamera(configs[1]);
        InitializeModelManager(configs[2]);
        InitilizeUI(configs[3], configs[2]);

        return true;
    }

    private void InitilizeTuyere()
    {
        // 风口更新器里需要初始化风口的大小，包括长宽高
        // 但是不确定风口什么时候生成，所以此处代码写在 ModelManager 里
        // 待风口生成好后，由 ModelManager 调用风口更新器里的 GetTuyereSize
    }

    public void ChangeIP(string newIP)
    {
        //Call database manager to reintialize
        print(newIP);
    }

    public void ChangePort(string newPort)
    {
        print(newPort);
    }

    private void InitializeCamera(string config)
    {
#if (INIT_FROM_DATABASE)
        string[] lines = config.Split('\n');
#else
        string config = Util.ReadConfigFile("camera.txt");
#endif
        Debug.Log("Camera parameters:\n" +
                  "zoom speed:" + lines[0] + "\n" +
                  "rotate speed:" + lines[1] + "\n" +
                  "drag speed:" + lines[2] + "\n" +
                  "limit distance:" + lines[3]
                  );

        FindObjectOfType<FocusController>().SetCameraParameters(float.Parse(lines[0]), float.Parse(lines[1]), float.Parse(lines[2]), float.Parse(lines[3]));
    }

    private void InitializeModelManager(string config)
    {
#if (INIT_FROM_DATABASE)
        string[] lines = Util.RemoveComments(config.Split('\n'));
#else
        string config = Util.ReadConfigFile("ModelManager.txt");
#endif

        // 只要炉缸部分的模型数据
        List<string> temp = new List<string>();
        foreach (string line in lines)
        {
            if (line.StartsWith("hearth"))
            {
                temp.Add(line);
            }
        }
        lines = temp.ToArray();

        // GeneratePipeline: add tags, then generate models.
        ModelManager.Instance.GeneratePipeline(lines);
    }

    private void InitializeDataServiceManager()
    {
#if (INIT_FROM_DATABASE)
        string configString = "ip:" + ip + "\n" + "port:" + port;
#else
        string filename = "DataServiceManagerConfig.txt";
        string configString = Util.ReadConfigFile(filename);
#endif

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

    private void InitilizeUI(string configClip, string configModel)
    {
        string[] linesClip = configClip.Split('\n');
        string[] linesShowPart = Util.RemoveComments(configModel.Split('\n'));
        List<string> temp = new List<string>();
        UIManager.Instance.Initialize(linesClip, temp.ToArray());
    }

    private void InitilizeTiming(string configTiming)
    {
#if (INIT_FROM_DATABASE)
        string[] linesTiming = configTiming.Split('\n');
#else
        string configTiming = Util.ReadConfigFile("timing.txt");
#endif

        foreach (string line in linesTiming)
        {
            string[] temp = line.Split(':');
            times[temp[0]] = new float[] { 0, float.Parse(temp[1]) };
        }
    }

    // 添加计时器需要做两步
    // 第一，在 timing.txt 中添加计时器
    // 第二，在 Update() 中添加该调用的方法
    private void FixedUpdate()
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
                else if (item.Key.Equals("heatmap_timing"))
                {
                    Debug.Log("热力图定时更新");
                    StartCoroutine(DataServiceManager.Instance.GetHeatMapPic(HeatmapDatabaseUpdater.Instance.UpdateHeatmap));
                }
                else if (item.Key.Equals("residual_timing"))
                {
                    Debug.Log("残厚定时更新");
                    StartCoroutine(DataServiceManager.Instance.GetResidualThicknessPic(ResidualUpdater.Instance.UpdateResidual));
                }
                item.Value[0] = 0;
            }
            item.Value[0] += Time.fixedDeltaTime;
        }
    }
}