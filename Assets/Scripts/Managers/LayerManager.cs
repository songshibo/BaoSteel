using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerManager : Singleton<LayerManager>
{
    public void SetBackgroundColorMaskWeight(float lerpValue)
    {
        Resources.Load<Material>("ColorMaskBlitMat").SetFloat("_LerpValue", lerpValue);
    }

    private void AddToHighlight(GameObject gameObject)
    {
        gameObject.layer = LayerMask.NameToLayer("highlight");
    }

    private void MoveFromHighlight(GameObject gameObject)
    {
        gameObject.layer = LayerMask.NameToLayer("ambient");
    }

    public void AddAllToHighlight(GameObject[] gameObjects)
    {
        for (int i = 0; i < gameObjects.Length; i++)
            AddToHighlight(gameObjects[i]);

        if (!Util.IsAnyObjectsInLayer("ambient"))
            SetBackgroundColorMaskWeight(0.0f);
    }

    public void MoveAllFromHighlight(GameObject[] gameObjects)
    {
        for (int i = 0; i < gameObjects.Length; i++)
            MoveFromHighlight(gameObjects[i]);

        SetBackgroundColorMaskWeight(0.7f);
    }
}
