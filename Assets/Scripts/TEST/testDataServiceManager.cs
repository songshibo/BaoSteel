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
        Debug.Log(data);
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
