﻿using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityFx.Outline;

public class SelectionManager : MonoSingleton<SelectionManager>
{
    public enum SelectionType
    {
        standard,
        heatload,
        heatmap
    }

    public SelectionType selectionType;
    public int RayCastLayer { get; set; } = 1 << 9; // default layer: highlight

    public OutlineBuilder GetOutlineBuilder()
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

    private void FixedUpdate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Input.GetKeyDown(KeyCode.Mouse0) && !EventSystem.current.IsPointerOverGameObject()) // 如果在UI上，则不处理，避免和UI逻辑冲突
        {

            RaycastHit[] hitArr = Physics.RaycastAll(ray, Mathf.Infinity, RayCastLayer);
            ResidualUpdater.Instance.ClickAndShowResidualDetail(hitArr);
            foreach (var h in hitArr)
            {
                Debug.LogWarning(h.transform.name);
            }
            if (Physics.Raycast(ray, out RaycastHit hit, RayCastLayer))
            {
                Debug.DrawLine(Camera.main.transform.position, hit.point, Color.red);
                switch (selectionType)
                {
                    case SelectionType.standard: // standard渲染下可以选择热点偶
                        if (hit.transform.CompareTag("thermocouple"))
                        {
                            ThermocoupleUpdater.Instance.DisplayHittedThermocoupleInfo(hit.transform.gameObject);
                        }
                        break;
                    case SelectionType.heatload:
                        HeatLoadUpdater.Instance.ClickAndShowHeatLoadDetail(hit.point);
                        break;
                    case SelectionType.heatmap:
                        // HeatmapUpdater.Instance.InvertSamplingFromRayCast(hit.point);
                        HeatmapDatabaseUpdater.Instance.InvertSamplingFromRayCast(hit.point);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (selectionType)
                {
                    case SelectionType.standard:
                        break;
                    case SelectionType.heatload:
                        break;
                    case SelectionType.heatmap:
                        break;
                    default:
                        break;
                }
            }
        }

        //no matter hit or not
        ThermocoupleUpdater.Instance.UpdateUIPanel(selectionType != SelectionType.standard);
        // based on current selectionType to change the activity of the UI prefab
        HeatmapDatabaseUpdater.Instance.UpdateUIPanel(selectionType != SelectionType.heatmap);

        HeatLoadUpdater.Instance.UpdateUIPanel(selectionType != SelectionType.heatload);
        ResidualUpdater.Instance.UpdateUIPanel();
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
