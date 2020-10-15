using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatLoadManager : MonoSingleton<HeatLoadManager>
{
    

    public bool HeatLoadUpdater(string content)
    {
        print(content);
        print("heat");
        return true;
    }
}
