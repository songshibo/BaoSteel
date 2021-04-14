using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterLine : MonoBehaviour
{
    public Vector3 start;
    public Vector3 end;
    public Material material;
    public Transform cameraPosition;
    public float minWidth;
    public float maxWidth;
    private LineRenderer lineRenderer;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.material = material;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

    // Update is called once per frame
    void Update()
    {
        float scale = cameraPosition.position.magnitude / 100f;
        lineRenderer.startWidth = lineRenderer.endWidth = Mathf.Lerp(minWidth, maxWidth, scale);
    }
}
