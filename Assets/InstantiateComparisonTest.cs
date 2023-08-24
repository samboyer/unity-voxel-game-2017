using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class InstantiateComparisonTest : MonoBehaviour {

    public GameObject prefab;

    public int count = 10000;

    public bool instantiateTrigger = false;

    public bool createTrigger = false;

    Stopwatch stopwatch = new Stopwatch();

    public Mesh mesh;
    public Material mat;

	void Update () {
        if (instantiateTrigger)
        {
            stopwatch.Reset();
            stopwatch.Start();
            for (int i = 0;i< count; i++)
            {
                Instantiate(prefab);
            }
            stopwatch.Stop();
            print("Instantiate: "+stopwatch.ElapsedMilliseconds + "ms");
            instantiateTrigger = false;
        }

        if (createTrigger)
        {
            stopwatch.Reset();
            stopwatch.Start();
            for (int i = 0; i < count; i++)
            {
                GameObject gO = new GameObject();
                gO.AddComponent<MeshFilter>().mesh = mesh;
                gO.AddComponent<MeshRenderer>().material = mat;
            }
            stopwatch.Stop();
            print("Create new: " + stopwatch.ElapsedMilliseconds + "ms");
            createTrigger = false;
        }
    }
}
