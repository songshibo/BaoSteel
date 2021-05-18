using System.Collections;
using TMPro;
using UnityEngine;


public class BatchLayerUpdater : MonoSingleton<BatchLayerUpdater>
{
    private enum ShowBatchLayer
    {
        no = 0,
        yes = 1,
    }

    public float startTime = 0f;
    public float endTime = 40f;
    public int frameNumber = 220;

    private Mesh[] meshes;
    private ShowBatchLayer showBatchLayer;
    private Material material;
    private GameObject canvas;
    private float time_per_frame;

    public void Initialize()
    {
        time_per_frame = (endTime - startTime) / frameNumber;
        material = Resources.Load<Material>("layer");
        canvas = Resources.Load<GameObject>("Canvas_frame_info");

        meshes = new Mesh[frameNumber];
        for(int i=0; i < frameNumber; i++)
        {
            GameObject g = Resources.Load<GameObject>("Prefabs/Frames/frame" + i.ToString());
            meshes[i] = g.GetComponent<MeshFilter>().sharedMesh;
        }
    }

    IEnumerator GenerateLayer(string number)
    {
        GameObject obj = new GameObject(number);
        Transform c = Instantiate(canvas, transform).transform;

        obj.layer = 9;
        obj.transform.parent = transform;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.Euler(0, 0, 0);
        c.localPosition = Vector3.zero;

        ShowOrNot(obj.transform);
        ShowOrNot(c);

        MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = material;

        c.name = number + "_info";
        c.Find("name").GetComponent<TMP_Text>().text = number;
        
        for (float time=startTime; time < endTime;)
        {
            int id = (int)(time / time_per_frame);

            meshFilter.mesh = meshes[id];
            c.localPosition = meshes[id].bounds.center + new Vector3(-meshes[id].bounds.extents.x, 0, 0.01f);

            time += Time.fixedDeltaTime;
            yield return 0;
        }
        yield return 0;
        DestroyImmediate(obj);
        DestroyImmediate(c.gameObject);
    }

    public void UpdateBatchLayer(string name)
    {
        StartCoroutine(GenerateLayer(name));
    }

    public void BatchLayerSwitch(bool value)
    {
        
        if (value)
        {
            showBatchLayer = ShowBatchLayer.yes;
        }
        else
        {
            showBatchLayer = ShowBatchLayer.no;
        }

        foreach (Transform child in transform)
        {
            ShowOrNot(child);  // 遍历，将每个孩子传递进去
        }
    }

    private void ShowOrNot(Transform t)
    {
        t.gameObject.SetActive(showBatchLayer != ShowBatchLayer.no);
    }

    internal void Rotate(float angle)
    {
        transform.rotation = Quaternion.Euler(0, angle, 0);
        transform.localPosition = new Vector3(0, 0, 0);
        if (angle == 0)
        {
            transform.localPosition = new Vector3(0, 0, 0.01f);
        }
        else if (angle == 90)
        {
            transform.localPosition = new Vector3(0.01f, 0, 0);
        }
        else if (angle == 180)
        {
            transform.localPosition = new Vector3(0, 0, -0.01f);
        }
        else if (angle == 270)
        {
            transform.localPosition = new Vector3(-0.01f, 0, 0);
        }
        else
        {
            Debug.LogError("这个是什么角度？？？");
        }
    }
}
