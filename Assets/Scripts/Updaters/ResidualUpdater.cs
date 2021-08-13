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
        CondensateOnly,
        Combined
    }
    public struct LastPos
    {
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
    public Texture2D condensateIronTex;
    private Texture2D gradientTex;

    private Mesh[] meshes;
    private MeshFilter filter;
    private MeshCollider mc;

    private GameObject residualPanel;
    private TextMeshProUGUI residualText;
    private TextMeshProUGUI positionText;

    private LastPos last;

    private bool profileStatus; // 是否打开了剖面

    public void Initialize()
    {
        gradientTex = gradient.GetTexture(512, 1);

        //StartCoroutine(DataServiceManager.Instance.GetResidualThicknessPic(UpdateResidual));

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
        ToStandard();
    }

    public void ClickAndShowResidualDetail(RaycastHit[] hitArr)
    {
        if (profileStatus && (displayMode == ResidualType.ResidualOnly || displayMode == ResidualType.CondensateOnly))
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

    // 更新信息面板的位置
    internal void UpdateUIPanel()
    {
        if (profileStatus && (displayMode == ResidualType.ResidualOnly || displayMode == ResidualType.CondensateOnly))
        {
            residualPanel.transform.localPosition = Util.ComputeUIPosition(Camera.main.WorldToScreenPoint(last.pos));
        }
    }

    // 显示或隐藏信息面板，PS: 如果没有点击过炉缸，就不应该显示信息面板
    private void ShowResidualPanel(bool status)
    {
        if (last.pos.y != 0) 
        {
            residualPanel.SetActive(status);
        }
    }

    // 更新残厚贴图
    public bool UpdateResidual(Texture2D residual)
    {
        xRes = residual.width;
        yRes = residual.height;
        residualThicknessTex = residual;

        if(displayMode == ResidualType.ResidualOnly)
        {
            residualMaterial.SetTexture("_ResidualThickness", residualThicknessTex);
        }
        residualMaterial.SetFloat("_CorrosionScale", corrosionScale);
        if (last.pos.y != 0 && displayMode == ResidualType.ResidualOnly && residualPanel.activeSelf)
        {
            InvertSamplingFromRayCast(last.pos, last.isBottom);
        }
        
        return true;
    }

    // 更新凝铁层贴图
    public bool UpdateCondensate(Texture2D condensate)
    {
        xRes = condensate.width;
        yRes = condensate.height;
        condensateIronTex = condensate;
        if (displayMode == ResidualType.CondensateOnly)
        {
            residualMaterial.SetTexture("_ResidualThickness", condensateIronTex);
        }
        
        residualMaterial.SetFloat("_CorrosionScale", corrosionScale);
        if (last.pos.y != 0 && displayMode == ResidualType.CondensateOnly && residualPanel.activeSelf)
        {
            InvertSamplingFromRayCast(last.pos, last.isBottom);
        }
        return true;
    }

    // 切换到标准状态，显示正常的炉缸
    public void ToStandard()
    {
        ShowResidualPanel(false);
        displayMode = ResidualType.Standard;
        UpdateKeyword();
    }

    // 切换到残厚
    public void ToResidualThickness()
    {
        ShowResidualPanel(false);
        displayMode = ResidualType.ResidualOnly;
        residualMaterial.SetTexture("_ResidualThickness", residualThicknessTex);
        UpdateKeyword(); // 启动shader关键字
    }

    // 切换到凝铁层
    public void ToCondensateIron()
    {
        ShowResidualPanel(false);
        displayMode = ResidualType.CondensateOnly;
        residualMaterial.SetTexture("_ResidualThickness", condensateIronTex);
        UpdateKeyword();
    }

    // 碰撞点，然后采样得到碰撞点腐蚀的详细信息
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


        if (residualThicknessTex != null && condensateIronTex != null)
        {
            float value = 0f;
            if (displayMode == ResidualType.ResidualOnly)
            {
                value = residualThicknessTex.GetPixel(x, y).r;
            }
            else if(displayMode == ResidualType.CondensateOnly)
            {
                value = condensateIronTex.GetPixel(x, y).r;
            }

            float corosion = Util.MAX_CORROSION * value;
            last.pos = hitpoint;
            last.isBottom = isBottom;
            residualPanel.SetActive(true);
            residualPanel.transform.localPosition = Util.ComputeUIPosition(Camera.main.WorldToScreenPoint(hitpoint));
            residualText.text = Math.Round(corosion, 3).ToString() + "m";
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
            case ResidualType.CondensateOnly:
                // Debug.Log("Residual Thickness Display Mode: CondensateOnly");
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

    public void SwitchProfile(float angle)
    {
        if (angle == -1)
        {
            profileStatus = false;
            ShowResidualPanel(false);
        }
        else
        {
            profileStatus = true;
            //ShowResidualPanel((displayMode == ResidualType.ResidualOnly || displayMode == ResidualType.CondensateOnly));

            switch (angle)
            {
                case 0:
                    filter.sharedMesh = meshes[0];
                    mc.sharedMesh = meshes[0];
                    break;
                case 90:
                    filter.sharedMesh = meshes[1];
                    mc.sharedMesh = meshes[1];
                    break;
                case 180:
                    filter.sharedMesh = meshes[2];
                    mc.sharedMesh = meshes[2];
                    break;
                case 270:
                    filter.sharedMesh = meshes[3];
                    mc.sharedMesh = meshes[3];
                    break;
                default:
                    Debug.LogWarning("角度不对");
                    break;
            }
        }
    }
}
