using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HsvMapper : MonoBehaviour {

    public int randColorCount;

    public Color[] colorsToMap;

    public GameObject colorIconPrefab;

    void Start() {
        colorsToMap = new Color[randColorCount];
        for(int i = 0; i < randColorCount; i++)
        {
            colorsToMap[i] = new Color(Random.value, Random.value, Random.value);
        }

        foreach(Color c in colorsToMap)
        {
            float[] hsvCol = RGBToHSV(c);

            float angleRads = (hsvCol[0]/360) * 2 * Mathf.PI;

            Vector3 colPos = new Vector3(Mathf.Sin(angleRads) * hsvCol[1], hsvCol[2]*2-1,Mathf.Cos(angleRads) * hsvCol[1]);

            GameObject gO = Instantiate(colorIconPrefab, colPos, Quaternion.identity);

            gO.GetComponent<SpriteRenderer>().color = c;
        }
    }

    public float[] RGBToHSV(Color col)
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

        return new float[] {hue, saturation, cMax};
    }
    
    public Color HSVToRGB(float h, float s, float v)
    {
        float c = s * v;
        float x = c * (1 - Mathf.Abs((h / 60) % 2 - 1));
        float m = v - c;

        float r = 0, g = 0, b= 0;
        if (h < 60) { r = c; g = x; }
        else if (h < 120) { r = x; g = c; }
        else if (h < 180) { g = c; b = x; }
        else if (h < 240) { g = x; b = c; }
        else if (h < 300) { r = x; b = c; }
        else if (h < 360) { r = c; b = x; }

        return new Color(r + m, g + m, c + m);
    }

}
