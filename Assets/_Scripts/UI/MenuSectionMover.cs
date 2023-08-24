using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class MenuSectionMover : MonoBehaviour
{
    public Interpolate.EaseType easeInType, easeOutType;
    public float easeInDuration=0.5f, easeOutDuration=0.5f;
    public PopupDirection easeInDirection, easeOutDirection;

    private RectTransform rt;

    public bool openOnStart = false;
    Vector2 originalPosition;

    void Start()
    {
        rt = GetComponent<RectTransform>();
        originalPosition = rt.anchoredPosition;
        //gameObject.SetActive (false);
        if (openOnStart)
        {
            OpenSection();
        }
    }

    public void OpenSection()
    {
        gameObject.SetActive(true);
        StartCoroutine(MovePopupIn());
    }

    private IEnumerator MovePopupIn()
    {
        if (easeInDirection == PopupDirection.scale)
        {
            rt.anchoredPosition = Vector2.zero;
        }

        float t = 0, startTime = Time.unscaledTime;
        Interpolate.Function Ease = Interpolate.Ease(easeInType);

        Vector2 startPos = Vector2.zero, endPos = Vector2.zero;

        switch (easeInDirection)
        {
            case PopupDirection.top:
                startPos = new Vector2(0, Screen.height);
                break;
            case PopupDirection.bottom:
                startPos = new Vector2(0, -Screen.height);
                break;
            case PopupDirection.left:
                startPos = new Vector2(-Screen.width, 0);
                break;
            case PopupDirection.right:
                startPos = new Vector2(Screen.width, 0);
                break;
        }

        while (t <= easeInDuration)
        {
            t = Time.unscaledTime - startTime;
            if (easeInDirection != PopupDirection.scale)
            {
                rt.anchoredPosition = Vector2.Lerp(startPos, endPos, Ease(0, 1, t, easeInDuration));
            }
            else
            {
                rt.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, Ease(0, 1, t, easeInDuration));
            }

            yield return true;
        }
    }

    public void CloseSection()
    {
        StartCoroutine(MovePopupOut());
    }

    private IEnumerator MovePopupOut()
    {
        float t = 0, startTime = Time.unscaledTime;
        Interpolate.Function Ease = Interpolate.Ease(easeOutType);

        Vector2 startPos = Vector2.zero, endPos = Vector2.zero;

        switch (easeOutDirection)
        {
            case PopupDirection.top:
                endPos = new Vector2(0, Screen.height);
                break;
            case PopupDirection.bottom:
                endPos = new Vector2(0, -Screen.height);
                break;
            case PopupDirection.left:
                endPos = new Vector2(-Screen.width, 0);
                break;
            case PopupDirection.right:
                endPos = new Vector2(Screen.width, 0);
                break;
        }

        while (t <= easeOutDuration)
        {
            t = Time.unscaledTime - startTime;
            if (easeOutDirection != PopupDirection.scale)
            {
                rt.anchoredPosition = Vector2.Lerp(startPos, endPos, Ease(0, 1, t, easeOutDuration));
            }
            else
            {
                rt.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, Ease(0, 1, t, easeOutDuration));
            }

            yield return true;
        }
        //gameObject.SetActive (false);
        rt.anchoredPosition = originalPosition;
    }
    public enum PopupDirection
    {
        top,
        bottom,
        left,
        right,
        scale
    }
    public static PopupDirection SwapPopupDirection(PopupDirection dir)
    {
        if (dir == PopupDirection.top) return PopupDirection.bottom;
        if (dir == PopupDirection.left) return PopupDirection.right;
        if (dir == PopupDirection.right) return PopupDirection.left;
        return PopupDirection.top;
    }
}

