using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Formats.Alembic.Importer;


public class BatchLayerUpdater : MonoSingleton<BatchLayerUpdater>
{
    private enum ShowBatchLayer
    {
        no = 0,  // 默认层
        yes = 9,  // 高亮层
    }

    private float startTime;
    private float endTime;
    private GameObject prefab;
    private ShowBatchLayer showBatchLayer;

    private void Start()
    {
        prefab = Resources.Load<GameObject>("Prefabs/material_layer");
        startTime = prefab.GetComponent<AlembicStreamPlayer>().StartTime;
        endTime = prefab.GetComponent<AlembicStreamPlayer>().EndTime; // 应该和abc文件的生命周期保持一致
    }

    IEnumerator GenerateLayer(string number)
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.name = number;
        ChangeLayer(obj.transform);
        Transform c = obj.transform.Find("Canvas");
        c.Find("name").GetComponent<TMP_Text>().text = number;


        Mesh mesh = obj.transform.Find("MateralLayer").Find("material_layer").gameObject.GetComponent<MeshFilter>().sharedMesh;
        AlembicStreamPlayer al = obj.GetComponent<AlembicStreamPlayer>();
        
        for (float time=startTime; time < endTime * 5;)
        {
            mesh.RecalculateBounds();
            c.localPosition = mesh.bounds.center + new Vector3(-mesh.bounds.extents.x - 4, 0, 0.01f);

            // 设置obj的时间
            al.CurrentTime = time / 5;
            time += Time.fixedDeltaTime;
            yield return 0;
        }
        yield return 0;
        DestroyImmediate(obj);
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

        ChangeLayer(transform);
    }

    private void ChangeLayer(Transform t)
    {
        Transform[] trans = t.GetComponentsInChildren<Transform>();
        foreach (Transform node in trans)
        {
            node.gameObject.layer = (int)showBatchLayer;
        }
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
