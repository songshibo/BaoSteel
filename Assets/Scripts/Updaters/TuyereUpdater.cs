using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class TuyereUpdater : MonoSingleton<TuyereUpdater>
{
    private TuyereSize tuyeresize;
    private struct TuyereSize
    {
        public float depth;
        public float height;
        public float width;

    }

    public void GetTuyereSize()
    {
        Vector3 v = GameObject.Find("tuyere_25").transform.Find("tuyere_wind").Find("shape").GetComponent<Renderer>().bounds.size;
        tuyeresize.depth = v.z;
        tuyeresize.width = v.x;
        tuyeresize.height = v.y;
    }

    public bool UpdateTuyereData(string content)
    {
        //string[] items = content.Substring(1, content.Length - 2).Split(',');
        //foreach (string item in items)
        //{

        //}

        JToken items = JObject.Parse(content);
        Dictionary<string, List<float>> number_data = new Dictionary<string, List<float>>();
        foreach (JProperty item in items)
        {
            number_data.Add(item.Name, new List<float>());
            foreach (JProperty data in item.Value)
            {
                number_data[item.Name].Add(float.Parse(data.Value.ToString()));
            }
        }

        GameObject[] tuyeres = GameObject.FindGameObjectsWithTag("tuyere");

        foreach (GameObject tuyere in tuyeres)
        {
            List<float> data = number_data[tuyere.name.Split('_')[1]];
            float depth_scale = data[0] / tuyeresize.depth;
            float height_scale = data[1] / tuyeresize.height;
            float width_scale = data[2] / tuyeresize.width;

            tuyere.transform.Find("tuyere_wind").Find("shape").localScale = new Vector3(width_scale, height_scale, depth_scale);
            tuyere.transform.Find("tuyere_wind").Find("wind").localScale = new Vector3(width_scale, depth_scale, height_scale);

        }
        return true;
    }
}
