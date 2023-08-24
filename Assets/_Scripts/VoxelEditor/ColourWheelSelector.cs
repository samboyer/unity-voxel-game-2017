using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColourWheelSelector : MonoBehaviour {
    public GameObject selectionObject;
    public bool useSelectable;

    Camera cam;

    public float raycastStartOffset;
    public float transitionDuration = 0.25f;

    Coroutine moveCo;
    Vector3 desiredPosition;

    void Start () {
        cam = Camera.main;
    }

    void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray r = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(r, out hit, 100000)){
                print(hit.transform.GetComponent<SpriteRenderer>().color);

                if (useSelectable)
                {
                    if (moveCo != null) StopCoroutine(moveCo);
                    desiredPosition = hit.transform.position;
                    moveCo = StartCoroutine(MoveSelectionSprite());
                }
            }
        }
	}


    IEnumerator MoveSelectionSprite()
    {
        Vector3 start = selectionObject.transform.position;
        Vector3 end = desiredPosition;

        float t = 0;
        while (t <= 1)
        {
            t += Time.deltaTime / transitionDuration;
            float t2 = t - 1;
            selectionObject.transform.position = Vector3.Lerp(start, end, t2 * t2 * t2 + 1);
            yield return true;
        }
    }
}
