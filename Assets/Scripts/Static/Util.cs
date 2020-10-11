using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using Unity.Mathematics;

public static class Util
{
    public static string ReadConfigFile(string filename)
    {
        string path = Application.dataPath + "/Config/" + filename;
        try
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                StreamReader streamReader = new StreamReader(fs, true);
                return streamReader.ReadToEnd();
            }
        }
        catch (FileNotFoundException ex)
        {
            Debug.LogError(ex);
            return null;
        }
    }


    public static GameObject[] FindChildren(GameObject obj)
    {
        List<GameObject> results = new List<GameObject>();
        foreach (Transform item in obj.transform)
        {
            results.Add(item.gameObject);
        }
        return results.ToArray();
    }

    public static string[] RemoveComments(string[] contents)
    {
        List<string> result = new List<string>();
        foreach (string item in contents)
        {
            if (!item.StartsWith("#"))
            {
                result.Add(item);
            }
        }
        return result.ToArray();
    }

    /// <summary>
    /// find all distinct gameobject array by tags
    /// </summary>
    /// <param name="tags">all tags</param>
    /// <returns>GameObject array</returns>
    public static GameObject[] FindDistinctObjectsWithTags(string[] tags)
    {
        List<GameObject> objects = new List<GameObject>();
        for (int i = 0; i < tags.Length; i++)
            objects.AddRange(GameObject.FindGameObjectsWithTag(tags[i]));

        return objects.Distinct().ToArray();
    }

    // wrap angle to -180/180
    public static float WrapAngle(float angle)
    {
        return angle > 180 ? angle - 360 : angle;
    }

    public static GameObject[] FindObjectsInLayer(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        GameObject[] objects = Object.FindObjectsOfType<GameObject>();
        List<GameObject> objectsInLayer = new List<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i].layer == layer)
                objectsInLayer.Add(objects[i]);
        }
        return objects.ToArray();
    }

    public static bool IsAnyObjectsInLayer(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        GameObject[] objects = Object.FindObjectsOfType<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i].layer == layer)
                return true;
        }
        return false;
    }

    // TE4560A-TE4560B_1  to TE4560
    // TI4370-TI4379_1 to TI4370-TI4379
    // TE4360_1 to TE4360
    public static string MergeThermocoupleName(string name)
    {
        string[] names = name.Split('_')[0].Split('-');
        char[] MyChar = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' };
        if (names.Length == 1)
        {
            return names[0];
        }
        else
        {
            if (names[0].TrimEnd(MyChar).Length == names[0].Length)
            {
                return name.Split('_')[0];
            }
            else
            {
                return names[0].TrimEnd(MyChar);
            }
        }
    }

    /// <summary>
    /// Generate Gradient Textures
    /// </summary>
    /// <param name="colorKey">Gradient Color Keys(Array)</param>
    /// <param name="width">Width of gradient texture</param>
    public static Texture2D GenerateGradient(Color[] colors, int width = 512, bool isLinear = false)
    {
        Texture2D texture = new Texture2D(width, 1, TextureFormat.ARGB32, false, isLinear)
        {
            alphaIsTransparency = true,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point
        };

        Gradient gradient = new Gradient();
        //alpha & Color key
        GradientColorKey[] colorKey = new GradientColorKey[colors.Length];
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[colors.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            colorKey[i].color = colors[i];
            colorKey[i].time = colors[i].a;
            alphaKey[i].alpha = 1.0f;
            alphaKey[i].time = colors[i].a;
        }
        //Set up Gradient
        gradient.SetKeys(colorKey, alphaKey);
        //Set up Texture
        for (int i = 0; i < width; i++)
        {
            texture.SetPixel(i, 0, gradient.Evaluate(i / (float)width));
        }
        texture.Apply(false);// do not update minimap

        return texture;
    }
}
