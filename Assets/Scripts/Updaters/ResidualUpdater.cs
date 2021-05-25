using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using System;
using TMPro;

public class ResidualUpdater : MonoSingleton<ResidualUpdater>
{
    public enum ResidualType
    {
        Standard,
        ResidualOnly,
        Combined
    }

    public CustomGradient gradient = new CustomGradient();
    float yRes;
    float xRes;
    public ResidualType displayMode;
    public float corrosionScale = 30f;
    public Material residualMaterial;

    [SerializeField]
    public Texture2D residualThicknessTex;
    private Texture2D gradientTex;

    private Mesh[] meshes;
    private MeshFilter filter;
    private MeshCollider mc;

    public void Initialize()
    {
        gradientTex = gradient.GetTexture(512, 1);

        StartCoroutine(DataServiceManager.Instance.GetResidualThicknessPic(UpdateResidual));

        if (residualMaterial != null)
        {
            residualMaterial.SetFloat("_CorrosionScale", corrosionScale);
            residualMaterial.SetFloat("_MaxHeight", Util.HEARTH_HEIGHT);
            residualMaterial.SetFloat("_BottomRadius", Util.BOTTOM_R);
            residualMaterial.SetFloat("_MinHeight", Util.BOTTOM_H);
            residualMaterial.SetTexture("_Gradient", gradientTex);
            UpdateKeyword();
        }

        meshes = new Mesh[5];
        meshes[0] = Resources.Load<GameObject>("hearth-inside/inside-0").GetComponent<MeshFilter>().sharedMesh;
        meshes[1] = Resources.Load<GameObject>("hearth-inside/inside-90").GetComponent<MeshFilter>().sharedMesh;
        meshes[2] = Resources.Load<GameObject>("hearth-inside/inside-180").GetComponent<MeshFilter>().sharedMesh;
        meshes[3] = Resources.Load<GameObject>("hearth-inside/inside-270").GetComponent<MeshFilter>().sharedMesh;
        meshes[4] = Resources.Load<GameObject>("hearth-inside/inside-full").GetComponent<MeshFilter>().sharedMesh;

        transform.gameObject.layer = 9;

        if (!TryGetComponent(out filter))
        {
            filter = gameObject.AddComponent<MeshFilter>();
        }
        filter.sharedMesh = meshes[0];

        if (!TryGetComponent(out mc))
        {
            mc = gameObject.AddComponent<MeshCollider>();
        }
        mc.sharedMesh = meshes[0];

        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = Resources.Load<Material>("ClippingMaterials/inside_no_tess");
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
    }

    public void ClickAndShowResidualDetail(RaycastHit[] hitArr)
    {
        foreach (RaycastHit hit in hitArr)
        {
            if (hit.transform.name.Equals(transform.name))
            {
                if(hit.normal.y == 1)
                {
                    InvertSamplingFromRayCast(hit.point, true);
                }
                else
                {
                    InvertSamplingFromRayCast(hit.point, false);
                }
                break;
            }
        }
    }

    public bool UpdateResidual(Texture2D tex)
    {
        xRes = tex.width;
        yRes = tex.height;
        residualThicknessTex = tex;
        residualMaterial.SetTexture("_ResidualThickness", residualThicknessTex);
        residualMaterial.SetFloat("_CorrosionScale", corrosionScale);
        return true;
    }

    public void ResidualThicknessSwitch(bool value)
    {
        if (value)
        {
            displayMode = ResidualType.ResidualOnly;
            UpdateKeyword();
        }
        else
        {
            displayMode = ResidualType.Standard;
            UpdateKeyword();
        }
    }

    public void InvertSamplingFromRayCast(Vector3 hitpoint, bool isBottom)
    {
        int x, y;
        float angle = Util.ComputeThermocoupleAngle(hitpoint);
        float height = 0;
        if (!isBottom)
        {
            height = hitpoint.y;
        }
        else
        {
            float radius = new Vector2(hitpoint.x, hitpoint.z).magnitude;
            height = (radius / Util.BOTTOM_R * Util.BOTTOM_H);
        }

        x = Mathf.RoundToInt(angle / 350f * xRes);
        y = Mathf.RoundToInt(height / Util.HEARTH_HEIGHT * yRes);


        if (residualThicknessTex != null)
        {
            float value = residualThicknessTex.GetPixel(x, y).r;

            //TODO: mapping value to defined range
            //TODO: update the UI component
        }
    }

    private void UpdateKeyword()
    {
        switch (displayMode)
        {
            case ResidualType.Standard:
                // Debug.Log("Residual Thickness Display Mode: Standard");
                residualMaterial.EnableKeyword("DISPLAYMODE_STANDARD");
                residualMaterial.DisableKeyword("DISPLAYMODE_RESIDUALTHICKNESS");
                residualMaterial.DisableKeyword("DISPLAYMODE_COMBINE");
                break;
            case ResidualType.ResidualOnly:
                // Debug.Log("Residual Thickness Display Mode: Residual Only");
                // Debug.Log("Corrsion Scale:" + residualMaterial.GetFloat("_CorrosionScale"));
                // Debug.Log("Bottom Radius:" + residualMaterial.GetFloat("_BottomRadius"));
                residualMaterial.DisableKeyword("DISPLAYMODE_STANDARD");
                residualMaterial.EnableKeyword("DISPLAYMODE_RESIDUALTHICKNESS");
                residualMaterial.DisableKeyword("DISPLAYMODE_COMBINE");
                break;
            case ResidualType.Combined:
                // Debug.Log("Residual Thickness Display Mode: Combined");
                residualMaterial.DisableKeyword("DISPLAYMODE_STANDARD");
                residualMaterial.DisableKeyword("DISPLAYMODE_RESIDUALTHICKNESS");
                residualMaterial.EnableKeyword("DISPLAYMODE_COMBINE");
                break;
            default:
                break;
        }
    }
}
