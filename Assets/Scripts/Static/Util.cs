using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using Unity.Mathematics;
using System;

public static class Util
{
    public static char[] endChar = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' };
    public static float MAX_HEIGHT = 47.8f;

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
        GameObject[] objects = UnityEngine.Object.FindObjectsOfType<GameObject>();
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
        GameObject[] objects = UnityEngine.Object.FindObjectsOfType<GameObject>();
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
        if (names.Length == 1)
        {
            return names[0];
        }
        else
        {
            string mergedName = "";
            if (names[0].TrimEnd(endChar).Length == names[0].Length)
            {
                mergedName = name.Split('_')[0];
            }
            else
            {
                mergedName = names[0].TrimEnd(endChar);
            }
            return mergedName;
        }
    }

    /// <summary>
    /// Generate Gradient Textures
    /// </summary>
    /// <param name="colorKey">Gradient Color Keys(Array)</param>
    /// <param name="width">Width of gradient texture</param>
    public static Texture2D GenerateGradient(Color[] colors, GradientMode mode = GradientMode.Blend, int innerNum = 2, int width = 512, bool isLinear = false)
    {
        Texture2D texture = new Texture2D(width, 1, TextureFormat.ARGB32, false, isLinear)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point
        };
        Array.Sort(colors, SortColorsbyAChannel);

        Gradient gradient = new Gradient();
        //alpha & Color key
        int len = colors.Length + (colors.Length - 1) * (innerNum - 1);
        GradientColorKey[] colorKey = new GradientColorKey[len];
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[len];
        int index = 0;
        for (int i = 0; i < colors.Length; i++)
        {
            colorKey[index].color = colors[i];
            colorKey[index].time = colors[i].a;
            alphaKey[index].alpha = 1.0f;
            alphaKey[index].time = colors[i].a;
            index++;
            if (i < colors.Length - 1)
            {
                for (int j = 1; j < innerNum; j++)
                {
                    float lerpParam = j / (float)innerNum;
                    Color innerColor = Color.Lerp(colors[i], colors[i + 1], lerpParam);
                    colorKey[index].color = innerColor;
                    colorKey[index].time = innerColor.a;
                    alphaKey[index].alpha = 1.0f;
                    alphaKey[index].time = innerColor.a;
                    index++;
                }
            }
        }
        //Set up Gradient
        gradient.SetKeys(colorKey, alphaKey);
        gradient.mode = mode;
        //Set up Texture
        for (int i = 0; i < width; i++)
        {
            texture.SetPixel(i, 0, gradient.Evaluate(i / (float)width));
        }
        texture.Apply(false);// do not update minimap

        return texture;
    }

    private static int SortColorsbyAChannel(Color a, Color b)
    {
        if (a.a < b.a)
            return 1;
        else if (a.a > b.a)
            return -1;
        else
            return 0;
    }

    public static string GetParentPathOfProject()
    {
        string[] s = Application.dataPath.Split('/');
        string projName = s[s.Length - 2];
        return Application.dataPath.Replace(projName + "/Assets", "");
    }

    public static Vector3 ComputeUIPosition(Vector2 coords)
    {
        Canvas canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        RectTransform canvasRectTransform = canvas.transform as RectTransform;

        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, coords, canvas.GetComponent<Camera>(), out pos);
        return new Vector3(pos.x, pos.y, 0);
    }

    public static float ComputeThermocoupleAngle(Vector3 coords)
    {
        float angle = Mathf.Rad2Deg * Mathf.Atan2(coords.x, coords.z) + 360.0f;
        return angle % 360.0f;
    }

    public static void DisableModelsByTags(string[] tags, bool hide)
    {
        List<GameObject> objList = new List<GameObject>();
        for (int i = 0; i < tags.Length; i++)
        {
            objList.AddRange(GameObject.FindGameObjectsWithTag(tags[i]));
        }

        if (hide)
        {
            foreach (GameObject obj in objList)
            {
                obj.GetComponent<MeshRenderer>().enabled = false;
            }
        }
        else
        {
            foreach (GameObject obj in objList)
            {
                obj.GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }

    public static void HideModelsByTags(string[] tags, bool hide)
    {
        List<GameObject> objList = new List<GameObject>();
        for (int i = 0; i < tags.Length; i++)
        {
            objList.AddRange(GameObject.FindGameObjectsWithTag(tags[i]));
        }

        if (hide)
        {
            LayerManager.Instance.MoveAllFromHighlight(objList.ToArray());
            string info = "Hide models with tags:{";
            foreach (string tag in tags)
            {
                info += tag + " ";
            }
            Debug.Log(info + "}.");
        }
        else
        {
            LayerManager.Instance.AddAllToHighlight(objList.ToArray());
            string info = "Show models with tags:{";
            foreach (string tag in tags)
            {
                info += tag + " ";
            }
            Debug.Log(info + "}.");
        }
    }
}
