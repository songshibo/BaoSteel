﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEngine.Networking;
using System.Security.Cryptography;
using System;

public class DataServiceManager : Singleton<DataServiceManager>
{
    // private static DataServiceManager _instance = null;
    // private static readonly object lockHelper = new object();


    private bool initialized = false;

    private Dictionary<string, string> config;

    private string url = null;

    private string ip = null;
    private string port = null;

    // public static DataServiceManager Instance()
    // {

    //     if (_instance == null)
    //     {
    //         lock (lockHelper)
    //         {
    //             if (_instance == null)
    //             {
    //                 _instance = new DataServiceManager();
    //             }
    //         }
    //     }

    //     return _instance;

    // }

    public void initialize(Dictionary<string, string> config)
    {

        // Regex regex = new Regex(@"(?<key>\S+)\s*:\s*(?<item>\S+)", RegexOptions.IgnoreCase);
        // config = new Dictionary<string, string>();
        // string[] config_lines = System.IO.File.ReadAllLines(config_path);
        // foreach (string config_line in config_lines)
        // {

        // if (regex.IsMatch(config_line))
        // {

        //     MatchCollection matchs = regex.Matches(config_line);
        //     foreach (Match match in matchs)
        //     {
        //         config.Add(match.Groups["key"].Value.ToString(), match.Groups["item"].Value.ToString());


        //     }

        // }

        // }
        // if (regex.IsMatch(configString))
        // {

        //     MatchCollection matchs = regex.Matches(configString);
        //     foreach (Match match in matchs)
        //     {
        //         config.Add(match.Groups["key"].Value.ToString(), match.Groups["item"].Value.ToString());


        //     }

        // }

        bool ip_exist = false;
        bool port_exit = false;
        foreach (KeyValuePair<string, string> c in config)
        {

            if (c.Key == "ip" || c.Key == "IP")
            {
                ip = c.Value;
                ip_exist = true;
            }
            if (c.Key == "port" || c.Key == "PORT")
            {
                port = c.Value;
                port_exit = true;
            }

        }

        if (ip_exist && port_exit)
        {
            url = "http://" + ip + ":" + port;

            initialized = true;
        }
        else
        {
            Debug.Log("No ip or port, please set the config file!");
        }

    }

    public IEnumerator GetUnityConfig(Func<string, bool> DataArrangement, string config_file)
    {

        if (initialized)
        {
            if (config_file == null)
            {
                Debug.Log("one config file should specified!");
            }
            else
            {
                Debug.Log(url);
                UnityWebRequest www = UnityWebRequest.Get(url + "/unityconfig?config_file=" + config_file);
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    // Show results as text
                    string data = www.downloadHandler.text;

                    DataArrangement(data);
                    // Or retrieve results as binary data

                }
            }
        }
    }

    public IEnumerator GetTemperature(Func<string, bool> DataArrangement, string layer = "0")
    {

        if (initialized)
        {
            string selected_l = null;
            if (layer != "0")
            {
                selected_l = "?layer=" + layer;
            }

            UnityWebRequest www = UnityWebRequest.Get(url + "/temperature" + selected_l);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                // Show results as text
                string data = www.downloadHandler.text;

                DataArrangement(data);

                // Or retrieve results as binary data

            }
        }
    }


    // 类型不对，会引起 HTTP/1.1 500 Internal Server Error
    public IEnumerator GetModel(Func<string, string, GameObject, bool> DataArrangement, string type, GameObject parent, float min_h = 0, float max_h = 0)
    {
        if (initialized)
        {
            UnityWebRequest www = UnityWebRequest.Get(url + "/model?" + "type=" + type + "&&" + "min_h=" + min_h.ToString() + "&&" + "max_h=" + max_h.ToString());
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(type + "数据库读取失败");
                Debug.Log(www.error);
            }
            else
            {
                // Show results as text
                string data = www.downloadHandler.text;

                DataArrangement(data, type, parent);
                // Or retrieve results as binary data

            }
        }
    }

    public IEnumerator GetThermocoupleTemperature(Func<string, bool> DataArrangement)
    {
        if (initialized)
        {

            UnityWebRequest www = UnityWebRequest.Get(url + "/temperature");
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log("热电偶温度数据读取失败");
                Debug.Log(www.error);
            }
            else
            {
                // Show results as text
                string data = www.downloadHandler.text;

                DataArrangement(data.Trim());
                // Or retrieve results as binary data

            }
        }
    }

    public IEnumerator GetTuyereSize(Func<string, bool> DataArrangement)
    {
        if (initialized)
        {
            UnityWebRequest www = UnityWebRequest.Get(url + "/tuyeresize");
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log("风口数据读取失败");
                Debug.Log(www.error);
            }
            else
            {
                // Show results as text
                string data = www.downloadHandler.text;

                DataArrangement(data.Trim());
                // Or retrieve results as binary data

            }
        }
    }

    public IEnumerator GetHeatLoad(Func<string, bool> DataArrangement)
    {
        if (initialized)
        {

            UnityWebRequest www = UnityWebRequest.Get(url + "/heatload");
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log("热负荷温度数据读取失败");
                Debug.Log(www.error);
            }
            else
            {
                // Show results as text
                string data = www.downloadHandler.text;

                DataArrangement(data.Trim());
                // Or retrieve results as binary data
            }
        }
    }

    public IEnumerator GetHeatmap(Func<string, bool> DataArrangement)
    {
        if (initialized)
        {

            UnityWebRequest www = UnityWebRequest.Get(url + "/heatmap");
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log("热力图数据读取失败");
                Debug.Log(www.error);
            }
            else
            {
                // Show results as text
                string data = www.downloadHandler.text;

                DataArrangement(data.Trim());
                // Or retrieve results as binary data
            }
        }
    }
    public IEnumerator GetResidual(Func<string, bool> DataArrangement)
    {
        if (initialized)
        {

            UnityWebRequest www = UnityWebRequest.Get(url + "/residualThickness");
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log("残厚数据读取失败");
                Debug.Log(www.error);
            }
            else
            {
                // Show results as text
                string data = www.downloadHandler.text;

                DataArrangement(data.Trim());
                // Or retrieve results as binary data
            }
        }
    }
}
