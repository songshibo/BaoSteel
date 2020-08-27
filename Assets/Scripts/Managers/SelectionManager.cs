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

    public void AddToOutlineList(GameObject gameObject)
    {
        OutlineBuilder outline = GetOutlineBuilder();
        outline.OutlineLayers.GetOrAddLayer(0).Add(gameObject);
    }

    public void MoveFromOutlineList(GameObject gameObject)
    {
        OutlineBuilder outline = GetOutlineBuilder();
        outline.OutlineLayers.GetOrAddLayer(0).Remove(gameObject);
    }
}
