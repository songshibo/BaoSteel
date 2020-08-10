using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testDataServiceManager : MonoBehaviour
{
    // Start is called before the first frame update

    bool arr(string d)
    {

        Debug.Log(d);

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
        StartCoroutine(DataServiceManager.Instance().GetTemperature(arr, "5, 6"));

    }

    void TestGetModelAPI()
    {
        StartCoroutine(DataServiceManager.Instance().GetModel(arr, "4", max_h:7));
    }
}
