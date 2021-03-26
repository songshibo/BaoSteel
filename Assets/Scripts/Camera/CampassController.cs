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
        angle.text = Math.Round(Util.ComputeThermocoupleAngle(cameraPosition.position), 0).ToString() + "°";

        float scale = cameraPosition.position.magnitude / 100f;
        transform.parent.localScale = Vector3.one * Mathf.Lerp(minScale, maxScale, scale);
    }
}
