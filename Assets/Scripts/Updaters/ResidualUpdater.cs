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
    public struct LastPos{
        public Vector3 pos;
        public bool isBottom;
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

    private GameObject residualPanel;
    private TextMeshProUGUI residualText;
    private TextMeshProUGUI positionText;

    private LastPos last;

    private bool profileStatus;

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

        string prefab_path = "Prefabs/ResidualPanel";
        residualPanel = Instantiate(Resources.Load<GameObject>(prefab_path), GameObject.Find("Canvas").transform);
        residualPanel.SetActive(false);

        residualText = residualPanel.transform.Find("Residual").GetComponent<TextMeshProUGUI>();
        positionText = residualPanel.transform.Find("Position").GetComponent<TextMeshProUGUI>();
    }

    public void ClickAndShowResidualDetail(RaycastHit[] hitArr)
    {
        if(profileStatus && displayMode == ResidualType.ResidualOnly)
        {
            foreach (RaycastHit hit in hitArr)
            {
                if (hit.transform.name.Equals(transform.name))
                {
                    if (hit.normal.y == 1)
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
    }

    internal void UpdateUIPanel()
    {
        if (profileStatus && displayMode == ResidualType.ResidualOnly)
        {
            residualPanel.transform.localPosition = Util.ComputeUIPosition(Camera.main.WorldToScreenPoint(last.pos));
        }
    }

    private void ShowResidualPanel(bool status)
    {
        if(last.pos.y != 0)
        {
            residualPanel.SetActive(status);
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
            ShowResidualPanel(profileStatus);
            displayMode = ResidualType.ResidualOnly;
            UpdateKeyword();
        }
        else
        {
            ShowResidualPanel(false);
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
            last.pos = hitpoint;
            last.isBottom = isBottom;
            residualPanel.SetActive(true);
            residualPanel.transform.localPosition = Util.ComputeUIPosition(Camera.main.WorldToScreenPoint(hitpoint));
            residualText.text = "0.1m";
            positionText.text = "角度：" + Math.Round(angle, 0) + "°\n高度：" + Math.Round(hitpoint.y, 1) + "m";
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

    public void SwitchProfile(bool v)
    {
        profileStatus = v;
        if (v)
        {
            ShowResidualPanel(displayMode == ResidualType.ResidualOnly);
        }
        else
        {
            ShowResidualPanel(false);
        }
    }
}
