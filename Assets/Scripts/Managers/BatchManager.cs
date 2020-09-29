using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using Newtonsoft.Json;

public class BatchManager : MonoSingleton<BatchManager>
{
    private Dictionary<string, float> data;
    private int start;
    private void Start()
    {
        data = new Dictionary<string, float>();
        for (int i = 0; i < 17; i++)
        {
            data.Add("layer" + i.ToString(), 23 + i);
        }
        start = 0;
        
    }

    private void Update()
    {
        transform.position -= new Vector3(0, 0.25f, 0) * Time.deltaTime;
    }

    public void UpdateBatch()
    {
        GenerateBatchLayer(JsonConvert.SerializeObject(data));

        data.Remove("layer" + start.ToString());
        data.Add("layer" + (start + 17).ToString(), data["layer" + (start + 16).ToString()] + 1);
        start++;

        //for (int i = 0; i < data.Count; i++)
        //{
        //    data["layer" + (start + i).ToString()]--;
        //}

        print("料层");
    }

    public bool GenerateBatchLayer(string content)
    {
        for (int child = 0; child < transform.childCount;)
        {
            DestroyImmediate(transform.GetChild(child).gameObject);
        }

        GameObject prefab = Resources.Load<GameObject>("Prefabs/layer");

        JToken items = JObject.Parse(content);
        foreach (JProperty item in items)
        {
            string number = item.Name;
            float height = float.Parse(item.Value.ToString());
            GameObject obj = Instantiate(prefab, transform);
            obj.transform.localPosition = new Vector3(2.5f, height, 0);
            obj.transform.Find("Canvas").Find("number").GetComponent<Text>().text = number;
            obj.name = number;
        }

        return true;
    }
}
