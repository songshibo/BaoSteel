using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class CullingController : MonoSingleton<CullingController>
{
    public Material[] extraMaterials;
    private Material[] LoadMaterials()
    {
        List<Material> mats = new List<Material>(Resources.LoadAll<Material>("ClippingMaterials/"));
        if (extraMaterials.Length > 0)
        {
            mats.AddRange(extraMaterials);
        }
        return mats.ToArray();
        //GameObject[] prefabs = Resources.LoadAll("Prefabs", typeof(GameObject)).Cast<GameObject>().ToArray();
        //List<Material> sharedMaterials = new List<Material>();
        //for (int i = 0; i < prefabs.Length; i++)
        //{
        //    Material material = prefabs[i].GetComponent<MeshRenderer>().sharedMaterial;
        //    if (!sharedMaterials.Contains(material))
        //        sharedMaterials.Add(material);
        //}
        //return sharedMaterials.ToArray();
    }

    public void ResetMaterialProperties()
    {
        Material[] materials = LoadMaterials();
        for (int i = 0; i < materials.Length; ++i)
        {
            materials[i].SetVector("_VerticalPlane", new Vector4(200, 0, 0, 0));
            materials[i].SetVector("_VerticalNormal", new Vector4(1, 0, 0, 0));
            materials[i].SetVector("_TopPlane", new Vector4(0, 200, 0, 0));
            materials[i].SetVector("_BottomPlane", Vector4.zero);
        }
    }

    public void ClipMaterialsAtAngle(float angle)
    {
        float radians = Mathf.Deg2Rad * angle;
        Vector4 verticalNormal = new Vector4(Mathf.Sin(radians), 0, Mathf.Cos(radians)).normalized;
        Material[] materials = LoadMaterials();
        for (int i = 0; i < materials.Length; ++i)
        {
            materials[i].SetVector("_VerticalPlane", Vector4.zero);
            materials[i].SetVector("_VerticalNormal", verticalNormal);
        }
    }

    public void ClipMaterialsAtHeight(float minHeight, float maxHeight)
    {
        Vector4 bottomPlane = new Vector4(0, minHeight, 0, 0);
        Vector4 topPlane = new Vector4(0, maxHeight, 0, 0);
        Material[] materials = LoadMaterials();
        for (int i = 0; i < materials.Length; ++i)
        {
            materials[i].SetVector("_TopPlane", topPlane);
            materials[i].SetVector("_BottomPlane", bottomPlane);
        }
    }
}
