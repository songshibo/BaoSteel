using UnityEngine;

public class TextureBaker : MonoBehaviour
{
    public bool bake = false;
    public int resolution = 1024;
    public GameObject obj2bake;
    public int subMeshIndex;

    Material source, dilate;

    private void Start()
    {
        LoadMaterials();
    }

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            bake = true;
        }
    }

    private void OnPostRender()
    {
        if (bake)
        {
            RenderTexture rt = new RenderTexture(resolution, resolution, 16, RenderTextureFormat.ARGB64);
            rt.Create();

            Graphics.SetRenderTarget(rt);
            GL.Clear(true, true, new Color(0, 0, 0, 0));
            GL.PushMatrix();
            GL.LoadOrtho();
            source.SetPass(0);
            Mesh mesh = obj2bake.GetComponent<MeshFilter>().sharedMesh;
            Graphics.DrawMeshNow(mesh, Matrix4x4.identity, subMeshIndex);
            Graphics.SetRenderTarget(null);
            RenderTexture rt2 = new RenderTexture(resolution, resolution, 16, RenderTextureFormat.ARGB64);
            rt2.Create();
            Graphics.Blit(rt, rt2, dilate);
            Debug.Log(Application.dataPath + "/");
            TextureProcessor.SaveTextoPNG(rt2, "BakedTexture", Application.dataPath + "/");
            RenderTexture.active = null;
            rt.Release();
            rt2.Release();
            GL.PopMatrix();

            Debug.Log("Finished");
            bake = false;
        }
    }
}
