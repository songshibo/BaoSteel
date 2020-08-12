using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEditor;
using System.Text.RegularExpressions;


// 继承 MonoBehavior 类自动拒绝使用 new 关键字实例化对象。
// 使用协程需要继承 MonoBehavior 类。
// 普通单例会出现 warning。
// 单例模式实例化放在 Awake 函数里，可以避免 warning，但需要挂载到场景中。不挂载会出现空引用 error。
// 使用 Instantiate 需要继承 MonoBehavior
// 使用 MonoHandler 的缺点：参数怎么传？
public sealed class ModelManager : MonoBehaviour
{
    private static ModelManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        string filename = "ModelManager.txt";
        string config = ExternalConfigReader.Instance().ReadConfigFile(filename);

        string[] lines = config.Split('\n');
        GeneratePipeline(lines);
    }

    public static ModelManager Instance() { return instance; }

    public void GeneratePipeline(string[] models)
    {
        List<string> from_database = new List<string>();
        List<string> not_from_database = new List<string>();

        // 划分哪些模型需要读数据库，哪些不需要
        foreach (string item in models)
        {
            if (item.StartsWith("-"))
            {
                not_from_database.Add(item.Replace("-", "").Trim());
            }
            else
            {
                from_database.Add(item.Trim());
            }
        }

        GenerateTags(not_from_database);
        GenerateTags(from_database);

        GenerateLocalModel(not_from_database);
        GenerateRemoteModel(from_database);
    }

    private void GenerateRemoteModel(List<string> from_database)
    {
        List<string> done = new List<string>();
        foreach (string type in from_database)
        {
            var digits = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            string model = type.TrimEnd(digits);
            if (!done.Contains(model))
            {
                done.Add(model);
                
                //Debug.Log(type);
                // 注意 type 是否 五个选择之一，不带数字
                StartCoroutine(DataServiceManager.Instance().GetModel(testDataServiceManager.arr, model));
            }
        }
    }

    public bool InstantiateRemoteModel(string data, string type)
    {
        //data = data.Trim();

        //JToken json = JObject.Parse(data);

        //foreach (JProperty model_type in json)
        //{
        //    foreach (JProperty one_layer in model_type.Value)
        //    {
        //        int cur_layer = int.Parse(one_layer.Name);
        //        GameObject prefab = (GameObject)Resources.Load("Prefabs/" + one_layer.Value["prefab"].ToString().Trim());

        //        //int amount = (int)one_layer.Value["amount"];
        //        //string name = one_layer.Value["name"].ToString();
        //        //float y = (float)one_layer.Value["height"];
        //        //float radius = (float)one_layer.Value["radius"];
        //        //float angle = (float)one_layer.Value["from_angle"];
        //        //float dangle = (float)(360.0 / amount);

        //        //for (int i = 0; i < amount; i++)
        //        //{
        //        //    double cur_angle = angle + dangle * i;
        //        //    double radian = cur_angle * Math.PI / 180;
        //        //    float x = (float)(Math.Sin(radian) * radius);
        //        //    float z = (float)(Math.Cos(radian) * radius);
        //        //    GameObject obj = Instantiate(prefab);
        //        //    obj.transform.position = new Vector3(x, y, z);
        //        //    obj.transform.eulerAngles = new Vector3(0, (float)cur_angle, 0);
        //        //    obj.tag = tag;
        //        //    obj.name = name + "_" + (i + 1).ToString();
        //        //    //Debug.Log(x);
        //        //    //Debug.Log(y);
        //        //    //Debug.Log(z);
        //        //    //break;
        //        //}
        //    }
        //}
        ////Debug.Log(json);
        //// 销毁铁口处的冷却壁
        return true;
    }

    private void GenerateLocalModel(List<string> not_from_database)
    {
        GameObject root = GameObject.Find("Model");
        if (!root)
        {
            root = new GameObject("Model");
        }

        foreach (string name in not_from_database)
        {
            GameObject obj = Instantiate((GameObject)Resources.Load("Prefabs/" + name), root.transform);
            obj.tag = name;
            obj.name = name;
        }
    }

    private void GenerateTags(List<string> tags)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagProp = tagManager.FindProperty("tags");

        foreach (string tag in tags)
        {
            if (!IsHasTag(tag))
            {
                tagProp.InsertArrayElementAtIndex(0);
                tagProp.GetArrayElementAtIndex(0).stringValue = tag;
            }
        }

        tagManager.ApplyModifiedProperties();
        tagManager.Update();
    }

    private bool IsHasTag(string tag)
    {

        for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.tags.Length; i++)
        {
            if (UnityEditorInternal.InternalEditorUtility.tags[i].Contains(tag))
                return true;
        }
        return false;
    }



    //root = GameObject.Find("Model");
    //    database = DataServiceManager.Instance();


    //    // part1    0-16.6，还缺少回旋区
    //    // include: hearth    wind_pipeline   iron_outlet    cooling_wall1-5    tuyere    thermocouple    gas_flow
    //    GameObject hearth = Instantiate((GameObject)Resources.Load("Prefabs/hearth"), root.transform);
    //hearth.tag = "hearth";
    //    GameObject iron_outlet = Instantiate((GameObject)Resources.Load("Prefabs/iron_outlet"), root.transform);
    //iron_outlet.tag = "iron_outlet";
    //    GameObject wind_pipeline = Instantiate((GameObject)Resources.Load("Prefabs/wind_pipeline"), root.transform);
    //wind_pipeline.tag = "wind_pipeline";

    //    StartCoroutine(database.GetModel(GenerateOtherModel, COOLING_WALL, 0, 16.6f));  // cooling_wall
    //    StartCoroutine(database.GetModel(GenerateOtherModel, TUYERE, 0, 16.6f));  // tuyere
    //    StartCoroutine(database.GetModel(GenerateThermoModel, THERMOCOUPLE, 0, 16.6f));  // thermocouple


    //    // part2    16.6-21.1
    //    // include: bosh    thermocouple    cooling_plate
    //    GameObject bosh = Instantiate((GameObject)Resources.Load("Prefabs/bosh"), root.transform);
    //bosh.tag = "bosh";

    //    StartCoroutine(database.GetModel(GenerateThermoModel, THERMOCOUPLE, 16.6f, 21.1f));  // thermocouple
    //    StartCoroutine(database.GetModel(GenerateOtherModel, COOLING_PLATE, 16.6f, 21.1f));  // cooling_plate


    //    // part3    21.1-23.2
    //    // include: waist    thermocouple    cooling_cross1    cooling_plate
    //    GameObject waist = Instantiate((GameObject)Resources.Load("Prefabs/waist"), root.transform);
    //waist.tag = "waist";

    //    StartCoroutine(database.GetModel(GenerateThermoModel, THERMOCOUPLE, 21.1f, 23.2f));  // thermocouple
    //    StartCoroutine(database.GetModel(GenerateOtherModel, COOLING_CROSS, 21.1f, 23.2f));  // cooling_cross1
    //    StartCoroutine(database.GetModel(GenerateOtherModel, COOLING_PLATE, 21.1f, 23.2f));  // cooling_plate


    //    // part4    23.2-41
    //    // include: body    thermocouple    cooling_cross2-10    cooling_plate    cooling_wall6-8
    //    GameObject body = Instantiate((GameObject)Resources.Load("Prefabs/body"), root.transform);
    //body.tag = "body";

    //    StartCoroutine(database.GetModel(GenerateThermoModel, THERMOCOUPLE, 23.2f, 41f));  // thermocouple
    //    StartCoroutine(database.GetModel(GenerateOtherModel, COOLING_CROSS, 23.2f, 41f));  // cooling_cross2-10
    //    StartCoroutine(database.GetModel(GenerateOtherModel, COOLING_PLATE, 23.2f, 41f));  // cooling_plate
    //    StartCoroutine(database.GetModel(GenerateOtherModel, COOLING_WALL, 23.2f, 41f));  // cooling_wall6-8


    //    // part5    41-60    因为上升管还有四个热电偶，所以高度上限随便设定的一个大一点的数
    //    // include: throat    chute    cross_temperature_measurement    cooling_wall9    thermocouple
    //    GameObject throat = Instantiate((GameObject)Resources.Load("Prefabs/throat"), root.transform);
    //throat.tag = "throat";
    //    GameObject chute = Instantiate((GameObject)Resources.Load("Prefabs/chute"), root.transform);
    //chute.tag = "chute";
    //    GameObject cross_temperature_measure = Instantiate((GameObject)Resources.Load("Prefabs/cross_temperature_measure"), root.transform);
    //cross_temperature_measure.tag = "cross_temperature_measure";

    //    StartCoroutine(database.GetModel(GenerateThermoModel, THERMOCOUPLE, 41f, 60f));  // thermocouple
    //    StartCoroutine(database.GetModel(GenerateOtherModel, COOLING_WALL, 41f, 60f));  // cooling_wall9 
}
