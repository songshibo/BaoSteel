using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsideStoveManager : MonoSingleton<InsideStoveManager>
{
    public int width, height;
    private MeshFilter filter;
    private MeshRenderer meshRenderer;

    private Mesh GeneratePanel(int w, int h)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[4];
        // center: (0,0,0) -> middle bottom of the panel
        // compute vertex positions
        vertices[0] = Vector3.up * h - 0.5f * w * Vector3.right;
        vertices[1] = Vector3.up * h + 0.5f * w * Vector3.right;
        vertices[2] = -0.5f * w * Vector3.right;
        vertices[3] = 0.5f * w * Vector3.right;
        mesh.vertices = vertices;
        //assign triangle indices
        mesh.triangles = new int[6] { 0, 1, 2, 1, 3, 2 };

        mesh.RecalculateNormals();
        mesh.Optimize();
        return mesh;
    }

    public void Initialize()
    {
        if (!TryGetComponent(out filter))
        {
            filter = gameObject.AddComponent<MeshFilter>();
        }
        if (!TryGetComponent(out meshRenderer))
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        meshRenderer.sharedMaterial = Resources.Load<Material>("InsideStovePanel");
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        filter.mesh = GeneratePanel(width, height);
        meshRenderer.enabled = false;
    }

    public void ControlPanel(bool isOn, float angle)
    {
        transform.rotation = Quaternion.Euler(0, 180 + angle, 0);
        meshRenderer.enabled = isOn;
        Debug.Log("Turn " + (meshRenderer.enabled ? "On" : "Off") +
            " the inside-stove panel");
    }
}
