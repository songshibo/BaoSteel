using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiaoCengUpdater : MonoSingleton<LiaoCengUpdater>
{
    public bool UpdateLiaoCeng(Texture2D arg)
    {
        Debug.Log("更新料层");
        return true;
    }
}
