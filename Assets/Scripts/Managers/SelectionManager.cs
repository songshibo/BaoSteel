using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityFx.Outline;

public class SelectionManager : Singleton<SelectionManager>
{
    private OutlineBuilder GetOutlineBuilder()
    {
        OutlineBuilder outlineBuilder = null;
        try
        {
            outlineBuilder = UnityEngine.Object.FindObjectOfType<OutlineBuilder>();
        }
        catch (NullReferenceException e)
        {
            Debug.Log("No OutlineBuilder:" + e);
        }
        return outlineBuilder;
    }

    /// <summary>
    /// 给单个物体添加outline效果
    /// </summary>
    /// <param name="gameObjects"></param>
    /// <param name="layerIndex">OutlineLayerCollection的对应outline配置</param>
    public void AddToOutlineList(GameObject gameObject, int layerIndex = 0)
    {
        OutlineBuilder outline = GetOutlineBuilder();
        outline.OutlineLayers.GetOrAddLayer(layerIndex).Add(gameObject);
    }

    /// <summary>
    /// 移除单个物体的outline效果
    /// </summary>
    /// <param name="gameObjects"></param>
    /// <param name="layerIndex">OutlineLayerCollection的对应outline配置</param>
    public void MoveFromOutlineList(GameObject gameObject, int layerIndex = 0)
    {
        OutlineBuilder outline = GetOutlineBuilder();
        outline.OutlineLayers.GetOrAddLayer(layerIndex).Remove(gameObject);
    }

    /// <summary>
    /// 给多个物体添加outline效果
    /// </summary>
    /// <param name="gameObjects">物体的数组</param>
    /// <param name="layerIndex">OutlineLayerCollection的对应outline配置</param>
    public void AddAllToOutlineList(GameObject[] gameObjects, int layerIndex = 0)
    {
        OutlineBuilder outline = GetOutlineBuilder();
        for (int i = 0; i < gameObjects.Length; i++)
        {
            outline.OutlineLayers.GetOrAddLayer(layerIndex).Add(gameObjects[i]);
        }
    }

    /// <summary>
    /// 移除多个物体的outline效果
    /// </summary>
    /// <param name="gameObjects">物体的数组</param>
    /// <param name="layerIndex">OutlineLayerCollection的对应outline配置</param>
    public void MoveAllFromOutlineList(GameObject[] gameObjects, int layerIndex = 0)
    {
        OutlineBuilder outline = GetOutlineBuilder();
        for (int i = 0; i < gameObjects.Length; i++)
        {
            outline.OutlineLayers.GetOrAddLayer(layerIndex).Remove(gameObjects[i]);
        }
    }

    /// <summary>
    /// 清除所有层的gameobject
    /// </summary>
    public void ClearLayersContent()
    {
        GetOutlineBuilder().OutlineLayers.ClearLayerContent();
    }
}
