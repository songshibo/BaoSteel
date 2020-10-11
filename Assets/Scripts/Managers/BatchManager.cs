using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using Newtonsoft.Json;
using System;

public class BatchManager : MonoSingleton<BatchManager>
{
    private Vector3 from;
    private Vector3 target;
    private float moveSpeed;
    private int count = 1; // 料层的编号
    private int number = 21; // 料层最大数量

    private void Start()
    {
        from = new Vector3(2.5f, 41, 0);
        target = new Vector3(2.5f, 22, 0);
        moveSpeed = 1f;

    }

    IEnumerator GenerateLayer(string number)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/layer");
        GameObject obj = Instantiate(prefab, transform);
        obj.transform.localPosition = from;
        obj.transform.Find("Canvas").Find("number").GetComponent<Text>().text = number;
        obj.name = number;

        while (obj.transform.position.y > target.y)
        {
            float y = obj.transform.position.y - moveSpeed * Time.deltaTime;
            float x = GetX(y);
            float z = 0;
            obj.transform.position = new Vector3(x, y, z);
            yield return 0;
        }
        //yield return 0;
        DestroyImmediate(obj);
    }

    private float GetX(float y)
    {
        return (float)(6.8 - Math.Log(3.67 * (y - 21)));
    }

    public void NewLayer(float time)
    {
        moveSpeed = (from.y - target.y) / (number * time);
        StartCoroutine(GenerateLayer("layer" + count.ToString()));
        count++;
    }


    //private Dictionary<string, float> data;
    //private int start;
    //private void Start()
    //{
    //    data = new Dictionary<string, float>();
    //    for (int i = 0; i < 17; i++)
    //    {
    //        data.Add("layer" + i.ToString(), 23 + i);
    //    }
    //    start = 0;

    //}

    //private void Update()
    //{
    //    transform.position -= new Vector3(0, 0.25f, 0) * Time.deltaTime;
    //}

    //public void UpdateBatch()
    //{
    //    GenerateBatchLayer(JsonConvert.SerializeObject(data));

    //    data.Remove("layer" + start.ToString());
    //    data.Add("layer" + (start + 17).ToString(), data["layer" + (start + 16).ToString()] + 1);
    //    start++;

    //    //for (int i = 0; i < data.Count; i++)
    //    //{
    //    //    data["layer" + (start + i).ToString()]--;
    //    //}

    //    print("料层");
    //}

    //public bool GenerateBatchLayer(string content)
    //{
    //    for (int child = 0; child < transform.childCount;)
    //    {
    //        DestroyImmediate(transform.GetChild(child).gameObject);
    //    }

    //    GameObject prefab = Resources.Load<GameObject>("Prefabs/layer");

    //    JToken items = JObject.Parse(content);
    //    foreach (JProperty item in items)
    //    {
    //        string number = item.Name;
    //        float height = float.Parse(item.Value.ToString());
    //        GameObject obj = Instantiate(prefab, transform);
    //        obj.transform.localPosition = new Vector3(2.5f, height, 0);
    //        obj.transform.Find("Canvas").Find("number").GetComponent<Text>().text = number;
    //        obj.name = number;
    //    }

    //    return true;
    //}
}
