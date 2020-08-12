using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineHandler : MonoSingleton<CoroutineHandler>
{
    public void CoroutineTest()
    {
        StartCoroutine(DataServiceManager.Instance.GetModel(testDataServiceManager.arr, "4", max_h: 7));
    }
}
