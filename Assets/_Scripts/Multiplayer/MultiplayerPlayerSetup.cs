using UnityEngine;
using UnityEngine.Networking;

public class MultiplayerPlayerSetup : NetworkBehaviour {
    public Behaviour[] componentsToDisable;

    GameObject disconnectedCamera;

    private void Start()
    {
        if (!isLocalPlayer)
        {
            this.tag = "OtherPlayer";
            foreach(Behaviour b in componentsToDisable)
            {
                b.enabled = false;
            }
        }
        else
        {
            disconnectedCamera = GameObject.Find("SERVERONLYCAMERA");
            disconnectedCamera.SetActive(false);
        }
    }

    private void OnDisable()
    {
        disconnectedCamera.SetActive(true);
    }
}
