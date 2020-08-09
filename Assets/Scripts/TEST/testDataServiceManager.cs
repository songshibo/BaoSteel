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


        StartCoroutine(DataServiceManager.Instance().GetTemperature(arr));



    }

    // Update is called once per frame
    void Update()
    {

    }
}
