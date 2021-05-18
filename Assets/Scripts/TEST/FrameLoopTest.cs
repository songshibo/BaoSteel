using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameLoopTest : MonoBehaviour
{
    Mesh[] frames;
    MeshFilter filter;
    float time = 0;
    [SerializeField]
    int index = 0;
    void Start()
    {
        List<Mesh> meshes = new List<Mesh>();
        for (int i = 0; i <= 219; i++)
        {
            GameObject frame_mesh = Resources.Load<GameObject>("MaterialLayerAnimation/frame" + i.ToString());
            meshes.Add(frame_mesh.GetComponent<MeshFilter>().sharedMesh);
        }
        frames = meshes.ToArray();
        meshes.Clear();

        filter = GetComponent<MeshFilter>();
        filter.mesh = frames[index];
    }

    // Update is called once per frame
    void Update()
    {
        if (time > 0.033f)
        {
            index++;
            if (index > 219)
            {
                index = 0;
            }
            filter.mesh = frames[index];
            time = 0;
        }
        time += Time.deltaTime;
    }
}
