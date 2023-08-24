using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PieMenuController : MonoBehaviour {

    public Image pieCenter;
    public Image pieCenterArc;
    public Canvas canvas;
    public Text iconNameText;
    public Image pieGlow;

    public Sprite[] items;
    public string[] names;
    int segments;
    public GameObject iconObj;
    public GameObject lineObj;

    public float growAngle = .005f;
    public float angleGrowLerp = .5f;

    public float openAnimDuration;
    public float closeAnimDuration;

    RectTransform[] icons;
    RectTransform[] lines;

    float individualAngle;

    bool open;
    Coroutine openClose;

    public delegate void OnChooseHandler(int index);
    public static event OnChooseHandler OnChoose;

    public static bool pieEnabled = true;

    void Start() {
        segments = items.Length;

        individualAngle = 1 / (float)segments * 360;

        lines = new RectTransform[segments];
        for (int l = 0; l < segments; l++)
        {
            lines[l] = Instantiate(lineObj, pieGlow.transform).GetComponent<RectTransform>();
            lines[l].localPosition = new Vector3(0, 0);
            lines[l].rotation = Quaternion.Euler(0,0, l * individualAngle + individualAngle/2);
        }
        icons = new RectTransform[segments];
        for (int i = 0; i < segments; i++)
        {
            icons[i] = Instantiate(iconObj, pieGlow.transform).GetComponent<RectTransform>();
            icons[i].localPosition = new Vector3(0, 0);
            icons[i].GetChild(0).GetComponent<Image>().sprite = items[i];
            icons[i].rotation = Quaternion.Euler(0, 0, i * individualAngle);
        }

        pieGlow.rectTransform.localScale = Vector3.zero;
        pieCenter.fillAmount = 0;

        //OpenPieMenu();
	}

    int selectedItem;

    void Update()
    {
        bool openNew = Input.GetKey(KeyCode.LeftControl);
        if (open && (!pieEnabled || !openNew))
        {
            if (openClose != null) StopCoroutine(openClose);
            openClose = StartCoroutine(PieCloseAnim());
            OnChoose(selectedItem);
        }
        if (pieEnabled && !open && openNew)
        {
            if (openClose != null) StopCoroutine(openClose);
            openClose = StartCoroutine(PieOpenAnim());
        }
        open = openNew;

        if (open)
        {

            Vector2 mousePos = Input.mousePosition;
            mousePos = mousePos - new Vector2(Screen.width / 2, Screen.height / 2);

            float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg - 90;
            pieCenterArc.rectTransform.rotation = Quaternion.Euler(0, 0, angle);

            //lines
            for (int l = 0; l < segments; l++)
            {
                float a = l * individualAngle + individualAngle / 2;
                float delta = Mathf.DeltaAngle(a, angle);
                delta = delta > 0 ? 180 - delta : -180 - delta;
                lines[l].rotation = Quaternion.Lerp(lines[l].rotation, Quaternion.Euler(0, 0, a + -delta * Mathf.Abs(delta) * growAngle), angleGrowLerp);
            }
            //icons
            for (int i = 0; i < segments; i++)
            {
                float a;
                if (i == segments - 1)
                    icons[i].rotation = Quaternion.Lerp(lines[i].rotation, lines[0].rotation, .5f);
                else
                    icons[i].rotation = Quaternion.Lerp(lines[i].rotation, lines[i + 1].rotation, .5f);

                icons[i].GetChild(0).rotation = Quaternion.identity;
            }

            selectedItem = Mathf.FloorToInt((Mathf.Repeat(angle, 360) - individualAngle / 2) / individualAngle);
            if (selectedItem < 0) selectedItem = segments-1;

            iconNameText.rectTransform.SetParent(icons[selectedItem].GetChild(0),false);
            iconNameText.rectTransform.localPosition = new Vector3(0, -100);
            iconNameText.text = names[selectedItem];
            //iconNameText.text = (Mathf.FloorToInt((Mathf.Repeat(angle,360) - individualAngle / 2) / individualAngle)).ToString();
        }
    }

    void OpenPieMenu()
    {
        StartCoroutine(PieOpenAnim());
    }

    IEnumerator PieOpenAnim()
    {
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime / openAnimDuration;
            float t1 = t - 1;
            t1 = t1 * t1 * t1 + 1;
            pieCenter.fillAmount = t1;
            pieGlow.rectTransform.localScale = new Vector3(t1,t1,t1);

            yield return true;
        }
        pieCenter.fillAmount = 1;
        pieGlow.rectTransform.localScale = Vector3.one;
    }

    IEnumerator PieCloseAnim()
    {
        float t = 0;
        Vector3 oldScale = pieGlow.rectTransform.localScale;
        float oldFill = pieCenter.fillAmount;
        while (t < 1)
        {
            t += Time.deltaTime / closeAnimDuration;
            float t1 = t * t * t;
            //float t1 = t - 1;
            //t1 = t1 * t1 * t1 + 1;
            pieCenter.fillAmount = Mathf.Lerp(oldFill,0,t1);
            pieGlow.rectTransform.localScale = Vector3.Lerp(oldScale, Vector3.zero, t1);

            yield return true;
        }
        pieCenter.fillAmount =0;
        pieGlow.rectTransform.localScale = Vector3.zero;
    }
}
