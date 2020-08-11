using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineHandler : MonoBehaviour
{
    private static CoroutineHandler instance;
    public static CoroutineHandler Instance()
    {
        return instance;
    }

    private void Awake()
    {
        instance = this;
    }

    public void CoroutineTest()
    {
        StartCoroutine(DataServiceManager.Instance().GetModel(testDataServiceManager.arr, "4", max_h: 7));
    }
}
