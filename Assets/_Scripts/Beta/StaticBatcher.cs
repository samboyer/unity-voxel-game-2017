using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticBatcher : MonoBehaviour {

    public bool updateNowTrigger = false;

    void Update() {
        if (updateNowTrigger)
        {
            StaticBatchingUtility.Combine(gameObject);
            updateNowTrigger = false;
        }
	}
}
