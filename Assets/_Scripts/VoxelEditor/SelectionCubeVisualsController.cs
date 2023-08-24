using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionCubeVisualsController : MonoBehaviour {
    public float AnimationSwitchColorDuration = 0.2f;

    Transform Cube;
    MeshRenderer FaceRend, CubeRend;
    int ColorID;

    Color colWhite = new Color(1, 1, 1, 0.5f);
    Color colRed = new Color(1, 0, 0, 1);
    Color colBlue = new Color(0, 0, 1, 1);
    Color colGreen = new Color(0, 1, 0, 0.8F);

    int lastKey = 0;

    void Start () {
        ColorID = Shader.PropertyToID("_Color");
        Cube = transform.GetChild(0);
        CubeRend = Cube.GetComponent<MeshRenderer>();
        FaceRend = Cube.GetChild(0).GetComponent <MeshRenderer>();
	}
	
	void Update () {
        CubeRend.enabled = true;

        if (Input.GetKey(KeyCode.LeftShift)){
            if (lastKey != 1)
            {
                StartCoroutine(AnimationSwitchColor());
            }
            FaceRend.enabled = false;
            CubeRend.material.SetColor(ColorID, colRed);
            lastKey = 1;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            if (lastKey != 2)
            {
                StopCoroutine(AnimationSwitchColor());
                StartCoroutine(AnimationSwitchColor());
            }
            FaceRend.enabled = false;
            CubeRend.material.SetColor(ColorID, colBlue);
            lastKey = 2;
        }
        else if (Input.GetKey(KeyCode.LeftAlt))
        {
            if (lastKey != 3)
            {
                StopCoroutine(AnimationSwitchColor());
                StartCoroutine(AnimationSwitchColor());
            }
            FaceRend.enabled = false;
            CubeRend.material.SetColor(ColorID, colGreen);
            lastKey = 3;
        }
        else
        {
            FaceRend.enabled = true;
            CubeRend.material.SetColor(ColorID, colWhite);
            lastKey = 0;
        }

        if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
        {
            CubeRend.enabled = false;
            FaceRend.enabled = false;
        }
    }

    IEnumerator AnimationSwitchColor()
    {
        float t = 0;
        while (t <= 1)
        {
            t += Time.deltaTime / AnimationSwitchColorDuration;
            float scl = 1.5f-(t*t*t)*0.5f;
            Cube.localScale = new Vector3(scl, scl, scl);
            yield return true;
        }
        Cube.localScale = new Vector3(1, 1, 1);
    }
}
