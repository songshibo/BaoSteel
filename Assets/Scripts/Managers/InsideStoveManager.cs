using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using System;

public class InsideStoveManager : MonoSingleton<InsideStoveManager>
{
    public Texture2D tex2d;
    private MeshFilter filter;
    private MeshRenderer meshRenderer;
    private SwitchManager swtichUI;

    private Mesh GeneratePanel()
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[4];
        Vector2[] uvs = new Vector2[4];
        // vertex positions
        vertices[0] = new Vector3(0.5f, 1f, 0.0f);
        uvs[0] = new Vector2(1f, 1f);
        vertices[1] = new Vector3(0.5f, 0f, 0.0f);
        uvs[1] = new Vector2(1f, 0f);
        vertices[2] = new Vector3(-0.5f, 0f, 0.0f);
        uvs[2] = new Vector2(0f, 0f);
        vertices[3] = new Vector3(-0.5f, 1f, 0.0f);
        uvs[3] = new Vector2(0f, 1f);

        mesh.vertices = vertices;
        //assign triangle indices
        mesh.triangles = new int[6] { 0, 1, 3, 1, 2, 3 };
        mesh.uv = uvs;

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
        if (tex2d != null)
        {
            meshRenderer.sharedMaterial.SetTexture("_MainTex", tex2d);
        }
        else
        {
            Debug.LogError("Inisde Stove texture 2D has not been found!");
        }
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        filter.mesh = GeneratePanel();
        meshRenderer.enabled = false;

        swtichUI = GameObject.Find("InsidePanelSwitch").transform.Find("Switch").GetComponent<SwitchManager>();
    }

    public void ControlPanelFromUI(bool isOn)
    {
        meshRenderer.enabled = isOn;
    }

    public void ControlPanel(float angle)
    {
        transform.rotation = Quaternion.Euler(0, 180 + angle, 0);
        // meshRenderer.enabled = isOn;
        // Auto Change combine UI value;
        // if (swtichUI.isOn != meshRenderer.enabled)
        //     swtichUI.AnimateSwitch();
        Debug.Log("Turn " + (meshRenderer.enabled ? "On" : "Off") +
            " the inside-stove panel");
    }

    public void UpdateInsideStove(Texture2D arg)
    {
        meshRenderer.sharedMaterial.SetTexture("_MainTex", arg);
    }

}
