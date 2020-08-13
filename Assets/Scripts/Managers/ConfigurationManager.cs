using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;


public class ConfigurationManager : MonoBehaviour
{
    private void Awake()
    {
        InitiallzieDataServiceManager();
        InitializeCamera();
        CullingController.Instance.ResetMaterialProperties();
        UIManager.Instance.InitializeUI();
    }

    private void Start()
    {
        CoroutineHandler.Instance.CoroutineTest();
    }

    private void InitializeCamera()
    {
        string filename = "camera.txt";
        string config = ExternalConfigReader.Instance().ReadConfigFile(filename);

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
        string config = ExternalConfigReader.Instance().ReadConfigFile(filename);

        string[] lines = config.Split('\n');

        // add tags, then generate models.
        ModelManager.Instance().GeneratePipeline(lines);
    }

    private void InitiallzieDataServiceManager()
    {
        string filename = "DataServiceManagerConfig.txt";
        string configString = ExternalConfigReader.Instance().ReadConfigFile(filename);
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
}
