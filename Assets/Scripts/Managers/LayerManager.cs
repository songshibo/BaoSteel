using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerManager : Singleton<LayerManager>
{
    public void SetBackgroundColorMaskWeight(float lerpValue)
    {
        Resources.Load<Material>("ColorMaskBlitMat").SetFloat("_LerpValue", lerpValue);
    }

    public void AddToHighlight(GameObject gameObject)
    {
        gameObject.layer = LayerMask.NameToLayer("highlight");
        SetBackgroundColorMaskWeight(0.8f);
    }

    public void MoveFromHighlight(GameObject gameObject)
    {
        gameObject.layer = LayerMask.NameToLayer("default");
    }

    public void AddAllToHighlight(GameObject[] gameObjects)
    {
        for (int i = 0; i < gameObjects.Length; i++)
            AddToHighlight(gameObjects[i]);
    }

    public void MoveAllFromHighlight(GameObject[] gameObjects)
    {
        for (int i = 0; i < gameObjects.Length; i++)
            MoveFromHighlight(gameObjects[i]);

        if (!Util.IsAnyObjectsInLayer("highlight"))
            SetBackgroundColorMaskWeight(0.0f);
    }
}
