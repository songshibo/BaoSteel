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

    float yRes;
    float xRes;
    public ResidualType displayMode;
    public float maxHeight = 15f;
    public float bottomRadius = 6.877f;
    public float bottomHeight = 8.132f;
    public float corrosionScale = 30f;
    public Material residualMaterial;

    [SerializeField]
    public Texture2D residualThicknessTex;

    public void Initialize()
    {

    }

    public void ResidualThicknessSwitch(bool value)
    {
        if (value)
        {
            displayMode = ResidualType.ResidualOnly;
        }
        else
        {
            displayMode = ResidualType.Standard;
        }
    }

    private void FixedUpdate()
    {
        if (residualMaterial != null)
        {
            residualMaterial.SetFloat("_CorrosionScale", corrosionScale);
            residualMaterial.SetFloat("_MaxHeight", maxHeight);
            residualMaterial.SetFloat("_BottomRadius", bottomRadius);
            residualMaterial.SetFloat("_MinHeight", bottomHeight);
            residualMaterial.SetTexture("_ResidualThickness", residualThicknessTex);
            UpdateKeyword();
        }
    }

    private void UpdateKeyword()
    {
        switch (displayMode)
        {
            case ResidualType.Standard:
                residualMaterial.EnableKeyword("DISPLAYMODE_STANDARD");
                residualMaterial.DisableKeyword("DISPLAYMODE_RESIDUALTHICKNESS");
                residualMaterial.DisableKeyword("DISPLAYMODE_COMBINE");
                break;
            case ResidualType.ResidualOnly:
                residualMaterial.DisableKeyword("DISPLAYMODE_STANDARD");
                residualMaterial.EnableKeyword("DISPLAYMODE_RESIDUALTHICKNESS");
                residualMaterial.DisableKeyword("DISPLAYMODE_COMBINE");
                break;
            case ResidualType.Combined:
                residualMaterial.DisableKeyword("DISPLAYMODE_STANDARD");
                residualMaterial.DisableKeyword("DISPLAYMODE_RESIDUALTHICKNESS");
                residualMaterial.EnableKeyword("DISPLAYMODE_COMBINE");
                break;
            default:
                break;
        }
    }

    public bool UpdateResidual(string content)
    {
        Dictionary<string, Dictionary<string, float>> furnace_core = new Dictionary<string, Dictionary<string, float>>(); // 保存炉底的残厚信息
        Dictionary<string, Dictionary<string, float>> furnace_wall = new Dictionary<string, Dictionary<string, float>>();
        JToken items = JToken.Parse(content);
        foreach (JProperty item in items)
        {
            if (item.Name.Equals("furnace_core"))
            {
                foreach (JProperty data in item.Value)
                {
                    furnace_core.Add(data.Name, new Dictionary<string, float>());
                    foreach (JProperty info in data.Value)
                    {
                        furnace_core[data.Name].Add(info.Name, float.Parse(info.Value.ToString()));
                    }
                }
            }
            else if (item.Name.Equals("furnace_wall"))
            {
                foreach (JProperty data in item.Value)
                {
                    furnace_wall.Add(data.Name, new Dictionary<string, float>());
                    foreach (JProperty info in data.Value)
                    {
                        furnace_wall[data.Name].Add(info.Name, float.Parse(info.Value.ToString()));
                    }
                }
            }
        }
        // print(content);
        return true;
    }
}
