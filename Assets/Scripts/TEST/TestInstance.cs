using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInstance : MonoSingleton<TestInstance>
{
    List<Vector3> part1 = new List<Vector3>();
    List<Vector3> part2 = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        part1.Add(new Vector3(1, 1, 1));
        print(part1);
        Debug.Log(11111111111);
        print(part2);
        Debug.Log(11111111111);
        part2 = part1;
        print(part1);
        Debug.Log(11111111111);
        print(part2);
        Debug.Log(11111111111);
        part1.Add(new Vector3(2, 2, 2));
        part2.Add(new Vector3(3, 3, 3));
        print(part1);
        Debug.Log(11111111111);
        print(part2);
    }

    void print(List<Vector3> temp)
    {
        foreach (var item in temp)
        {
            Debug.Log(item);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
