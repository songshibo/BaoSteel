using UnityEngine;
using TMPro;
using System;

public class CampassController : MonoBehaviour
{
    public Transform cameraRig;
    public Transform cameraPosition;
    public Transform arrowPivot;
    public TextMeshProUGUI angle;
    public float minScale = 0.3f;
    public float maxScale = 1;

    private void Update()
    {
        arrowPivot.localRotation = cameraRig.rotation;
        Vector3 actualPos = cameraPosition.position - cameraRig.position;
        angle.text = Math.Round(Util.ComputeThermocoupleAngle(actualPos), 0).ToString() + "°";

        float scale = cameraPosition.position.magnitude / 100f;
        transform.parent.localScale = Vector3.one * Mathf.Lerp(minScale, maxScale, scale);
    }
}
