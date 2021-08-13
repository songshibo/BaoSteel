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
    private DownloadHandlerTexture heatMapPic; // 热力图
    private DownloadHandlerTexture internalDataPic; // 炉内
    private DownloadHandlerTexture residualHandler; // 残厚
    private DownloadHandlerTexture condensateHandler; // 凝铁层
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
        heatMapPic = new DownloadHandlerTexture(true);
        internalDataPic = new DownloadHandlerTexture(true);
        residualHandler = new DownloadHandlerTexture(true);
        condensateHandler = new DownloadHandlerTexture(true);
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

    public IEnumerator Login(Func<string, bool> DataArrangement, string name, string password)
    {
        if (initialized)
        {
            UnityWebRequest www = UnityWebRequest.Get(url + "/verify?username=" + name + "&password=" + password);
            yield return www.SendWebRequest();
            if(www.isNetworkError || www.isHttpError)
            {
                Debug.LogWarning(www.error);
            }
            else
            {
                DataArrangement(www.downloadHandler.text);
            }
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

    public IEnumerator GetInternalDataPic(Func<Texture2D, bool> DataArrangement, string targetData)
    {

        if (initialized)
        {
            UnityWebRequest www = UnityWebRequest.Get(url + "/internal?target_data=" + targetData); //创建UnityWebRequest对象
            www.downloadHandler = internalDataPic;
            yield return www.SendWebRequest();                                 //等待返回请求的信息

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {

                DataArrangement(internalDataPic.texture);
                // Or retrieve results as binary data
            }
        }
    }

    public IEnumerator GetHeatMapPic(Func<Texture2D, bool> DataArrangement)
    {

        if (initialized)
        {
            UnityWebRequest www = UnityWebRequest.Get(url + "/heatmap"); //创建UnityWebRequest对象
            www.downloadHandler = heatMapPic;
            yield return www.SendWebRequest();                                 //等待返回请求的信息

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                DataArrangement(heatMapPic.texture);
                // Or retrieve results as binary data
            }
        }
    }

    public IEnumerator GetResidualThicknessPic(Func<Texture2D, bool> DataArrangement)
    {
        if (initialized)
        {
            UnityWebRequest www_residual = UnityWebRequest.Get(url + "/residualThicknessFromBG"); //创建UnityWebRequest对象
            www_residual.downloadHandler = residualHandler;

            yield return www_residual.SendWebRequest(); //等待返回请求的信息

            if (www_residual.isNetworkError || www_residual.isHttpError)
            {
                Debug.Log(www_residual.error);
                yield break;
            }
            else
            {
                DataArrangement(residualHandler.texture);
                // Or retrieve results as binary data
            }
        }
    }

    public IEnumerator GetCondensateIronPic(Func<Texture2D, bool> DataArrangement)
    {
        if (initialized)
        {
            UnityWebRequest www_condensate = UnityWebRequest.Get(url + "/ningtieFromBG");
            www_condensate.downloadHandler = condensateHandler;
            yield return www_condensate.SendWebRequest(); //等待返回请求的信息

            if (www_condensate.isNetworkError || www_condensate.isHttpError)
            {
                Debug.Log(www_condensate.error);
                yield break;
            }
            else
            {
                DataArrangement(condensateHandler.texture);
                // Or retrieve results as binary data
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

            UnityWebRequest www = UnityWebRequest.Get(url + "/heatmap_test");
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
