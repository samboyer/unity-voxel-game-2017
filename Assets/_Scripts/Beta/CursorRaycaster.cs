using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SamBoyer.VoxelEngine;

public class CursorRaycaster : MonoBehaviour {
    public int maxDistance;
    public LayerMask poiMask;

    Camera cam;
    GameObject player;
    PlayerInventoryController inventory;

    [Range(0, 1)]
    public float destructionYield=0.2f;
    public float destructionYieldMaxVariation;

    public static VoxelModel currentModel;
   
    private void Start()
    {
        cam = Camera.main;
        player = GameObject.FindGameObjectWithTag("Player");
        inventory = player.GetComponent<PlayerInventoryController>();
        currentModel = VoxelModelLibrary.GetVoxelModel("beta_pillar");
    }

    void Update () {
        Ray r = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(r, out hit, maxDistance, poiMask.value))
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetMouseButtonDown(0) && hit.transform.gameObject.layer == 10) //destruction
                {
                    hit.transform.parent.GetComponent<WorldObject>().DestroyObject(destructionYield + Random.Range(-destructionYieldMaxVariation, destructionYieldMaxVariation));
                }
                if (Input.GetMouseButtonDown(1) && hit.transform.gameObject.layer == 9) //placement
                {
                    if (inventory.voxelsCarrying > currentModel.voxelCount)
                    {
                        hit.transform.GetComponent<TerrainChunk>().PlaceCustomObject(currentModel.modelName, hit.point, Quaternion.LookRotation(Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up)));
                        inventory.voxelsCarrying -= currentModel.voxelCount;
                    }
                }
            }
        }
	}

    private void OnGUI()
    {
        if(currentModel!=null)GUI.Label(new Rect(0, 0, 300, 25), string.Format("Selected model: {0}, costs {1} voxels", currentModel.modelName, currentModel.voxelCount));

        GUI.Label(new Rect(0, 25, 300, 25), string.Format("Voxels: {0}", inventory.voxelsCarrying));
    }
}
