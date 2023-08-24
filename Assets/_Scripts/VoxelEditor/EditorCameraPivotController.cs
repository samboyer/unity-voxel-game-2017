using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorCameraPivotController : MonoBehaviour {
    public float RotSensitivityX = 5, RotSensitivityY = 5;
    public float panSensitivity = 1;
    public float zoomSensitivity = 1;
    
    public float minZoomDistance = 1, maxZoomDistance = 100;

    public float velocityDecayX = 0.5f, velocityDecayY = 0.5f, velocityDecayZoom = 0.5f;

    public bool rmbNeeded = true;
    public bool panEnabled = true;
    public bool zoomEnabled = true;

    bool isCameraOrthographic;

    Transform parent;
    Camera cam;
    float xRot = 0, yRot = 0;

    void Start () {
        parent = transform.parent;
        cam = GetComponent<Camera>();

        xRot = parent.rotation.eulerAngles.y;
        yRot = parent.rotation.eulerAngles.x;
        isCameraOrthographic = cam.orthographic;
        transform.localRotation = Quaternion.identity;

        zoom = -transform.localPosition.z;
    }

    float zoom;

    float xVelo, yVelo, zoomVelo;

    void Update()
    {
        float mouseDeltaX = 0, mouseDeltaY = 0, deltaZoom = 0;


        mouseDeltaX = Input.GetAxis("Mouse X");
        mouseDeltaY = Input.GetAxis("Mouse Y");

        deltaZoom = -Input.mouseScrollDelta.y;

        Debug.DrawLine(transform.position, parent.position, Color.blue);
        xVelo = xVelo * velocityDecayX + (Input.GetMouseButton(1) || !rmbNeeded ? mouseDeltaX * RotSensitivityX : 0);
        yVelo = yVelo * velocityDecayY + (Input.GetMouseButton(1) || !rmbNeeded ? mouseDeltaY * RotSensitivityY : 0);

        xRot += (xVelo) % 360;
        yRot += (-yVelo);
        yRot = Mathf.Clamp(yRot, -90f, 90f);

        parent.rotation = Quaternion.Euler(yRot, xRot, 0);

        //SCROLL ZOOM
        if (zoomEnabled && !Input.GetKey(KeyCode.LeftControl))
        {
        zoomVelo = zoomVelo * velocityDecayZoom + deltaZoom * zoomSensitivity; //multiply deltaZoom to existing zoom
        zoom = Mathf.Clamp(zoom + zoomVelo, minZoomDistance, maxZoomDistance);
        cam.orthographicSize = zoom;
        transform.localPosition = new Vector3(0, 0, -zoom);
        }


        if (panEnabled && Input.GetMouseButton(2)) //PAN
        {
            parent.position = parent.position + (mouseDeltaY * panSensitivity) * -transform.up + (mouseDeltaX * panSensitivity) * -transform.right;
        }
    }
}
