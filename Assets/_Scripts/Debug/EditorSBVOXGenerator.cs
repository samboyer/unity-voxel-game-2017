#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SamBoyer.VoxelEngine;

public class EditorSBVOXGenerator : MonoBehaviour {

    string resourceName = "";

    string centXS="0", centYS="0", centZS="0";
    float centX, centY, centZ;
    bool simplifyPalette = true;

    Transform centVis;

    private void Start()
    {
        centVis = GameObject.Find("CENTERVIS").transform;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(0, 0, 100, 25), "ModelPath:");
        resourceName = GUI.TextField(new Rect(100, 0, 100, 25), resourceName);

        if (GUI.Button(new Rect(0, 25, 100, 25), "Load Resource")) LoadModel();

        GUI.Label(new Rect(0, 60, 100, 25), "Center:");

        float.TryParse(centXS = GUI.TextField(new Rect(100, 60, 50, 25), centXS), out centX);
        float.TryParse(centYS = GUI.TextField(new Rect(150, 60, 50, 25), centYS), out centY);
        float.TryParse(centZS = GUI.TextField(new Rect(200, 60, 50, 25), centZS), out centZ);

        simplifyPalette = GUI.Toggle(new Rect(0, 100, 100, 25), simplifyPalette, "Simplify Palette");

        if (GUI.Button(new Rect(0, 125, 100, 25), "Save SBVOX")) SaveAsSBVOX();

    }

    private void Update()
    {
        centVis.position = new Vector3(centX, centY, centZ);
    }

    VoxelRenderer vr;

    void LoadModel()
    {
        print("loading");

        string pathEnd = resourceName.Substring(resourceName.LastIndexOf('/') + 1);

        vr = GameObject.Find("MODEL").GetComponent<SamBoyer.VoxelEngine.VoxelRenderer>();

        TextAsset file = Resources.Load("models/" + resourceName) as TextAsset;
        if (file == null)
        {
            Debug.Log("model resource " + resourceName + " not found.");
        }
        Stream s = new MemoryStream(file.bytes);

        VoxelModel mdl = VoxelModel.ReadVoxelModel(new BinaryReader(s), pathEnd);
        mdl.modelCenter = Vector3.zero;
        vr.Model = mdl;
    }

    void SaveAsSBVOX()
    {
        if (vr.Model == null)
        {
            print("No model loaded!");
            return;
        }

        vr.Model.modelCenter = new Vector3(centX, centY, centZ);

        if(simplifyPalette)
            vr.Model.palette.SimplifyPalette();

        vr.Model.SaveAsSBVOXFile(UnityEditor.EditorUtility.SaveFilePanelInProject("Save Model", resourceName, "bytes",""));

        print("Saved!");
    }
}
#endif