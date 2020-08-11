using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;


/// <summary>
/// TODO:
///     风口回旋区
/// </summary>


// 继承 MonoBehavior 类自动拒绝使用 new 关键字实例化对象。
// 使用协程需要继承 MonoBehavior 类。
// 普通单例会出现 warning，这里先采用普通单例。
// 单例模式实例化放在 Awake 函数里，可以避免 warning，但需要挂载到场景中。不挂载会出现空引用 error。
public sealed class ModelManager : MonoBehaviour
{
    private const string COOLING_PLATE = "1";
    private const string COOLING_CROSS = "2";
    private const string COOLING_WALL = "3";
    private const string THERMOCOUPLE = "4";
    private const string TUYERE = "5";

    private static ModelManager instance;
    private GameObject root;
    private DataServiceManager database;

    public static ModelManager Instance() { return instance; }

    private bool GenerateOtherModel(string data, string type, float mh)
    {
        string max_h = mh.ToString().Trim();
        data = data.Trim();
        
        List<GameObject> prefabs = new List<GameObject>();
        string tag = "Untagged";
        if (type == COOLING_WALL)
        {
            if (max_h == "16.6")
            {
                for (int i = 1; i < 6; i++)
                {
                    prefabs.Add((GameObject)Resources.Load("Prefabs/cooling_wall" + i.ToString()));
                }
                tag = "cooling_wall1";
            }
            else if (max_h == "41")
            {
                prefabs.Add((GameObject)Resources.Load("Prefabs/cooling_wall6"));
                prefabs.Add((GameObject)Resources.Load("Prefabs/cooling_wall7"));
                prefabs.Add((GameObject)Resources.Load("Prefabs/cooling_wall8"));
                tag = "cooling_wall4";
            }
            else if (max_h == "60")
            {
                prefabs.Add((GameObject)Resources.Load("Prefabs/cooling_wall9"));
                tag = "cooling_wall5";
            }

        }
        else if (type == TUYERE)
        {
            if (max_h == "16.6")
            {
                prefabs.Add((GameObject)Resources.Load("Prefabs/tuyere"));
                tag = "tuyere";
            }
        }
        else if (type == COOLING_PLATE)
        {
            if (max_h == "21.1")
            {
                for (int i = 0; i < 14; i++)
                {
                    prefabs.Add((GameObject)Resources.Load("Prefabs/coolling_plate"));
                }
                tag = "cooling_plate2";
            }
            else if (max_h == "23.2")
            {
                for (int i = 14; i < 21; i++)
                {
                    prefabs.Add((GameObject)Resources.Load("Prefabs/coolling_plate"));
                }
                tag = "cooling_plate3";
            }
            else if (max_h == "41")
            {
                for (int i = 21; i < 55; i++)
                {
                    prefabs.Add((GameObject)Resources.Load("Prefabs/coolling_plate"));
                }
                tag = "cooling_plate4";
            }
        }
        else if (type == COOLING_CROSS)
        {
            if (max_h == "23.2")
            {
                prefabs.Add((GameObject)Resources.Load("Prefabs/coolling_cross1"));
                tag = "cooling_cross3";
            }
            else if (max_h == "41")
            {
                for (int i = 2; i < 11; i++)
                {
                    prefabs.Add((GameObject)Resources.Load("Prefabs/coolling_cross" + i.ToString()));
                }
                tag = "cooling_cross4";
            }
        }

        
        Debug.Log(type + "    " + mh.ToString() + "    " + prefabs.Count.ToString());
        JToken json = JObject.Parse(data);
        
        foreach (JProperty model_type in json)
        {
            foreach (JProperty model_layer in model_type.Value)
            {
                int cur_layer = int.Parse(model_layer.Name);
                GameObject prefab = prefabs[cur_layer];

                int amount = (int)model_layer.Value["amount"];
                string name = model_layer.Value["name"].ToString();
                float y = (float)model_layer.Value["height"];
                float radius = (float)model_layer.Value["radius"];
                float angle = (float)model_layer.Value["from_angle"];
                float dangle = (float)(360.0 / amount);

                for (int i = 0; i < amount; i++)
                {
                    double cur_angle = angle + dangle * i;
                    double radian = cur_angle * Math.PI / 180;
                    float x = (float)(Math.Sin(radian) * radius);
                    float z = (float)(Math.Cos(radian) * radius);
                    GameObject obj = (GameObject)GameObject.Instantiate(prefab);
                    obj.transform.position = new Vector3(x, y, z);
                    obj.transform.eulerAngles = new Vector3(0, (float)cur_angle, 0);
                    obj.tag = tag;
                    obj.name = name + "_" + (i+1).ToString();
                    //Debug.Log(x);
                    //Debug.Log(y);
                    //Debug.Log(z);
                    //break;
                }
            }
        }
        Debug.Log(json);
        // 销毁铁口处的冷却壁

        return true;
    }

    private bool GenerateThermoModel(string data, string type, float mh)
    {
        string max_h = mh.ToString().Trim();
        data = data.Trim();

        GameObject prefabs = (GameObject)Resources.Load("Prefabs/thermocouple");
        string tag = "Untagged";

        if (max_h == "16.6")
        {
            tag = "thermocouple1";
        }
        else if (max_h == "21.1")
        {
            tag = "thermocouple2";
        }
        else if (max_h == "23.2")
        {
            tag = "thermocouple3";
        }
        else if (max_h == "41")
        {
            tag = "thermocouple4";
        }
        else if (max_h == "60")
        {
            tag = "thermocouple5";
        }

        //Debug.Log(data);
        return true;
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        GeneratePipeline();
    }

    // 加载预设体资源到场景中
    private void GeneratePipeline()
    {
        root = GameObject.Find("Model");
        database = DataServiceManager.Instance();


        // part1    0-16.6，还缺少回旋区
        // include: hearth    wind_pipeline   iron_outlet    cooling_wall1-5    tuyere    thermocouple    gas_flow
        GameObject hearth = Instantiate((GameObject)Resources.Load("Prefabs/hearth"), root.transform);
        hearth.tag = "hearth";
        GameObject iron_outlet = Instantiate((GameObject)Resources.Load("Prefabs/iron_outlet"), root.transform);
        iron_outlet.tag = "iron_outlet";
        GameObject wind_pipeline = Instantiate((GameObject)Resources.Load("Prefabs/wind_pipeline"), root.transform);
        wind_pipeline.tag = "wind_pipeline";

        StartCoroutine(database.GetModel(GenerateOtherModel, COOLING_WALL, 0, 16.6f));  // cooling_wall
        StartCoroutine(database.GetModel(GenerateOtherModel, TUYERE, 0, 16.6f));  // tuyere
        StartCoroutine(database.GetModel(GenerateThermoModel, THERMOCOUPLE, 0, 16.6f));  // thermocouple

        
        // part2    16.6-21.1
        // include: bosh    thermocouple    cooling_plate
        GameObject bosh = Instantiate((GameObject)Resources.Load("Prefabs/bosh"), root.transform);
        bosh.tag = "bosh";

        StartCoroutine(database.GetModel(GenerateThermoModel, THERMOCOUPLE, 16.6f, 21.1f));  // thermocouple
        StartCoroutine(database.GetModel(GenerateOtherModel, COOLING_PLATE, 16.6f, 21.1f));  // cooling_plate


        // part3    21.1-23.2
        // include: waist    thermocouple    cooling_cross1    cooling_plate
        GameObject waist = Instantiate((GameObject)Resources.Load("Prefabs/waist"), root.transform);
        waist.tag = "waist";

        StartCoroutine(database.GetModel(GenerateThermoModel, THERMOCOUPLE, 21.1f, 23.2f));  // thermocouple
        StartCoroutine(database.GetModel(GenerateOtherModel, COOLING_CROSS, 21.1f, 23.2f));  // cooling_cross1
        StartCoroutine(database.GetModel(GenerateOtherModel, COOLING_PLATE, 21.1f, 23.2f));  // cooling_plate


        // part4    23.2-41
        // include: body    thermocouple    cooling_cross2-10    cooling_plate    cooling_wall6-8
        GameObject body = Instantiate((GameObject)Resources.Load("Prefabs/body"), root.transform);
        body.tag = "body";

        StartCoroutine(database.GetModel(GenerateThermoModel, THERMOCOUPLE, 23.2f, 41f));  // thermocouple
        StartCoroutine(database.GetModel(GenerateOtherModel, COOLING_CROSS, 23.2f, 41f));  // cooling_cross2-10
        StartCoroutine(database.GetModel(GenerateOtherModel, COOLING_PLATE, 23.2f, 41f));  // cooling_plate
        StartCoroutine(database.GetModel(GenerateOtherModel, COOLING_WALL, 23.2f, 41f));  // cooling_wall6-8


        // part5    41-60    因为上升管还有四个热电偶，所以高度上限随便设定的一个大一点的数
        // include: throat    chute    cross_temperature_measurement    cooling_wall9    thermocouple
        GameObject throat = Instantiate((GameObject)Resources.Load("Prefabs/throat"), root.transform);
        throat.tag = "throat";
        GameObject chute = Instantiate((GameObject)Resources.Load("Prefabs/chute"), root.transform);
        chute.tag = "chute";
        GameObject cross_temperature_measure = Instantiate((GameObject)Resources.Load("Prefabs/cross_temperature_measure"), root.transform);
        cross_temperature_measure.tag = "cross_temperature_measure";

        StartCoroutine(database.GetModel(GenerateThermoModel, THERMOCOUPLE, 41f, 60f));  // thermocouple
        StartCoroutine(database.GetModel(GenerateOtherModel, COOLING_WALL, 41f, 60f));  // cooling_wall9 
    }

    
}
