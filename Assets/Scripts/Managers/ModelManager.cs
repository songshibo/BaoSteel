﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEditor;
using System.Text.RegularExpressions;

// 使用协程 需要 MonoBehavior
// 继承 MonoBehavior 就不能 new， 也就不能使用普通单例
// 单例模式实例化放在 Awake 函数里，可以避免 warning，但需要挂载到场景中。不挂载会出现空引用 error。
// 使用 Instantiate 需要继承 MonoBehavior，可以 GameObject.Instantiate
public sealed class ModelManager : MonoSingleton<ModelManager>
{
    public void GeneratePipeline(string[] models)
    {
        List<string> done = new List<string>(); // 记录已生成的模型，例如 cooling_wall5 和 cooling_wall6 只需要请求 cooling_wall 生成一次就可以了。

        // 划分哪些模型需要读数据库，哪些不需要
        // 以 '-' 开头的行，表示不需要读数据库
        // 读数据库的行要去掉末尾的数字，例如 type 为 cooling_wall 而不是 cooling_wall8。type 错误会导致 http 错误。
        foreach (string item in models)
        {
            string itm = item.Trim();
            if (itm.StartsWith("-"))
            {
                string s = itm.Replace("-", "");
                GenerateTag(s);
                GenerateLocalModel(s);

            }
            else
            {
                GenerateTag(itm);

                var digits = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
                itm = itm.TrimEnd(digits);
                if (!done.Contains(itm))
                {
                    done.Add(itm);

                    // 注意 type 是否 五个选择之一，不带数字
                    StartCoroutine(DataServiceManager.Instance.GetModel(GenerateRemoteModel, itm));
                }
                
            }
        }
    }

    private void DestroyIronOutlet()
    {
        for (int i = 7; i <= 10; i++)
        {
            GameObject obj1 = GameObject.Find("cooling_wall3_" + i.ToString());
            GameObject obj2 = GameObject.Find("cooling_wall4_" + i.ToString());
            DestroyImmediate(obj1);
            DestroyImmediate(obj2);
        }
        for (int i = 19; i <= 22; i++)
        {
            GameObject obj1 = GameObject.Find("cooling_wall3_" + i.ToString());
            GameObject obj2 = GameObject.Find("cooling_wall4_" + i.ToString());
            DestroyImmediate(obj1);
            DestroyImmediate(obj2);
        }
        for (int i = 35; i <= 38; i++)
        {
            GameObject obj1 = GameObject.Find("cooling_wall3_" + i.ToString());
            GameObject obj2 = GameObject.Find("cooling_wall4_" + i.ToString());
            DestroyImmediate(obj1);
            DestroyImmediate(obj2);
        }
        for (int i = 47; i <= 50; i++)
        {
            GameObject obj1 = GameObject.Find("cooling_wall3_" + i.ToString());
            GameObject obj2 = GameObject.Find("cooling_wall4_" + i.ToString());
            DestroyImmediate(obj1);
            DestroyImmediate(obj2);
        }
        GameObject obj = GameObject.Find("cooling_wall4_" + 11.ToString());
        DestroyImmediate(obj);
        obj = GameObject.Find("cooling_wall4_" + 23.ToString());
        DestroyImmediate(obj);
        obj = GameObject.Find("cooling_wall4_" + 39.ToString());
        DestroyImmediate(obj);
        obj = GameObject.Find("cooling_wall4_" + 51.ToString());
        DestroyImmediate(obj);
    }

    public bool GenerateRemoteModel(string data, string type)
    {
        data = data.Trim();
        // Debug.Log(data);
        JToken json = JObject.Parse(data);

        GameObject root = GameObject.Find("Model");
        if (!root)
        {
            root = new GameObject("Model");
        }

        foreach (JProperty model_type in json)
        {
            foreach (JProperty one_layer in model_type.Value)
            {
                int cur_layer = int.Parse(one_layer.Name);
                string tag = one_layer.Value["prefab"].ToString().Trim();
                string name = one_layer.Value["name"].ToString().Trim();
                GameObject prefab = (GameObject)Resources.Load("Prefabs/" + one_layer.Value["prefab"].ToString().Trim());

                int amount = (int)one_layer.Value["amount"];
                float y = (float)one_layer.Value["height"];
                float radius = (float)one_layer.Value["radius"];
                float angle = (float)one_layer.Value["from_angle"];
                float dangle = (float)(360.0 / amount);

                for (int i = 0; i < amount; i++)
                {
                    double cur_angle = angle + dangle * i;
                    double radian = cur_angle * Math.PI / 180;
                    float x = (float)(Math.Sin(radian) * radius);
                    float z = (float)(Math.Cos(radian) * radius);
                    GameObject obj = Instantiate(prefab);
                    obj.transform.position = new Vector3(x, y, z);
                    obj.transform.eulerAngles = new Vector3(0, (float)cur_angle, 0);
                    obj.tag = tag;
                    obj.name = name + "_" + (i + 1).ToString();
                    obj.transform.parent = root.transform;
                }
            }
        }
        if (type == "cooling_wall")
        {
            DestroyIronOutlet();
        }
        return true;
    }

    private void GenerateLocalModel(string name)
    {
        GameObject root = GameObject.Find("Model");
        if (!root)
        {
            root = new GameObject("Model");
        }

        GameObject obj = Instantiate((GameObject)Resources.Load("Prefabs/" + name), root.transform);
        obj.tag = name;
        obj.name = name;
    }

    private void GenerateTag(string tag)
    {
        if (!IsHasTag(tag))
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagProp = tagManager.FindProperty("tags");
            tagProp.InsertArrayElementAtIndex(0);
            tagProp.GetArrayElementAtIndex(0).stringValue = tag;
            tagManager.ApplyModifiedProperties();
            tagManager.Update();
        }
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

}
