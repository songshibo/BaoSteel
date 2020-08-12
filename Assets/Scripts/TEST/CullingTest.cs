using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CullingTest : MonoBehaviour
{
    [Range(0, 360)]
    public float angle;
    public float bottom;
    public float top;
    public bool clipSwitch;

    private void Update()
    {
        if (clipSwitch)
        {
            CullingController.Instance.ClipMaterialsAtAngle(angle);
            CullingController.Instance.ClipMaterialsAtHeight(bottom, top);
        }
        else
            CullingController.Instance.ResetMaterialProperties();
    }
}
