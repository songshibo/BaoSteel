using System.Linq;
using UnityEngine;

public class CullingController : Singleton<CullingController>
{
    private Material[] LoadMaterials()
    {
        return Resources.LoadAll("ClippingMaterials", typeof(Material)).Cast<Material>().ToArray();
    }

    public void ResetMaterialProperties()
    {
        Material[] materials = LoadMaterials();
        for (int i = 0; i < materials.Length; ++i)
        {
            materials[i].SetVector("VerticalPlane", new Vector4(200, 0, 0, 0));
            materials[i].SetVector("VerticalNormal", new Vector4(1, 0, 0, 0));
            materials[i].SetVector("TopPlane", new Vector4(0, 200, 0, 0));
            materials[i].SetVector("BottomPlane", Vector4.zero);
        }
    }

    public void ClipMaterialsAtAngle(float angle)
    {
        float radians = Mathf.Deg2Rad * angle;
        Vector4 verticalNormal = new Vector4(Mathf.Sin(radians), 0, Mathf.Cos(radians)).normalized;
        Material[] materials = LoadMaterials();
        for (int i = 0; i < materials.Length; ++i)
        {
            materials[i].SetVector("VerticalPlane", Vector4.zero);
            materials[i].SetVector("VerticalNormal", verticalNormal);
        }
    }

    public void ClipMaterialsAtHeight(float minHeight, float maxHeight)
    {
        Vector4 bottomPlane = new Vector4(0, minHeight, 0, 0);
        Vector4 topPlane = new Vector4(0, maxHeight, 0, 0);
        Material[] materials = LoadMaterials();
        for (int i = 0; i < materials.Length; ++i)
        {
            materials[i].SetVector("TopPlane", topPlane);
            materials[i].SetVector("BottomPlane", bottomPlane);
        }
    }
}
