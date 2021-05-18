using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;

public class GeneratePrefab
{
    private static string prefabDirectory = "Assets/Resources/Prefabs/Frames/";

    [MenuItem("Tools/Generate prefab")]
    public static void Generate()
    {
        int num = 0;
        GameObject[] gs = Selection.gameObjects;
        foreach (GameObject g in gs)
        {
            string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(g);
            //Debug.LogWarning(prefabPath);
            if (prefabPath.EndsWith(".prefab"))
            {
                Debug.LogWarning(g.name + "该物体是prefab");
                continue;
            }


            string path = string.Concat(prefabDirectory, g.name, ".prefab"); //获取文件名
            bool success = false;
            PrefabUtility.SaveAsPrefabAssetAndConnect(g, path, InteractionMode.UserAction, out success);
            if (success)
            {
                num++;
            }
            else
            {
                Debug.LogWarning("怎么失败了");
            }
        }
        Debug.LogWarning(num);
    }

}