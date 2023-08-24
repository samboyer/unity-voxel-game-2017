using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SamBoyer.VoxelEngine;

public class SplodeyTest : MonoBehaviour {

    public SplodeyVoxels splode;
    public VoxelRenderer VoxRend;

    [Range(0,1)]
    public float percentage;

    public void SPLODE()
    {
        splode.ExplodeModel(VoxRend.Model, percentage,Vector3.zero);
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 100, 25), "Splode")) SPLODE();
    }
}
