using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class HeatmapUpdater : MonoSingleton<HeatmapUpdater>
{
    public ComputeShader shader;
    private ComputeBuffer buffer;

    [Space(10)]
    public int xRes, yRes; // yRes = yAxisScaleFactor * xRes
    public float power;
    public float smoothin;
    public float yAxisScaleFactor;
    [Space]
    public Material targetMat;

    public RenderTexture texture;
    private int kernel;

    private void GenerateHeatMap()
    {
        texture = new RenderTexture(xRes, yRes, 24)
        {
            enableRandomWrite = true
        };
        texture.Create();

        shader.SetTexture(kernel, "Result", texture);
        shader.SetBuffer(kernel, "points", buffer);
        shader.Dispatch(kernel, xRes / 16, yRes / 16, 1);
        targetMat.SetTexture("Heatmap", texture);
    }

    private void test()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("thermocouple");
        print(objects.Length);
        List<Vector3> data = new List<Vector3>();
        float yMax = 47.8f;
        // float yMax = float.MinValue;
        for (int i = 0; i < objects.Length; i++)
        {
            float angle = (float)Math.Round(Mathf.Rad2Deg * Mathf.Atan2(objects[i].transform.position.z, objects[i].transform.position.x), 2) + 180;
            float height = (float)Math.Round(objects[i].transform.position.y, 2);
            // if (height > yMax)
            //     yMax = height;

            Vector3 single_data = new Vector3(angle, height * yAxisScaleFactor, height + UnityEngine.Random.Range(20, 30));

            if (height > 10 && height < 15 && angle > 40 && angle < 120)
            {
                single_data.z += 50;
            }

            data.Add(single_data);
            if (angle == 0)
                data.Add(new Vector3(360, height * yAxisScaleFactor, height + UnityEngine.Random.Range(10, 40)));
        }
        yMax *= yAxisScaleFactor;

        buffer = new ComputeBuffer(data.Count, Marshal.SizeOf(typeof(Vector3)));
        buffer.SetData(data);

        kernel = shader.FindKernel("CSMain");
        shader.SetFloat("xRes", xRes);
        shader.SetFloat("yRes", yRes);
        shader.SetFloat("power", power);
        shader.SetFloat("smoothin", smoothin);
        shader.SetInt("len", data.Count);
        shader.SetFloat("yHeight", yMax);

        targetMat.SetFloat("yFactor", yAxisScaleFactor);
        targetMat.SetFloat("yHeight", yMax);

        GenerateHeatMap();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            test();
        }
    }
}
