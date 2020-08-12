using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System;

public class testDataServiceManager : MonoBehaviour
{
    // Start is called before the first frame update

    public static bool arr(string data, string type)
    {

        data = data.Trim();
        Debug.Log(data);
        JToken json = JObject.Parse(data);

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
                    obj.transform.eulerAngles = new Vector3(-90, (float)cur_angle, 0);
                    obj.tag = tag;
                    obj.name = name + "_" + (i + 1).ToString();
                    //Debug.Log(x);
                    //Debug.Log(y);
                    //Debug.Log(z);
                    //break;
                }
            }
        }
        //Debug.Log(json);
        // 销毁铁口处的冷却壁
        return true;
    }

    void Start()
    {

        // TestGetTemperatureAPI();
        TestGetModelAPI();

    }

    // Update is called once per frame
    void TestGetTemperatureAPI()
    {
        // StartCoroutine(DataServiceManager.Instance.GetTemperature(arr, "5, 6"));

    }

    void TestGetModelAPI()
    {
        StartCoroutine(DataServiceManager.Instance.GetModel(arr, "cooling_plate", max_h: 7));
    }
}
