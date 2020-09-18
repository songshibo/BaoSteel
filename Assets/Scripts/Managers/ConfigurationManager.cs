using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;


public class ConfigurationManager : MonoBehaviour
{
    private void Awake()
    {
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
}