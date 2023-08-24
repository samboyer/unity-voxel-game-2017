using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StupidSelfDestruct : MonoBehaviour
{
    public float time;

    private void Start()
    {
        StartCoroutine(SelfDestruct());
    }
    IEnumerator SelfDestruct()
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
