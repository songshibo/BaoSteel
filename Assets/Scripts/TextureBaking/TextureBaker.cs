using UnityEngine;

public class TextureBaker : MonoBehaviour
{
    bool bake = false;
    Material source, dilate;
    public int resolution;
    public Mesh mesh;
    public int subMeshIndex;
    public bool fullMesh;

    public bool LoadMaterials()
    {
        if (source == null || dilate == null)
        {
            Shader sourceShader = Shader.Find("TextureBake/SourceShader");
            Shader dilateShader = Shader.Find("TextureBake/Dilate");

            if (sourceShader == null)
            {
                Debug.LogError("Can not find source shader!");
                return false;
            }
            else
            {
                source = new Material(sourceShader);
            }
            if (dilateShader == null)
            {
                Debug.LogError("Can not find dilate shader!");
                return false;
            }
            else
            {
                dilate = new Material(dilateShader);
            }
        }
        return true;
    }

    private void OnPostRender()
    {
        if (bake)
        {
            RenderTexture rt = new RenderTexture(resolution, resolution, 16, RenderTextureFormat.ARGB64);
            rt.Create();

            Graphics.SetRenderTarget(rt);
            GL.Clear(true, true, Color.black);
            GL.PushMatrix();
            GL.LoadOrtho();
            source.SetPass(0);
            if (fullMesh)
            {
                Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
            }
            else
            {
                Graphics.DrawMeshNow(mesh, Matrix4x4.identity, subMeshIndex);
            }
            Graphics.SetRenderTarget(null);
            RenderTexture rt2 = new RenderTexture(resolution, resolution, 16, RenderTextureFormat.ARGB64);
            rt2.Create();
            Graphics.Blit(rt, rt2, dilate);
            TextureProcessor.SaveRenderTextureAsAsset(rt2, "Name", "Assets/");
            RenderTexture.active = null;
            rt.Release();
            rt2.Release();
            GL.PopMatrix();

            bake = false;
        }
    }

    // private void Update()
    // {
    //     if (sourceMat != null)
    //     {
    //         Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, sourceMat, 0, Camera.main, 0);
    //         Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, sourceMat, 0, Camera.main, 1);
    //         Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, sourceMat, 0, Camera.main, 2);
    //     }
    // }
    public void Bake()
    {
        bake = true;
    }
}
