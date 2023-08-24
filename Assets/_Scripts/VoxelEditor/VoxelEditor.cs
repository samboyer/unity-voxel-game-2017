using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SamBoyer.VoxelEngine;
using UnityEngine.EventSystems;

using System.IO;

[RequireComponent(typeof (VoxelRenderer))]
public class VoxelEditor : MonoBehaviour
{
    VoxelRenderer rend;
    Transform selectionCube;

    byte selectedColorIndex;

    public bool useParticles = true;
    public int destructionParticlesCount = 8;
    ParticleSystem destructionParticles;
    ParticleSystem paintParticles;

    [Header("Sound")]
    public bool useSound = true;
    public AudioSource soundAdd;
    public AudioSource soundSubtract;
    public AudioSource soundPaint1;
    public AudioSource soundPaint2;
    public AudioSource soundColorPick;
    public AudioSource soundColorScroll;
    public float pitchVariation = 0.1f;

    [Header("TEMP")]
    Texture2D selectedColorTexture; //rly messy, idk why im bothering

    Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;

        rend = GetComponent<VoxelRenderer>();
        selectionCube = GameObject.Find("SELECTION").transform;
        dropdownContainer = GameObject.Find("DROPDOWN").GetComponent<RectTransform>();
        destructionParticles = GameObject.Find("DestructionParticles").GetComponent<ParticleSystem>();
        paintParticles = GameObject.Find("PaintParticles").GetComponent<ParticleSystem>();

        selectedColorIndex = 1;

        selectedColorTexture = new Texture2D(1, 1);
        selectedColorTexture.SetPixel(0, 0, Color.white);
        selectedColorTexture.Apply();


        Vector3 modelCenter = new Vector3(rend.Model.size.x / 2, rend.Model.size.z / 2, rend.Model.size.y / 2);
        transform.position = new Vector3(-modelCenter.x, 0, -modelCenter.z);
    }

    //RAYCASTING STUFF
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.mouseScrollDelta != Vector2.zero){ //COLOR INDEX SCROLL
            int i = selectedColorIndex + (int)Input.mouseScrollDelta.y;

            if (i < 1) { i += 255; }
            if (i > 255) { i -= 255; }

            selectedColorIndex = (byte)i;

            selectedColorTexture = MakeTextureFromColor();

            if (useSound)
            {
                soundColorScroll.Play();
            }
        }

        /*if (dropdownMoving)
        {
            dropdownContainer.anchoredPosition = new Vector2(0, Mathf.Min(-50, dropdownContainer.anchoredPosition.y + Input.GetAxis("Mouse Y") * dropdownAcceleration));
        }*/


        Vector3 pos2 = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Debug.DrawRay(pos2, mainCam.transform.forward * 10);

        RaycastHit hit;
        if (!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition), out hit))
        {
            if (hit.transform == transform)
            {
                int[] voxelCoord = GetVoxelFromRaycastHit(hit);

                selectionCube.localPosition = new Vector3(voxelCoord[0], voxelCoord[2], voxelCoord[1]);
                selectionCube.GetChild(0).localRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

                Debug.DrawRay(hit.point, hit.normal * 2);



                if (Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2)) //LMB (and not RMB or MMB for good measure)
                {
                    if (Input.GetKey(KeyCode.LeftShift)) //SUBTRACT VOXEL
                    {
                        int index = rend.Model.GetVoxelColorIndex(voxelCoord[0], voxelCoord[1], voxelCoord[2]);

                        rend.Model.SubtractVoxel(voxelCoord[0], voxelCoord[1], voxelCoord[2]);
                        rend.UpdateMesh();
                        if (useParticles)
                        {
                            destructionParticles.transform.localPosition = new Vector3(voxelCoord[0]+0.5f, voxelCoord[2]+0.5f, voxelCoord[1]+0.5f);
                            ParticleSystem.EmitParams p = new ParticleSystem.EmitParams();
                            byte[] col = rend.Model.palette.colors[index];
                            p.startColor = new Color32(col[0],col[1],col[2],255);
                            p.ResetVelocity();
                            destructionParticles.Emit(p, destructionParticlesCount);
                        }

                        if (useSound)
                        {
                            soundSubtract.pitch = 1 + ((Random.value * 2) - 1) * pitchVariation;
                            soundSubtract.Play();
                        }

                    }
                    else if(Input.GetKey(KeyCode.LeftControl)) //PAINT VOXEL
                    {
                        if (rend.Model.GetVoxelColorIndex(voxelCoord[0], voxelCoord[1], voxelCoord[2]) != selectedColorIndex) //don't bother if it's already that color
                        {
                            rend.Model.SetVoxelColorIndex(voxelCoord[0], voxelCoord[1], voxelCoord[2], selectedColorIndex);
                            rend.UpdateMesh();

                        }

                        if (useParticles)
                        {
                            paintParticles.transform.localPosition = new Vector3(voxelCoord[0] + 0.5f, voxelCoord[2] + 0.5f, voxelCoord[1] + 0.5f);
                            ParticleSystem.EmitParams p = new ParticleSystem.EmitParams();
                            byte[] col = rend.Model.palette.colors[selectedColorIndex];
                            p.startColor = new Color32(col[0], col[1], col[2], 150);
                            p.ResetVelocity();
                            paintParticles.Emit(p, destructionParticlesCount);
                        }

                        if (useSound)
                        {
                            if (Random.value > 0.5f)
                            {
                                soundPaint1.pitch = 1 + ((Random.value * 2) - 1) * pitchVariation;
                                soundPaint1.Play();
                            }else
                            {
                                soundPaint2.pitch = 1 + ((Random.value * 2) - 1) * pitchVariation;
                                soundPaint2.Play();
                            }
                        }
                    }
                    else if(Input.GetKey(KeyCode.LeftAlt)) //COLOR PICKER
                    {
                        selectedColorIndex = rend.Model.GetVoxelColorIndex(voxelCoord[0],voxelCoord[1],voxelCoord[2]);
                        selectedColorTexture = MakeTextureFromColor();

                        if (useSound)
                        {
                            soundColorPick.pitch = 1 + ((Random.value * 2) - 1) * pitchVariation;
                            soundColorPick.Play();
                        }
                    }
                    else //ADD VOXEL
                    {
                        //add the normal to this coord, to get the voxel space selected
                        voxelCoord[0] += Mathf.RoundToInt(hit.normal.x);
                        voxelCoord[1] += Mathf.RoundToInt(hit.normal.z); //  .\ /.
                        voxelCoord[2] += Mathf.RoundToInt(hit.normal.y); //     ^

                        rend.Model.AddVoxel(voxelCoord[0], voxelCoord[1], voxelCoord[2], selectedColorIndex);
                        rend.UpdateMesh();

                        if (useSound)
                        {
                            soundAdd.pitch = 1 + ((Random.value * 2) - 1) * pitchVariation;
                            soundAdd.Play();
                        }
                    }
                }
                

                /*if (Input.GetKey(KeyCode.LeftShift)) //REPLACE ENTIRE PALETTE COLOR
                {
                    print("Index to replace: " + Mathf.FloorToInt(hit.textureCoord.x * 256));
                    rend.Model.palette.colors[Mathf.FloorToInt(hit.textureCoord.x * 256)] = new byte[4] { 29, 252, 203, 255 };
                    rend.UpdatePalette();
                }*/
            }
        }
    }

    int[] GetVoxelFromRaycastHit(RaycastHit hit)
    {
        Vector3 localPosition = (hit.point - transform.position);

        localPosition.x += -0.5f * hit.normal.x; //if the coord is on this x/y/z edge, add/subtract a little to ensure it's in the right cube
        localPosition.y += -0.5f * hit.normal.y;
        localPosition.z += -0.5f * hit.normal.z;

        return new int[3] { Mathf.FloorToInt(localPosition.x), Mathf.FloorToInt(localPosition.z), Mathf.FloorToInt(localPosition.y) };
    }

    /*public void OpenModel()
    {
        string[] path = StandaloneFileBrowser.OpenFilePanel("Open model...", "", "vox", false);
        if (path[0] == "") return;
        print(path);
        rend.Model = VoxelModel.OpenVOXFile(path[0]);
        if (rend.Model != null)
        {
            rend.UpdateMesh();
            rend.UpdatePalette();
            Vector3 modelCenter = new Vector3(rend.Model.size.x / 2, rend.Model.size.z / 2, rend.Model.size.y / 2);
            transform.position = new Vector3(-modelCenter.x, 0, -modelCenter.z);
        }
    }*/

    /*public void SaveModel()
    {
        string path = StandaloneFileBrowser.SaveFilePanel("Save model...", "", "model", "vox");
        if (path == "") return;
        print(path);
        bool successful = rend.Model.SaveAsVOXFile(path);
        if (successful) { print("Saved!"); }
    }*/

    //UI STUFF
    Texture2D MakeTextureFromColor() {
        Texture2D tex = new Texture2D(1, 1);
        Color32 col = new Color32(rend.Model.palette.colors[selectedColorIndex][0], rend.Model.palette.colors[selectedColorIndex][1], rend.Model.palette.colors[selectedColorIndex][2], rend.Model.palette.colors[selectedColorIndex][3]);
        tex.SetPixel(0, 0, col);
        tex.Apply();
        return tex;
    }

    //TEMP
    private void OnGUI()
    {
        GUI.Label(new Rect(0, 0, 300, 50), string.Format("Selected Color Index: {0}",selectedColorIndex));
        GUI.DrawTexture(new Rect(0, 50, 25, 25), selectedColorTexture);

        GUI.Label(new Rect(Screen.width - 250, Screen.height - 180, 250, 180),

             @"VOXEL ENGINE EDITOR CONTROLS
Camera:
    RMB+Mouse: Rotate
    MMB+Mouse: Pan
    Scroll Wheel: Zoom
Editing:
    LMB: Add
    LShift+LMB: Subtract
    Ctrl+LMB: Paint
    Alt+LMB: Color Picker
    Ctrl+Scroll Wheel: Cycle Color Index");
    }

    public float dropdownAcceleration = 10;
    RectTransform dropdownContainer;
    bool isDropdownOpen = false;
    float dropdownTransitionDuration = 0.5f;
    float dropdownMaxHeight = 300;

    public void DropdownClick()
    {
        if (isDropdownOpen) StartCoroutine(DropdownClose());
        else StartCoroutine(DropdownOpen());
    }

    IEnumerator DropdownClose()
    {
        float end = -25;
        float start = dropdownContainer.anchoredPosition.y;
        float t=0;
        while(t<=1){
            t += Time.deltaTime / dropdownTransitionDuration;
            float t2 = t - 1;
            dropdownContainer.anchoredPosition = new Vector2(0, Mathf.Lerp(start, end, t2*t2*t2 + 1));
            yield return true;
        }
        isDropdownOpen = false;
    }

    IEnumerator DropdownOpen()
    {
        float start = -25;
        float end = -dropdownMaxHeight;
        float t = 0;
        while (t <= 1)
        {
            t += Time.deltaTime / dropdownTransitionDuration;
            float t2 = t - 1;
            dropdownContainer.anchoredPosition = new Vector2(0, Mathf.Lerp(start, end, t2 * t2 * t2 + 1));
            yield return true;
        }
        isDropdownOpen = true;
    }
}
