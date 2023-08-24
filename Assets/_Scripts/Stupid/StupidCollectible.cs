using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StupidCollectible : MonoBehaviour {

    public int value=1;

    public Color32 voxelColor;

    public Vector3 angularVelocity;
    public float bobbingScale=1;
    public float bobbingSpeed=1;
    Vector3 angle = Vector3.zero;
    Vector3 pos;
    float offset;

    bool homing;
    bool collected = false;
    Transform target;

    Vector3 homingDirection;
    public float homingAcceleration;
    float homingSpeed=0;

    public float maxHomingSpeed = 10;

    public Vector3 destination;
    public float startDuration = .5f;
    //AudioSource aS;

    private void Start()
    {
        GetComponent<MeshRenderer>().material.color = voxelColor;

        angle.y = Random.Range(0, 360f);
        pos = transform.localPosition;
        offset = pos.y;
        //aS = GetComponent<AudioSource>();
        StartCoroutine(MoveToDestination());
    }

    IEnumerator MoveToDestination()
    {
        float t = 0;
        Vector3 oldPos = transform.position;
        while (t < 1)
        {
            t += Time.deltaTime / startDuration;
            float t1 = t - 1;
            transform.position = Vector3.LerpUnclamped(oldPos, destination, t1*t1*t1+1);
            yield return true;
        }
        transform.position = destination;
    }

    void Update()
    {
        if (!collected)
        {
            angle += angularVelocity;
            transform.localEulerAngles = angle;

            if (!homing)
            {
                pos.y = Mathf.Sin(Time.time * bobbingSpeed) * bobbingScale + offset;
                //transform.localPosition = pos;
            }
            else
            {
                homingSpeed = Mathf.Min(homingSpeed + homingAcceleration * Time.deltaTime, maxHomingSpeed);
                transform.position += homingDirection * homingSpeed * Time.deltaTime;
                homingDirection = (homingDirection + (target.position - transform.position).normalized).normalized;
            }
        }
        else
        {
            //if (!aS.isPlaying) Destroy(gameObject);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            homing = true;
            target = col.transform;
            homingDirection = (target.position - transform.position).normalized;
        }
    }

    public void HitPlayer()
    {
        if (!collected && target!=null)
        {
            //aS.Play();
            target.GetComponent<PlayerInventoryController>().CollectVoxel(value, voxelColor);
            collected = true;
            GetComponent<MeshRenderer>().enabled = false;
        }
    }
}
