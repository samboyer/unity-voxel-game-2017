using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StupidCameraPointer : MonoBehaviour {
    public Transform cameraTransform;

    private void Start()
    {
        if (cameraTransform == null) cameraTransform = Camera.main.transform;
    }

    void Update () {
        transform.LookAt(cameraTransform);
	}
}
