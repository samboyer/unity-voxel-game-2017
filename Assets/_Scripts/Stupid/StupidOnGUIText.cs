using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StupidOnGUIText : MonoBehaviour {

    [TextArea]
    public string text = "";

    public bool showStats = true;

    private void OnGUI()
    {
        GUI.Label(new Rect(0, 0, 1000, 1000), text + (showStats ? string.Format("Performance Stats: \nRender time: {0:0.0}ms ({1:0.} FPS)\n",Time.deltaTime*1000,1/Time.deltaTime) : ""));
    }
}
