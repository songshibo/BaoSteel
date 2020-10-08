using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;


public class ConfigurationManager : MonoBehaviour
{
    private Dictionary<string, float[]> times;

    private void Awake()
    {
        InitilizeTiming();
        InitializeDataServiceManager();
        InitializeCamera();
        InitializeModelManager();
        InitilizeUI();
        CullingController.Instance.ResetMaterialProperties();
        LayerManager.Instance.SetBackgroundColorMaskWeight(0);
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
            times[temp[0]] = new float[]{0, float.Parse(temp[1])};
        }
    }

    // 添加计时器需要做两步
    // 第一，在 times.txt 中添加计时器
    // 第二，在 Update() 中添加该调用的方法
    private void Update()
    {
        foreach (var item in times)
        {
            if (item.Value[0] >= item.Value[1])
            {
                if (item.Key.Equals("thermocouple_timing"))
                {
                    StartCoroutine(DataServiceManager.Instance.GetThermocoupleTemperature(UIManager.Instance.ThermocoupleUpdater));
                    
                }
                else if (item.Key.Equals("tuyere_timing"))
                {
                    StartCoroutine(DataServiceManager.Instance.GetTuyereSize(ModelManager.Instance.TuyereUpdater));
                }
                else if (item.Key.Equals("batch_timing"))
                {
                    BatchManager.Instance.Test();
                }

                item.Value[0] = 0;
            }
            item.Value[0] += Time.deltaTime;
        }
    }
}