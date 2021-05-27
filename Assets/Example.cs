using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example : MonoBehaviour
{
    int RayCastLayer { get; set; } = 1 << 9;

    public int num = 0;
    private Mesh[] meshes;
    private MeshFilter filter;
    private MeshCollider mc;

    // Start is called before the first frame update
    void Start()
    {
        meshes = new Mesh[5];
        meshes[0] = Resources.Load<GameObject>("hearth-inside/inside-0").GetComponent<MeshFilter>().sharedMesh;
        meshes[1] = Resources.Load<GameObject>("hearth-inside/inside-90").GetComponent<MeshFilter>().sharedMesh;
        meshes[2] = Resources.Load<GameObject>("hearth-inside/inside-180").GetComponent<MeshFilter>().sharedMesh;
        meshes[3] = Resources.Load<GameObject>("hearth-inside/inside-270").GetComponent<MeshFilter>().sharedMesh;
        meshes[4] = Resources.Load<GameObject>("hearth-inside/inside-full").GetComponent<MeshFilter>().sharedMesh;
        filter = GetComponent<MeshFilter>();
        mc = GetComponent<MeshCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        //filter.mesh = meshes[num];
        //mc.sharedMesh = meshes[num];
        //if (Input.GetMouseButtonDown(1))
        //{
        //    Debug.LogWarning("右键");

        //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //    RaycastHit[] hitArr = Physics.RaycastAll(ray, Mathf.Infinity, RayCastLayer);

        //    if (hitArr.Length > 0)
        //    {
        //        for (int i = 0; i < hitArr.Length; i++)
        //        {
        //            Debug.LogWarning("射到了：" + hitArr[i].collider.gameObject.name + hitArr[i].point);
        //        }
        //    }
        //}
    }

}
