using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class Util
{
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
}
