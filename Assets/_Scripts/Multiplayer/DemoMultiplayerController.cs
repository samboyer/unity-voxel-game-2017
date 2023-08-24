using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;

public class DemoMultiplayerController : MonoBehaviour {

    public GameObject PLAYERPREFAB;
    public Vector3 startPos;
    public Vector3 maxRandomOffset;

    private void Start()
    {
        Instantiate(PLAYERPREFAB, 
            new Vector3(startPos.x + Random.Range(-maxRandomOffset.x,maxRandomOffset.x),
            startPos.y + Random.Range(-maxRandomOffset.y, maxRandomOffset.y),
            startPos.z + Random.Range(-maxRandomOffset.z, maxRandomOffset.z)),
            Quaternion.identity);
    }
    private void Update()
    {
        print(Network.peerType);

    }
}
