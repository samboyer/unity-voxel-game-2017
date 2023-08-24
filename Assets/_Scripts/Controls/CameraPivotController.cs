using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.PostProcessing;

public class CameraPivotController : MonoBehaviour {
    public float RotSensitivityX = 5, RotSensitivityY = 5;
    public float zoomSensitivity = 1;
    public float minZoomDistance = 1, maxZoomDistance = 100;

    public float velocityDecayX = 0.5f, velocityDecayY = 0.5f, velocityDecayZoom = 0.5f;

    public float collisionRadius = .5f;
    public LayerMask collisionMask = Physics.DefaultRaycastLayers;

    bool isCameraOrthographic;

    Transform parent;
    Camera cam;
    [HideInInspector]
    public float xRot = 0, yRot = 0;
    [HideInInspector]
    public float zoom;

    public bool lockMouse = true;

    public bool cinematicMode;
    public float cinematicZoom=.2f;

    void Start()
    {
        parent = transform.parent;
        cam = GetComponent<Camera>();

        transform.localRotation = Quaternion.identity;
        //zoom = transform.localPosition.z;
        //xRot = parent.rotation.eulerAngles.y;
        //yRot = parent.rotation.eulerAngles.x;

        //Cursor.lockState = CursorLockMode.Locked;
    }

    float xVelo, yVelo, zoomVelo;

    bool antiOccluderActive = false;
    Vector3 oldworldpos;

    void Update()
    {
        float mouseDeltaX=0, mouseDeltaY=0, deltaZoom=0;

        if (PauseMenuController.isPaused || !lockMouse || Input.GetKey(KeyCode.LeftControl))
        {
            Cursor.lockState = CursorLockMode.None;

        }
        else
        {
            deltaZoom = - Input.mouseScrollDelta.y;
            if (cinematicMode)
            {
                if (Input.GetKey(KeyCode.PageUp)) deltaZoom = -cinematicZoom;
                if (Input.GetKey(KeyCode.PageDown)) deltaZoom = cinematicZoom;
            }

            if (antiOccluderActive && deltaZoom > 0) deltaZoom = 0;
            mouseDeltaX = Input.GetAxis("Mouse X");
            mouseDeltaY = Input.GetAxis("Mouse Y");
            Cursor.lockState = CursorLockMode.Locked;
        }

        Debug.DrawLine(transform.position, parent.position, Color.blue);
        xVelo = xVelo * velocityDecayX + mouseDeltaX * RotSensitivityX;
        yVelo = yVelo * velocityDecayY + mouseDeltaY * RotSensitivityY;


        xRot += (xVelo) % 360;
        yRot += (-yVelo);
        yRot = Mathf.Clamp(yRot, -89f, 89f);

        parent.rotation = Quaternion.Euler(yRot, xRot, 0);

        //SCROLL ZOOM
        zoomVelo = zoomVelo * velocityDecayZoom + deltaZoom * zoom * zoomSensitivity; //multiply deltaZoom to existing zoom for hella zoomy-zoomness.
        zoom = Mathf.Clamp(zoom + zoomVelo, minZoomDistance, maxZoomDistance);
        cam.orthographicSize = zoom;


        RaycastHit hit;
        if (antiOccluderActive) //if the anti-occluder was active last frame,
            antiOccluderActive = Physics.SphereCast(parent.position, collisionRadius, transform.position - parent.position, out hit, zoom, collisionMask.value); //keep on if there's no line of sight to desired position
        else

            //antiOccluderActive = Physics.CheckSphere(parent.TransformPoint(0, 0, -zoom), collisionRadius, collisionMask.value); //enable if something's intersecting with the camera
            //check to see if there's an intersection this frame
            antiOccluderActive = Physics.OverlapCapsule(transform.position, oldworldpos, collisionRadius).Length>0;
            
            //antiOccluderActive = Physics.SphereCast(parent.position,, transform.position - parent.position, out hit, zoom, collisionMask.value); //keep on if there's no line of sight to desired position


        if (antiOccluderActive){ //if the camera is inside something
            Debug.DrawLine(transform.position, parent.TransformPoint(0, 0, -zoom), Color.red);

            if(Physics.SphereCast(parent.position, collisionRadius, transform.position - parent.position, out hit, zoom, collisionMask.value)){
                //antiOccluderActive = true;
                transform.localPosition = new Vector3(0, 0, -Mathf.Min(hit.distance, zoom));
            }
            else
            {
                //antiOccluderActive = false;
            }
        }
        else
        {
            transform.localPosition = new Vector3(0, 0, -zoom);
        }
        oldworldpos = transform.position;

        RenderSettings.fog = transform.position.y <= 0;
    }
}
