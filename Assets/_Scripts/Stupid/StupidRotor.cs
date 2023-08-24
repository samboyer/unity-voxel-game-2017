using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StupidRotor : MonoBehaviour {
    public Vector3 AngularVelocityPerFrame;

    Vector3 angle = Vector3.zero;
	void Update () {
        angle += AngularVelocityPerFrame;
        transform.localEulerAngles = angle;
    }
}
