using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsideStoveUpdater : MonoSingleton<InsideStoveUpdater>
{
    public bool UpdateInsideStove(Texture2D arg)
    {
        InsideStoveManager.Instance.UpdateInsideStove(arg);
        return true;
    }
}
