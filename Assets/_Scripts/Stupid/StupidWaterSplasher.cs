using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StupidWaterSplasher : MonoBehaviour
{

    public GameObject particleSplash;
    public GameObject particleSplashEmerge;

    private void OnTriggerEnter(Collider other)
    {
        Instantiate(particleSplash, new Vector3(other.transform.position.x, .1f, other.transform.position.z), Quaternion.Euler(-90,0,0));

        VoxelCharacterController motor;
        if ((motor = other.GetComponent<VoxelCharacterController>()) != null)
        {
            motor.isUnderwater = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject obj = Instantiate(particleSplashEmerge, new Vector3(other.transform.position.x, .1f, other.transform.position.z), Quaternion.Euler(-90, 0, 0));

        VoxelCharacterController motor;
        if ((motor = other.GetComponent<VoxelCharacterController>()) != null)
        {
            motor.isUnderwater = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.y < 0)
        {
            Instantiate(particleSplash, collision.contacts[0].point, Quaternion.identity);
        }
    }
}
