using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterFollower : MonoBehaviour {
    GameObject player;
    float yHeight;

	void Start () {
        player = GameObject.FindGameObjectWithTag("Player");
        yHeight = transform.position.y;
	}
	
	void Update () {
        if (player != null)
        {
            transform.position = new Vector3(player.transform.position.x, yHeight, player.transform.position.z);
        }
        else
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
    }
}
