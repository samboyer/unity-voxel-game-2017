using SamBoyer.VoxelEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInteractionController : MonoBehaviour {

    enum InteractionMode{
        Unequipped,
        ItemHit,
        Place,
        Build
    }

    public float punchDistance = 2;
    [Range(0,1)]
    public float punchAngleCosine = .4f;

    public int punchDamage = 20;
    public int punchDamageVariation = 7;

    [Range(0, 1)]
    public float destructionYield = 0.2f;
    public float destructionYieldMaxVariation;

    public float modelDistance = 3;

    public LayerMask POIMask;
    private VoxelModel currentModel;
    public VoxelModel CurrentModel
    {
        get { return currentModel; }
        set
        {
            currentModel = value;
            wireframePreview.GetComponent<VoxelRenderer>().Model = currentModel;
        }
    }

    [Header("UI")]
    public Text textModelCost;
    public RectTransform buttonCreateModel;
    public RectTransform buttonEditCharacter;

    [Header("Sound")]
    public float sfxVolume = .25f;
    public AudioClip sfxNope;
    public AudioClip sfxPlace;
    public AudioClip sfxBreak;

    [Header("Object refs")]
    public AudioSource spacialAudioSource;
    public GameObject wireframePreview;

    public GameObject particlesPlaceDust;

    InteractionMode currentMode;

    Animator anim;
    VoxelCharacterController motor;
    PlayerInventoryController inventory;
    AudioSource audioSource;

    public CameraPivotController cameraMotor;

    void Start()
    {
        anim = transform.Find("MODEL").GetComponent<Animator>();
        motor = GetComponent<VoxelCharacterController>();
        inventory = GetComponent<PlayerInventoryController>();
        audioSource = GetComponent<AudioSource>();

        currentModel = VoxelModelLibrary.GetVoxelModel("beta_pillar", true);
        currentModel.voxelMesh.Update();
        currentMode = InteractionMode.Unequipped;
        //wireframePreview = GameObject.Find("WIREFRAME PREVIEW");
        wireframePreview.GetComponent<VoxelRenderer>().Model = currentModel;
        wireframePreview.SetActive(false);
        textModelCost.gameObject.SetActive(false);
        buttonCreateModel.gameObject.SetActive(false);
        buttonEditCharacter.gameObject.SetActive(false);

        PieMenuController.OnChoose += PieMenuController_OnChoose;
    }

    private void OnDestroy()
    {
        PieMenuController.OnChoose -= PieMenuController_OnChoose;
    }


    void Update () {
        Debug.DrawLine(transform.position, transform.position + anim.transform.forward * punchDistance);

        RaycastHit hit;

        switch (currentMode)
        {
            case InteractionMode.Unequipped:
                if (!Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(0))
                {
                    anim.SetTrigger("Punch");
                }
                break;
            case InteractionMode.Place:

                Ray r = new Ray(transform.position + anim.transform.forward * modelDistance + new Vector3(0, 100, 0), Vector3.down);
                //RaycastHit hit;
                if (Physics.Raycast(r, out hit, 1000, POIMask.value) && hit.transform.gameObject.layer == 9) {
                    wireframePreview.SetActive(true);
                    wireframePreview.transform.position = hit.point;
                    wireframePreview.transform.rotation = anim.transform.rotation;

                    if (!Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(0))
                    {
                        if(inventory.voxelsCarrying > currentModel.voxelCount)
                        {
                            hit.transform.GetComponent<TerrainChunk>().PlaceCustomObject(currentModel.modelName, hit.point, anim.transform.rotation);
                            inventory.voxelsCarrying -= currentModel.voxelCount;
                            spacialAudioSource.transform.position = hit.point;
                            spacialAudioSource.PlayOneShot(sfxPlace, sfxVolume);

                            particlesPlaceDust.transform.position = hit.point+Vector3.up;
                            particlesPlaceDust.GetComponent<ParticleSystem>().Emit(Random.Range(20, 30));
                        }
                        else
                        {
                            audioSource.PlayOneShot(sfxNope, sfxVolume);
                        }
                    }
                }else
                {
                    wireframePreview.SetActive(false);
                }
                break;
        }

    }

    /*private void OnGUI()
    {
        if (currentModel != null) GUI.Label(new Rect(0, 0, 300, 25), string.Format("Selected model: {0}, costs {1} voxels", currentModel.modelName, currentModel.voxelCount));

        //GUI.Label(new Rect(0, 25, 300, 25), string.Format("Voxels: {0}", inventory.voxelsCarrying));
    }*/


    void PieMenuController_OnChoose(int index)
    {
        switch (index)
        {
            case 0:
                currentMode = InteractionMode.Unequipped;
                break;
            case 1:
                currentMode = InteractionMode.Place;
                textModelCost.text = "Cost: " + currentModel.voxelCount;
                break;
            case 2:
                currentMode = InteractionMode.Build;
                break;
        }
        wireframePreview.SetActive(currentMode == InteractionMode.Place);
        textModelCost.gameObject.SetActive(currentMode == InteractionMode.Place);

        buttonCreateModel.gameObject.SetActive(currentMode==InteractionMode.Build);
        buttonEditCharacter.gameObject.SetActive(currentMode==InteractionMode.Build);

        cameraMotor.lockMouse = currentMode != InteractionMode.Build;
}

    public void DoPunch()
    {
        RaycastHit hit;
        Collider[] cols = Physics.OverlapSphere(transform.position + new Vector3(0, 2, 0), punchDistance);
        foreach (Collider col in cols)
        {
            if(col.gameObject.layer == 10)
            {
                Vector3 dir = (col.transform.position - transform.position).normalized;
                if(Vector3.Dot(anim.transform.forward, dir) > punchAngleCosine)
                {
                    WorldObject obj = col.transform.parent.GetComponent<WorldObject>();
                    obj.DoDamage(punchDamage + Random.Range(-punchDamageVariation, punchDamageVariation), destructionYield + Random.Range(-destructionYieldMaxVariation, destructionYieldMaxVariation));
                    spacialAudioSource.transform.position = col.transform.position;
                    spacialAudioSource.PlayOneShot(sfxBreak, sfxVolume);
                }
            }
        }
    }
}
