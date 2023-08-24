using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//using System.Threading;
//using System.Linq;

public class WorldTerrainGenerator : MonoBehaviour {

    public static WorldTerrainGenerator current;

    public int seed;

    public float chunkMaxDistance = 20000;
    public int chunkSize;

    //public int maxResolution = 1;
    public float resolutionFalloffMinDistance;
    public float resolutionFalloff;

    public float foliageRenderDistance;

    public GameObject chunkPrefab;

    public WorldGenState worldGenState;

    public float chunkGenCriticalTime = 0.015f;
    public float chunkCheckPausePeriod = 1;
    public float chunkCheckRestartDistance = 512;
    public float chunkCheckMinDistance = 64;

    public Dictionary<ChunkCoord, TerrainChunk> loadedChunks = new Dictionary<ChunkCoord, TerrainChunk>();

    GameObject player;

    Vector3 currentPlayerPos;

    Vector3 lastChunkCheckPlayerPos = Vector3.zero;
    int currentChunkCheck = 0;

    public bool ignoreQualitySettings = false;

    private void Awake()
    {
        if (current != null) print("uh oh, 2 worldgens .-.");
        current = this;
    }
    private void OnDestroy()
    {
        if (current == this) current = null;
    }

    void Start()
    {
        if(!ignoreQualitySettings)
            ApplyWorldGenQualitySettings();

        //seed = WorldSessionController.file.worldState.seed;

        //loadedChunks = new Dictionary<ChunkCoord, TerrainChunk>();
        NoiseGenerator.RebuildHash(seed);

        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            currentPlayerPos = lastChunkCheckPlayerPos = player.transform.position;
        }
        StartCoroutine(PeriodicChunkCheck());
    }

    void ApplyWorldGenQualitySettings()
    {
        GameQualityControls.QualityState state = GameQualityControls.currentState;

        this.chunkMaxDistance = state.worldGenMaxDistance;
        this.resolutionFalloffMinDistance = state.worldGenHiResDistance;
        this.resolutionFalloff = state.worldGenResolutionFalloff;
        this.foliageRenderDistance = state.worldGenFoliageMaxDistance;
    }


    void Update()
    {
        if (player != null)
        {
            currentPlayerPos = player.transform.position;
        }
        else
        {
            currentPlayerPos = Vector3.zero;
            player = GameObject.FindGameObjectWithTag("Player");
        }
    }

    IEnumerator PeriodicChunkCheck()
    {
        while (true)
        {
            yield return CheckChunksSpiral();
            yield return new WaitForSeconds(chunkCheckPausePeriod);
        }
    }

    bool initialChunkGenDone = false;

    bool firstGen = true;

    IEnumerator CheckChunksSpiral()
    {
        float playerXPos = currentPlayerPos.x, playerYPos = currentPlayerPos.z;

        float deltaPosX = playerXPos - lastChunkCheckPlayerPos.x, deltaPosY = playerYPos - lastChunkCheckPlayerPos.z;

        float distSinceLastChunkCheck = Mathf.Sqrt(deltaPosX * deltaPosX + deltaPosY * deltaPosY);

        if (distSinceLastChunkCheck < chunkCheckMinDistance && initialChunkGenDone) //if it's already been done once and player hasn't moved much, don't bother
        {
            if (firstGen)
            {
                firstGen = false;
                print("first WorldGen spiral done in " + Time.time + " seconds");
            }
            yield break;
        }

        lastChunkCheckPlayerPos = currentPlayerPos;

        int ix = Mathf.FloorToInt(playerXPos / chunkSize), iy = Mathf.FloorToInt(playerYPos / chunkSize); //x,y coords, initialised to the containing chunk of the character
        int dirx = 0, diry = 1; //x,y spiral line direction

        int linepos = 0, spiralLineLength = 1;
        bool increaseLengthNextTime = false;

        //print("Chunk containing player: " + ix + ", " + iy);

        float iterationTime = Time.realtimeSinceStartup;

        float breakCornerDistance = Mathf.Sqrt(chunkMaxDistance * chunkMaxDistance * 2); //the maximum distance the spiral will continue for before breaking


        int chunksMade = 0;

        while (true)
        {
            float xOffset = (ix * chunkSize) - playerXPos, yOffset = (iy * chunkSize) - playerYPos; //find out world 2D offset from chunk corner to player

            float sqrDist = (xOffset * xOffset + yOffset * yOffset);
            float distToPlayer = Mathf.Sqrt(sqrDist); //world distance from chunk corner to player

            if (distToPlayer <= chunkMaxDistance) //if close enough,
            {
                //CALCULATE REQUIRED RESOLUTION 
                float t = Mathf.Clamp01((distToPlayer - resolutionFalloffMinDistance) * resolutionFalloff) - 1; //transition factor between max res and min res
                int newResolution = Mathf.ClosestPowerOfTwo((int)Mathf.Lerp(worldGenState.maxResolution, 1, t * t * t * t * t + 1)); //nearest power of two, with quintic falloff

                ChunkCoord coord = new ChunkCoord(ix, iy);

                TerrainChunk chunk;

                if (loadedChunks.TryGetValue(coord, out chunk))
                {
                    chunk.lastChunkCheck = currentChunkCheck;

                    if (chunk.currentMeshResolution != newResolution) //upgrade/downgrade
                    {
                        chunk.UpdateTerrain(newResolution);
                    }

                    if (distToPlayer > foliageRenderDistance)
                    {
                        chunk.DestroyAllObjects();
                    }
                    else
                    {
                        chunk.PlaceAllObjects();
                    }
                }
                else //chunk needs to be instantiated
                {
                    BuildChunk(coord, newResolution);
                    chunksMade++;
                }
            }

            //LOOP STOPPING CONDITION
            if (distToPlayer > breakCornerDistance) {
                break;
            }

            //V spiral logic V

            if (linepos == spiralLineLength) //if this spiral line has been completed
            {
                linepos = 0; //reset line position
                //rotate the spiral direction
                if (dirx == 0) //moving up or down
                {
                    dirx = diry == 1 ? 1 : -1; //up becomes right, down becomes left
                    diry = 0;
                }
                else //moving left or right
                {
                    diry = dirx == 1 ? -1 : 1; //right becomes down, left becomes up
                    dirx = 0;
                }

                if (increaseLengthNextTime) //basically, the length of the spiral line only increses after 2 lines, so a bool is the best way to do this.
                {
                    spiralLineLength++;
                }
                increaseLengthNextTime = !increaseLengthNextTime;
            }
            ix += dirx;
            iy += diry;
            linepos++;

            //if too much time has been spent on the IEnumerator this frame,
            if ((Time.realtimeSinceStartup - iterationTime) > chunkGenCriticalTime)
            {
                iterationTime = Time.realtimeSinceStartup;
                yield return true;

                //calculate player movement during this frame and stop if need be
                float playerDeltaX = currentPlayerPos.x - playerXPos, playerDeltaY = currentPlayerPos.z - playerYPos;
                if (Mathf.Sqrt(playerDeltaX * playerDeltaX + playerDeltaY * playerDeltaY) > chunkCheckRestartDistance)
                {
                    yield break;
                }

                while (!this.enabled) yield return null;
            }
        }

        //find chunks to delete
        var chunks = loadedChunks.ToArray(); //NOTE: this clone must be done because values are being removed from the dictionary, which will cause an enumerator sync error
        foreach (KeyValuePair<ChunkCoord, TerrainChunk> c in chunks)
        {
            if (c.Value.lastChunkCheck != currentChunkCheck) DestroyChunk(c);
        }

        unchecked { currentChunkCheck+=1; }

        initialChunkGenDone = true;
    }

    public int CheckCurrentResolutionOfChunk(ChunkCoord coord)
    {
        TerrainChunk chunk;
        if(loadedChunks.TryGetValue(coord, out chunk))
        {
            return chunk.currentMeshResolution;
        }
        return 0;
    }

    void BuildChunk(ChunkCoord coord, int resolution)
    {
        GameObject gO = Instantiate(chunkPrefab, new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize), Quaternion.identity, transform);
        gO.name = coord.x + "," + coord.y;        
        TerrainChunk chunkData = gO.GetComponent<TerrainChunk>();

        loadedChunks.Add(coord, chunkData);

        chunkData.chunkPosX = coord.x;
        chunkData.chunkPosY = coord.y;
        chunkData.chunkSize = chunkSize;
        chunkData.worldGenState = worldGenState;
        chunkData.lastChunkCheck = currentChunkCheck;
        //chunkData.worldGenRef = this;
        chunkData.LoadChunkData();

        if (resolution > 1) //if it's not reeeaaallly far in the distance,
        {
            chunkData.PrecomputeFullHeightmap(); //precompute the heightmap
        }
        chunkData.UpdateTerrain(resolution);
        chunkData.PlaceAllObjects();
    }

    void DestroyChunk(KeyValuePair<ChunkCoord, TerrainChunk> chunk)
    {
        Destroy(loadedChunks[chunk.Key].gameObject);
        loadedChunks.Remove(chunk.Key);
    }

    public void SaveAllDirtyChunks()
    {
        foreach(KeyValuePair<ChunkCoord,TerrainChunk> c in loadedChunks)
        {
            if (c.Value.chunkDirty) c.Value.SaveChunkData();
        }
    }
}