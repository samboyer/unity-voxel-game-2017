using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SamBoyer.VoxelEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;


public class VoxelEditorMulti : MonoBehaviour
{
    Transform selectionCube;

    byte selectedColorIndex = 1;

    public bool editingAllowed = true;

    public bool useParticles = true;
    public int destructionParticlesCount = 8;


    [Header("Sound")]
    public bool useSound = true;
    public AudioSource soundAdd;
    public AudioSource soundSubtract;
    public AudioSource soundPaint1;
    public AudioSource soundPaint2;
    public AudioSource soundColorPick;
    public AudioSource soundColorScroll;
    public float pitchVariation = 0.1f;

    public float holdDelayTime = 0.3f;
    public float holdRepeatTime = 0.1f;

    public Transform sidePositiveX;
    public Transform sideNegativeX;
    public Transform sidePositiveY;
    public Transform sideNegativeY;



    [Header("TEMP")]
    Texture2D selectedColorTexture; //rly messy, idk why im bothering

    Camera mainCam;

    VoxelPalette palette;

    ParticleSystem destructionParticles;
    ParticleSystem paintParticles;

    void Start()
    {
        mainCam = Camera.main;
        palette = GameObject.Find("MODELS").transform.GetComponentInChildren<VoxelRenderer>().Model.palette;
        selectionCube = GameObject.Find("SELECTION").transform;
        dropdownContainer = GameObject.Find("DROPDOWN").GetComponent<RectTransform>();
        destructionParticles = GameObject.Find("DestructionParticles").GetComponent<ParticleSystem>();
        paintParticles = GameObject.Find("PaintParticles").GetComponent<ParticleSystem>();

        selectedColorTexture = new Texture2D(1, 1);
        selectedColorTexture.SetPixel(0, 0, Color.white);
        selectedColorTexture.Apply();

        PopulatePaletteUI();
    }

    float holdCurrentWait;

    void Update()
    {
        Cursor.lockState = CursorLockMode.None;

        //PALETTE SCROLL & DISPLAY
        if (Input.GetKey(KeyCode.LeftControl) && Input.mouseScrollDelta != Vector2.zero) { //COLOR INDEX SCROLL
            int i = selectedColorIndex + (int)Input.mouseScrollDelta.y;

            if (i < 1) { i += palette.numColors; }
            if (i > palette.numColors) { i -= palette.numColors; }

            SelectColorIndex((byte)i);
            //selectedColorIndex = (byte)i;

            selectedColorTexture = MakeTextureFromColor();

            /*if (useSound)
            {
                soundColorScroll.Play();
            }*/
        }

        //RAYCASTING & EDITING

        Vector3 pos2 = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Debug.DrawRay(pos2, mainCam.transform.forward * 10);

        RaycastHit hit;
        if (editingAllowed && !EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition), out hit))
        {
            //determine which model was hit
            VoxelRenderer clickedRenderer;
            if ((clickedRenderer = hit.transform.GetComponent<VoxelRenderer>()) != null)
            {
                selectionCube.gameObject.SetActive(true);

                int[] voxelCoord = GetVoxelFromRaycastHit(hit, clickedRenderer);
                selectionCube.localPosition = new Vector3(voxelCoord[0], voxelCoord[2], voxelCoord[1]);
                selectionCube.GetChild(0).localRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

                Debug.DrawRay(hit.point, hit.normal * 2);

                if (Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
                {
                    if (Input.GetKey(KeyCode.LeftControl)) //PAINT (CONTINUOUSLY)
                    {
                        if (clickedRenderer.Model.GetVoxelColorIndex(voxelCoord[0], voxelCoord[1], voxelCoord[2]) != selectedColorIndex) //don't bother if it's already that color
                        {
                            clickedRenderer.Model.SetVoxelColorIndex(voxelCoord[0], voxelCoord[1], voxelCoord[2], selectedColorIndex);
                            clickedRenderer.UpdateMesh();

                            if (useParticles)
                            {
                                //paintParticles.transform.localPosition = new Vector3(voxelCoord[0] + 0.5f, voxelCoord[2] + 0.5f, voxelCoord[1] + 0.5f);
                                ParticleSystem.EmitParams p = new ParticleSystem.EmitParams();
                                byte[] col = clickedRenderer.Model.palette.colors[selectedColorIndex];
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
                                }
                                else
                                {
                                    soundPaint2.pitch = 1 + ((Random.value * 2) - 1) * pitchVariation;
                                    soundPaint2.Play();
                                }
                            }
                        }
                    }

                    else
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            HandleClickEdit(clickedRenderer, voxelCoord, hit);
                        }
                        holdCurrentWait -= Time.deltaTime;

                        if (holdCurrentWait <= 0)
                        {
                            holdCurrentWait = holdRepeatTime;
                            HandleHoldEdit(clickedRenderer, voxelCoord, hit);
                        }
                    }
                }
                else holdCurrentWait = holdDelayTime;

            }

            else if (hit.transform.gameObject.name == "EDITORFLOORGRID") //if the floor was hit,
            {
                selectionCube.gameObject.SetActive(true);

                Vector3 localPosition = (hit.point - hit.transform.position);

                localPosition.x += -0.5f * hit.normal.x; //if the coord is on this x/y/z edge, add/subtract a little to ensure it's in the right cube
                localPosition.y += -0.5f * hit.normal.y;
                localPosition.z += -0.5f * hit.normal.z;

                int[] voxelCoord = { Mathf.FloorToInt(localPosition.x), Mathf.FloorToInt(localPosition.z), Mathf.FloorToInt(localPosition.y) };

                selectionCube.localPosition = new Vector3(voxelCoord[0], voxelCoord[2], voxelCoord[1]);
                selectionCube.GetChild(0).localRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

                if (Input.GetMouseButtonDown(0))
                {
                    {
                        //add the normal to this coord, to get the voxel space selected
                        voxelCoord[0] += Mathf.RoundToInt(hit.normal.x);
                        voxelCoord[1] += Mathf.RoundToInt(hit.normal.z); //  .\ /.
                        voxelCoord[2] += Mathf.RoundToInt(hit.normal.y); //     ^

                        voxelModels[0].AddVoxel(voxelCoord[0], voxelCoord[1], voxelCoord[2], selectedColorIndex);

                        renderers[0].UpdateMesh();

                        if (useSound)
                        {
                            soundAdd.pitch = 1 + ((Random.value * 2) - 1) * pitchVariation;
                            soundAdd.Play();
                        }
                    }
                }
            }

            else
            {
                selectionCube.gameObject.SetActive(false);
            }
        }
        else
        {
            //hide the indicator
            selectionCube.gameObject.SetActive(false);
        }
    }

    void HandleClickEdit(VoxelRenderer rend, int[] voxelCoord, RaycastHit hit)
    {
        if (Input.GetKey(KeyCode.LeftShift)) //SUBTRACT VOXEL
        {
            int index = rend.Model.GetVoxelColorIndex(voxelCoord[0], voxelCoord[1], voxelCoord[2]);

            rend.Model.SubtractVoxel(voxelCoord[0], voxelCoord[1], voxelCoord[2]);
            rend.UpdateMesh();
            if (useParticles)
            {
                //destructionParticles.transform.localPosition = new Vector3(voxelCoord[0]+0.5f, voxelCoord[2]+0.5f, voxelCoord[1]+0.5f);
                ParticleSystem.EmitParams p = new ParticleSystem.EmitParams();
                byte[] col = rend.Model.palette.colors[index];
                p.startColor = new Color32(col[0], col[1], col[2], 255);
                p.ResetVelocity();
                destructionParticles.Emit(p, destructionParticlesCount);
            }

            if (useSound)
            {
                soundSubtract.pitch = 1 + ((Random.value * 2) - 1) * pitchVariation;
                soundSubtract.Play();
            }

        }
        else if (Input.GetKey(KeyCode.LeftAlt)) //COLOR PICKER
        {
            SelectColorIndex(rend.Model.GetVoxelColorIndex(voxelCoord[0], voxelCoord[1], voxelCoord[2]));
            //selectedColorIndex = rend.Model.GetVoxelColorIndex(voxelCoord[0],voxelCoord[1],voxelCoord[2]);
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

    void HandleHoldEdit(VoxelRenderer rend, int[] voxelCoord, RaycastHit hit)
    {

        if (Input.GetKey(KeyCode.LeftShift)) //SUBTRACT VOXEL
        {
            int index = rend.Model.GetVoxelColorIndex(voxelCoord[0], voxelCoord[1], voxelCoord[2]);

            rend.Model.SubtractVoxel(voxelCoord[0], voxelCoord[1], voxelCoord[2]);
            rend.UpdateMesh();
            if (useParticles)
            {
                //destructionParticles.transform.localPosition = new Vector3(voxelCoord[0]+0.5f, voxelCoord[2]+0.5f, voxelCoord[1]+0.5f);
                ParticleSystem.EmitParams p = new ParticleSystem.EmitParams();
                byte[] col = rend.Model.palette.colors[index];
                p.startColor = new Color32(col[0], col[1], col[2], 255);
                p.ResetVelocity();
                destructionParticles.Emit(p, destructionParticlesCount);
            }

            if (useSound)
            {
                soundSubtract.pitch = 1 + ((Random.value * 2) - 1) * pitchVariation;
                soundSubtract.Play();
            }
        }
    }


    int[] GetVoxelFromRaycastHit(RaycastHit hit, VoxelRenderer rend) //Z-up
    {
        Vector3 localPosition = (hit.point + rend.Model.modelCenter - hit.transform.position);

        localPosition.x += -0.5f * hit.normal.x; //if the coord is on this x/y/z edge, add/subtract a little to ensure it's in the right cube
        localPosition.y += -0.5f * hit.normal.y;
        localPosition.z += -0.5f * hit.normal.z;

        return new int[3] { Mathf.FloorToInt(localPosition.x), Mathf.FloorToInt(localPosition.z), Mathf.FloorToInt(localPosition.y) };
    }

    public GameObject editorModelPrefab;

    public static VoxelModel[] voxelModels;

    VoxelRenderer[] renderers;

    //Vector3 collectionCenter;

    bool singleModelMode = false;

    public void LoadModelsToEdit(VoxelModel[] models, Vector3 collectionCenter)
    {
        renderers = new VoxelRenderer[models.Length];
        voxelModels = models;
        //this.collectionCenter = collectionCenter;
        Transform modelsParent = GameObject.Find("MODELS").transform;
        modelsParent.position = -collectionCenter;

        for (int i = 0; i < models.Length; i++)
        {
            VoxelRenderer rend = Instantiate(editorModelPrefab, models[i].modelCenter - collectionCenter, Quaternion.identity, modelsParent).GetComponent<VoxelRenderer>();
            rend.Model = models[i];
            rend.UpdatePalette();
            renderers[i] = rend;
        }

        Transform floor = GameObject.Find("EDITORFLOORGRID").transform;
        floor.localScale = new Vector3(models[0].size.x, 1, models[0].size.y);
        //floor.localPosition = new Vector3(-models[0].size.x / 2, 0, -models[0].size.z / 2);
    }

    public void LoadModelToEdit(VoxelModel model)
    {
        singleModelMode = true;

        renderers = new VoxelRenderer[1];
        voxelModels = new VoxelModel[] { model };
        //this.collectionCenter = collectionCenter;
        Transform modelsParent = GameObject.Find("MODELS").transform;

        Vector3 modelCenterOffset = new Vector3(-model.size.x / 2f, 0, -model.size.y / 2f);

        modelsParent.position = modelCenterOffset;

        VoxelRenderer rend = Instantiate(editorModelPrefab, -modelCenterOffset, Quaternion.identity).GetComponent<VoxelRenderer>();
        rend.transform.SetParent(modelsParent, false);
        rend.Model = model;
        rend.UpdatePalette();
        renderers[0] = rend;

        PlaceFloorGrid();
    }

    void PlaceFloorGrid()
    {
        Transform floor = GameObject.Find("EDITORFLOORGRID").transform;
        floor.localScale = new Vector3(voxelModels[0].size.x, 1, voxelModels[0].size.y);

        sidePositiveX.position = new Vector3(voxelModels[0].size.x / 2f,0,0);
        sideNegativeX.position = new Vector3(-voxelModels[0].size.x / 2f,0,0);
        sidePositiveY.position = new Vector3(0,0,voxelModels[0].size.y / 2f);
        sideNegativeY.position = new Vector3(0,0,-voxelModels[0].size.y / 2f);
    }


    public void ExpandModelDimensions(int direction)
    {
        voxelModels[0].ExpandDimensions(1, (VoxelModel.Direction)direction);
        renderers[0].UpdateMesh();
        PlaceFloorGrid();

        GameObject.Find("MODELS").transform.position = new Vector3(-voxelModels[0].size.x / 2f, 0, -voxelModels[0].size.y / 2f);
    }

    public void ContractModelDimensions(int direction)
    {
        voxelModels[0].ExpandDimensions(-1, (VoxelModel.Direction)direction);
        renderers[0].UpdateMesh();
        PlaceFloorGrid();

        GameObject.Find("MODELS").transform.position = new Vector3(-voxelModels[0].size.x / 2f, 0, -voxelModels[0].size.y / 2f);
    }

    //UI STUFF
    Texture2D MakeTextureFromColor() {
        Texture2D tex = new Texture2D(1, 1);
        Color32 col = palette.MakeColorFromIndex(selectedColorIndex);
        tex.SetPixel(0, 0, col);
        tex.Apply();
        return tex;
    }

    private void OnGUI()
    {
        if (singleModelMode)
        {
            GUI.Label(new Rect(0, 0, 200, 25), "Voxel count: " + voxelModels[0].voxelCount);
        }
    }

    public float dropdownAcceleration = 10;
    RectTransform dropdownContainer;
    bool isDropdownOpen = false;
    float dropdownTransitionDuration = 0.5f;
    public float dropdownHeight = 125;

    public void DropdownClick()
    {
        if (isDropdownOpen) StartCoroutine(DropdownClose());
        else StartCoroutine(DropdownOpen());
    }

    IEnumerator DropdownClose()
    {
        float end = 0;
        float start = dropdownContainer.anchoredPosition.y;
        float t = 0;
        while (t <= 1) {
            t += Time.deltaTime / dropdownTransitionDuration;
            float t2 = t - 1;
            dropdownContainer.anchoredPosition = new Vector2(0, Mathf.Lerp(start, end, t2 * t2 * t2 + 1));
            yield return true;
        }
        isDropdownOpen = false;
    }

    IEnumerator DropdownOpen()
    {
        float start = 0;
        float end = -dropdownHeight;
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

    public RectTransform paletteScrollContentBox;
    public RectTransform paletteButtonsContainer;
    public GameObject PaletteUIItemPrefab;

    public RectTransform selectedColorSprite;


    static float colorButtonWidth = 80;

    void PopulatePaletteUI() {
        paletteScrollContentBox.sizeDelta = new Vector2(palette.colors.Length * colorButtonWidth, 0);

        for (int i = 1; i < palette.colors.Length; i++)
        {
            byte[] col = palette.colors[i];

            GameObject gO = Instantiate(PaletteUIItemPrefab, paletteButtonsContainer, false);

            gO.GetComponent<RectTransform>().anchoredPosition = new Vector2((i - 1) * colorButtonWidth, 0);
            Color32 color = new Color32(col[0], col[1], col[2], col[3]);
            gO.GetComponent<Image>().color = color;
            SetButtonOnClick(gO.GetComponent<Button>(), (byte)i);
        }
    }

    void SetButtonOnClick(Button btn, byte arg)
    {
        btn.onClick.AddListener(() => SelectColorIndex(arg));
    }

    Coroutine spriteMoveCoroutine;

    public void SelectColorIndex(byte index)
    {
        selectedColorIndex = index;

        if (spriteMoveCoroutine != null) StopCoroutine(spriteMoveCoroutine);
        spriteMoveCoroutine = StartCoroutine(MoveSelectedColorSpriteToIndex());

        if (useSound)
        {
            soundColorScroll.Play();
        }
    }

    public void OpenColorWheel()
    {
        GameObject.Find("COLOURWHEEL").GetComponent<ColourWheelController>().StartColorWheel();
    }

    public void AddColorButton()
    {
        byte r = byte.Parse(GameObject.Find("AddColorR").GetComponent<InputField>().text);
        byte g = byte.Parse(GameObject.Find("AddColorG").GetComponent<InputField>().text);
        byte b = byte.Parse(GameObject.Find("AddColorB").GetComponent<InputField>().text);

        AddColorToPalette(r, g, b);
    }

    public void AddColorToPalette(byte r, byte g, byte b)
    {
        if (palette.numColors == 255) return;

        foreach (byte[] col in palette.colors) //check if it's not already in .-.
        {
            if (col[0] == r && col[1] == g && col[2] == b) return;
        }

        palette.AddColor(r, g, b);

        foreach (VoxelRenderer rend in renderers)
        {
            rend.UpdatePalette();
        }

        //add the new button to UI
        paletteScrollContentBox.sizeDelta = new Vector2(palette.numColors * colorButtonWidth, 0);

        GameObject gO = Instantiate(PaletteUIItemPrefab, paletteButtonsContainer, false);

        gO.GetComponent<RectTransform>().anchoredPosition = new Vector2((palette.numColors - 1) * colorButtonWidth, 0);
        Color32 color = new Color32(r, g, b, 255);
        gO.GetComponent<Image>().color = color;
        SetButtonOnClick(gO.GetComponent<Button>(), (byte)palette.numColors);

        SelectColorIndex((byte)palette.numColors);
    }

    public float selectedColorSpriteMoveDuration = 0.25f;

    IEnumerator MoveSelectedColorSpriteToIndex()
    {
        float start = selectedColorSprite.anchoredPosition.x;
        float end = (selectedColorIndex - 1) * colorButtonWidth;

        float t = 0;
        while (t <= 1)
        {
            t += Time.deltaTime / selectedColorSpriteMoveDuration;
            float t2 = t - 1;
            selectedColorSprite.anchoredPosition = new Vector2(Mathf.Lerp(start, end, t2 * t2 * t2 + 1), 0);
            yield return true;
        }
    }
}
