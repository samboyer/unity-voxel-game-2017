using SamBoyer.VoxelEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class WorldSessionController : MonoBehaviour {

    public static string worldName;

    public static SaveFileManager file;
    WorldTerrainGenerator worldTerrainGen;

    Transform player;

    public static bool firstLoad;

    public float AutosavePeriod = 30;

    public Camera screencapCamera;
    public GameObject UIToDisable;

    void Awake () {
        worldTerrainGen = WorldTerrainGenerator.current;

#if UNITY_EDITOR
        if (file!= null){
            InitialiseWorldFromSave();
        }else
        {
            //Wait to put player on the floor (so they dont fall through)
            player = GameObject.FindGameObjectWithTag("Player").transform;
            player.GetComponent<VoxelCharacterController>().enabled = false;
            StartCoroutine(WaitToActivatePlayer(player.position));
        }
#else
        InitialiseWorldFromSave();
#endif
        StartCoroutine(AutosaveCoroutine());
	}

    public static void StartSession(string safeName, bool firstTimeLoaded)
    {
        file = new SaveFileManager(safeName);
        worldName = safeName;
        SceneManager.LoadScene("World");
        firstLoad = firstTimeLoaded;
    }

    void InitialiseWorldFromSave()
    {

        SaveFileObjects.SaveFileWorldState worldState = file.worldState;
        if (worldState != null)
        {
            WorldTerrainGenerator.current.seed = worldState.seed;
        }

        SaveFileObjects.SaveFilePlayerState playerState = file.LoadPlayerData();
        if (playerState != null) {
            player = GameObject.FindGameObjectWithTag("Player").transform;

            //PLAYER POSITIONING
            player.position = new Vector3(playerState.posX, playerState.posY, playerState.posZ);
            //player.rotation = Quaternion.Euler(playerState.rotX, playerState.rotY, playerState.rotZ);
            player.rotation = Quaternion.Euler(0, playerState.rotY, 0);

            CameraPivotController pivotController = player.Find("CAMERAPIVOT").GetChild(0).GetComponent<CameraPivotController>();

            pivotController.xRot = playerState.camRotX;
            pivotController.yRot = playerState.camRotY;
            pivotController.zoom = playerState.camZoom;

            //INVENTORY INITIALISATION
            PlayerInventoryController inv = player.GetComponent<PlayerInventoryController>();
            inv.voxelsCarrying = playerState.voxels;

            PlayerInventoryController.collectedColors = new List<byte[]>();
            for(int i = 0; i < playerState.collectedColors.Length; i++)
            {
                PlayerInventoryController.collectedColors.Add(Extensions.ColorBytesFromHexString(playerState.collectedColors[i]));
            }
            inv.SortColors(); //...whats that

            //Wait to put player on the floor (so they dont fall through)
            player.GetComponent<VoxelCharacterController>().enabled = false;
            StartCoroutine(WaitToActivatePlayer(player.position));
        }

        //LOAD PLAYERMODEL
        Transform playerModel = GameObject.FindGameObjectWithTag("Player").transform.Find("MODEL");
        VoxelModel mdl;

        VoxelPalette pal = new VoxelPalette();

        if ((mdl = file.OpenVoxelModel("betaPlayer_torso")) != null)
        {
            pal = mdl.palette;
            playerModel.GetChild(0).GetComponent<VoxelRenderer>().Model = mdl; //torso
        }

        if ((mdl = file.OpenVoxelModel("betaPlayer_head")) != null)
        {
            mdl.palette = pal;
            playerModel.GetChild(0).GetChild(0).GetComponent<VoxelRenderer>().Model = mdl; //head
        }

        if ((mdl = file.OpenVoxelModel("betaPlayer_armL")) != null)
        {
            mdl.palette = pal;
            playerModel.GetChild(0).GetChild(1).GetComponent<VoxelRenderer>().Model = mdl; //armL
        }

        if ((mdl = file.OpenVoxelModel("betaPlayer_armR")) != null)
        {
            mdl.palette = pal;
            playerModel.GetChild(0).GetChild(2).GetComponent<VoxelRenderer>().Model = mdl; //armR
        }

        if ((mdl = file.OpenVoxelModel("betaPlayer_legL")) != null)
        {
            mdl.palette = pal;
            playerModel.GetChild(0).GetChild(3).GetComponent<VoxelRenderer>().Model = mdl; //legL
        }

        if ((mdl = file.OpenVoxelModel("betaPlayer_legR")) != null)
        {
            mdl.palette = pal;
            playerModel.GetChild(0).GetChild(4).GetComponent<VoxelRenderer>().Model = mdl; //legR
        }

        print(WorldTerrainGenerator.current.seed);
    }

    //wait for first chunk to be placed before activating player component? bit of a hack for now
    IEnumerator WaitToActivatePlayer(Vector3 playerPos)
    {
        //calculate required chunk
        ChunkCoord coord = new ChunkCoord(Mathf.FloorToInt(playerPos.x / worldTerrainGen.chunkSize), Mathf.FloorToInt(playerPos.z / worldTerrainGen.chunkSize));

        int requiredRes = worldTerrainGen.worldGenState.maxResolution;

        while (worldTerrainGen.CheckCurrentResolutionOfChunk(coord) != requiredRes) yield return new WaitForSeconds(0.1f);

        if (firstLoad)
        {
            Vector3 pos = player.position;
            pos.y = worldTerrainGen.loadedChunks[coord].GetHeightAtLocalPoint(pos.x, pos.z); //shouldn't that be a local space?
            player.position = pos;
        }

        player.GetComponent<VoxelCharacterController>().enabled = true;
    }

	
	public void SaveWorld()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player").transform;
        Vector3 eulerRot = player.rotation.eulerAngles;

        CameraPivotController pivotController = player.Find("CAMERAPIVOT").GetChild(0).GetComponent<CameraPivotController>();

        /*SaveFileObjects.SaveFileWorldState worldState = new SaveFileObjects.SaveFileWorldState
        {
            seed = worldTerrainGen.seed,
            worldTime = Time.time,
            
        };*/

        PlayerInventoryController inv = player.GetComponent<PlayerInventoryController>();

        string[] colStrs = new string[PlayerInventoryController.collectedColors.Count];
        for(int i=0;i< colStrs.Length;i++)
        {
            colStrs[i] = Extensions.HexStringFromBytes(PlayerInventoryController.collectedColors[i]);
        }

        SaveFileObjects.SaveFilePlayerState playerState = new SaveFileObjects.SaveFilePlayerState
        {
            name = "player",
            posX = player.position.x,
            posY = player.position.y,
            posZ = player.position.z,
            rotX = eulerRot.x,
            rotY = eulerRot.y,
            rotZ = eulerRot.z,
            camRotX = pivotController.xRot,
            camRotY = pivotController.yRot,
            camZoom = pivotController.zoom,
            voxels = inv.voxelsCarrying,
            collectedColors = colStrs,
            //collectedColors = new byte[] { 25, 30, 35, 255 },
        };

        file.SavePlayerData(playerState);
        file.WriteWorldData();

        //save dirty chunk data
        worldTerrainGen.SaveAllDirtyChunks();

        if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="World")
            StartCoroutine(Screencap());

        print("Saved!");
    }

    IEnumerator Screencap()
    {

        yield return new WaitForEndOfFrame();
        UIToDisable.SetActive(false);

        Texture2D tex = new Texture2D(400,300, TextureFormat.ARGB32, false);

        RenderTexture renderTexture = new RenderTexture(400,300, 24, RenderTextureFormat.ARGB32);

        RenderTexture.active = renderTexture;
        screencapCamera.targetTexture = renderTexture;
        screencapCamera.Render();

        tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        tex.Apply();

        //release
        RenderTexture.active = null;
        screencapCamera.targetTexture = null;

        file.SaveWorldScreenshot(tex);

        UIToDisable.SetActive(true);

        /*tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);


        tex.Apply();
        file.SaveWorldScreenshot(tex);*/
    }

    public void SaveWorldAndExit()
    {
        SaveWorld();
        file.CloseSaveFile();
        Time.timeScale = 1;
        PauseMenuController.isPaused = false;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("Menu");
    }


    VoxelModel[] CollectCharacterModels()
    {
        Transform playerModel = GameObject.FindGameObjectWithTag("Player").transform.Find("MODEL");
        VoxelModel[] models = new VoxelModel[6];
        models[0] = playerModel.GetChild(0).GetComponent<VoxelRenderer>().Model; //torso
        models[1] = playerModel.GetChild(0).GetChild(0).GetComponent<VoxelRenderer>().Model;
        models[2] = playerModel.GetChild(0).GetChild(1).GetComponent<VoxelRenderer>().Model;
        models[3] = playerModel.GetChild(0).GetChild(2).GetComponent<VoxelRenderer>().Model;
        models[4] = playerModel.GetChild(0).GetChild(3).GetComponent<VoxelRenderer>().Model;
        models[5] = playerModel.GetChild(0).GetChild(4).GetComponent<VoxelRenderer>().Model;
        return models;
    }

    IEnumerator AutosaveCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(AutosavePeriod);
#if UNITY_EDITOR
            if(file!=null) SaveWorld();
#else
            SaveWorld();
#endif

            print("Autosaved!");
        }
    }
}
