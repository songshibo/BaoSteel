using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoHandler : MonoBehaviour
{
    private static MonoHandler instance;
    public static MonoHandler Instance()
    {
        return instance;
    }

    private void Awake()
    {
        instance = this;
    }

    public void Coroutine()
    {
        StartCoroutine(DataServiceManager.Instance().GetModel(testDataServiceManager.arr, "cooling_plate"));
    }

    public void InstantiatePrefab(GameObject prefab, Transform position)
    {
        // 实例化
        Instantiate(prefab, position);
    }
}
