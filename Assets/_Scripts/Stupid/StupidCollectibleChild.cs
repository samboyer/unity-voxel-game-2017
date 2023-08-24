
using UnityEngine;

public class StupidCollectibleChild : MonoBehaviour {

    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
            transform.parent.GetComponent<StupidCollectible>().HitPlayer();
    }
}
