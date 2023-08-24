using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class ColourWheelController : MonoBehaviour {
    public GameObject colorIconPrefab;

    public Camera editorCamera;
    public Camera colorWheelCamera;
    public Image canvasOverlay;

    [SerializeField]
    float overlayTransitionDuration;
    [SerializeField]
    [Range(0,1)]
    float overlayAlpha;


    public LayerMask wheelLayerOnlyMask;

    VoxelEditorMulti editor;

    void Start () {
        editor = GameObject.Find("EDITOR").GetComponent<VoxelEditorMulti>();
        colorWheelCamera.gameObject.SetActive(false);
        PopulateWheel();
	}

    bool waitingForSelection = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            FinishColorWheel();

        if (waitingForSelection && Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray r = colorWheelCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(r, out hit, 100000, wheelLayerOnlyMask.value))
            {
                print(hit.transform.GetComponent<SpriteRenderer>().color);
                FinishColorWheel(hit.transform.GetComponent<SpriteRenderer>().color);
            }
        }
    }
	
    public void StartColorWheel()
    {
        editorCamera.GetComponent<EditorCameraPivotController>().enabled = false;
        editor.editingAllowed = false;
        colorWheelCamera.gameObject.SetActive(true);
        waitingForSelection = true;
        StartCoroutine(CanvasOverlayIn());
    }

    void FinishColorWheel(Color32 col)
    {
        GameObject.Find("EDITOR").GetComponent<VoxelEditorMulti>().AddColorToPalette(col.r, col.g, col.b);
        editorCamera.GetComponent<EditorCameraPivotController>().enabled = true;
        editor.editingAllowed = true;
        colorWheelCamera.gameObject.SetActive(false);
        waitingForSelection = false;
        StartCoroutine(CanvasOverlayOut());
    }

    void FinishColorWheel()
    {
        editorCamera.GetComponent<EditorCameraPivotController>().enabled = true;
        editor.editingAllowed = true;
        colorWheelCamera.gameObject.SetActive(false);
        waitingForSelection = false;
        StartCoroutine(CanvasOverlayOut());
    }

    void PopulateWheel()
    {
        //get the player's collected colours
        Color32[] colorsToMap = new Color32[PlayerInventoryController.collectedColors.Count];

        for(int i = 0; i < colorsToMap.Length; i++)
        {
            colorsToMap[i] = PlayerInventoryController.collectedColors[i].ToColor();
        }

        //put them in wheel
        foreach (Color32 c in colorsToMap)
        {
            float[] hsvCol = RGBToHSV(c);

            float angleRads = (hsvCol[0] / 360) * 2 * Mathf.PI;

            Vector3 colPos = new Vector3(Mathf.Sin(angleRads) * hsvCol[1], hsvCol[2] * 2 - 1, Mathf.Cos(angleRads) * hsvCol[1]);

            GameObject gO = Instantiate(colorIconPrefab, colPos, Quaternion.identity, transform);

            gO.GetComponent<SpriteRenderer>().color = c;
            gO.GetComponent<StupidCameraPointer>().cameraTransform = colorWheelCamera.transform;
        }
    }


    IEnumerator CanvasOverlayIn()
    {
        canvasOverlay.gameObject.SetActive(true);
        Color col = canvasOverlay.color;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / overlayTransitionDuration;
            float t2 = t - 1;

            col.a = Mathf.Lerp(0, overlayAlpha, t2 * t2 * t2 + 1);
            canvasOverlay.color = col;

            yield return true;
        }
    }

    IEnumerator CanvasOverlayOut()
    {
        Color col = canvasOverlay.color;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / overlayTransitionDuration;
            float t2 = t - 1;

            col.a = Mathf.Lerp(overlayAlpha, 0, t2 * t2 * t2 + 1);
            canvasOverlay.color = col;

            yield return true;
        }

        canvasOverlay.gameObject.SetActive(false);

    }

    public static float[] RGBToHSV(Color col)
    {
        //http://www.rapidtables.com/convert/color/rgb-to-hsv.htm <3
        float cMax = Mathf.Max(col.r, col.g, col.b);
        float cMin = Mathf.Min(col.r, col.g, col.b);
        float delta = cMax - cMin;

        float hue;
        if (delta == 0) hue = 0;
        else if (cMax == col.r) hue = 60 * (((col.g - col.b) / delta) % 6);
        else if (cMax == col.g) hue = 60 * ((col.b - col.r) / delta + 2);
        else hue = 60 * ((col.r - col.g) / delta + 4);

        float saturation = (cMax == 0) ? 0 : delta / cMax;

        return new float[] { hue, saturation, cMax };
    }

    public static Color HSVToRGB(float h, float s, float v)
    {
        float c = s * v;
        float x = c * (1 - Mathf.Abs((h / 60) % 2 - 1));
        float m = v - c;

        float r = 0, g = 0, b = 0;
        if (h < 60) { r = c; g = x; }
        else if (h < 120) { r = x; g = c; }
        else if (h < 180) { g = c; b = x; }
        else if (h < 240) { g = x; b = c; }
        else if (h < 300) { r = x; b = c; }
        else if (h < 360) { r = c; b = x; }

        return new Color(r + m, g + m, c + m);
    }
}
