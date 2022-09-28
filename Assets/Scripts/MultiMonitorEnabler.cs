using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiMonitorEnabler : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("displays connected: " + Display.displays.Length);
        for(var i = 0; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
        }
    }
}
