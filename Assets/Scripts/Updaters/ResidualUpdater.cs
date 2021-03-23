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
    float yRes;
    float xRes;

    public float yHeight = 20f;
    public Material stoveInsideColoredMat;
    public float displacement = -0.7f;
    public Material stoveInsideTessMat;
    [SerializeField]
    Texture2D residualThicknessTex;
    [SerializeField]
    CustomGradient customGradient = new CustomGradient();
    Texture2D gradientTex;

    public void Initialize()
    {
        if (stoveInsideColoredMat != null)
        {
            gradientTex = customGradient.GetTexture(64, 2);
            stoveInsideColoredMat.SetTexture("_ResidualThickness", residualThicknessTex);
            stoveInsideColoredMat.SetTexture("_CustomGradient", gradientTex);
            stoveInsideColoredMat.SetFloat("yHeight", yHeight);
        }
    }

    private void Start()
    {
        yRes = 1024;
        xRes = 2.6f * yRes;
    }

    private void FixedUpdate()
    {
        if (stoveInsideTessMat != null)
        {
            stoveInsideTessMat.SetFloat("_DisplacementAmount", displacement);
            if (Input.GetKeyDown(KeyCode.F1))
            {
                MeshRenderer renderer = GameObject.Find("hearth").GetComponent<MeshRenderer>();
                if (renderer.sharedMaterials[2].shader.name == "Custom/Tessellation Lit")
                {
                    renderer.sharedMaterials = new Material[] { renderer.sharedMaterials[0], renderer.sharedMaterials[1], stoveInsideColoredMat };
                }
                else
                {
                    renderer.sharedMaterials = new Material[] { renderer.sharedMaterials[0], renderer.sharedMaterials[1], stoveInsideTessMat };
                }
            }
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



        print(content);
        return true;
    }
}
