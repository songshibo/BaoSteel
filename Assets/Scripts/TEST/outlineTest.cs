using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class outlineTest : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SelectionManager.Instance.MoveFromOutlineList(GameObject.Find("chute"));
        }
    }
}
