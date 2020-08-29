using System;
using UnityEngine;
using UnityFx.Outline;

public class SelectionManager : MonoSingleton<SelectionManager>
{
    [SerializeField]
    private bool selectionModel = false;
    public int RayCastLayer { get; set; } = 1 << 9; // default layer: highlight
    public string RayCastTag { get; set; } = "";

    private OutlineBuilder GetOutlineBuilder()
    {
        OutlineBuilder outlineBuilder = null;
        try
        {
            outlineBuilder = FindObjectOfType<OutlineBuilder>();
        }
        catch (NullReferenceException e)
        {
            Debug.LogWarning("No OutlineBuilder:" + e);
        }
        return outlineBuilder;
    }

    private void Update()
    {
        if (selectionModel)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, RayCastLayer))
            {
                if (hit.transform.gameObject.tag.Contains(RayCastTag) && Input.GetKeyDown(KeyCode.Mouse0))
                {
                    if (!GetOutlineBuilder().OutlineLayers.GetOrAddLayer(0).Contains(hit.transform.gameObject))
                    {
                        // Clear other selected object
                        ClearCertainLayerContents(0);
                        AddToOutlineList(hit.transform.gameObject);
                    }
                    else
                        MoveFromOutlineList(hit.transform.gameObject);
                }
            }
        }
    }

    public void SetSelectionModel(bool value)
    {
        selectionModel = value;
        if (!selectionModel)
        {
            ClearCertainLayerContents(0);
        }
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

    public void ClearCertainLayerContents(int layerIndex)
    {
        GetOutlineBuilder().OutlineLayers.GetOrAddLayer(layerIndex).Clear();
    }
}
