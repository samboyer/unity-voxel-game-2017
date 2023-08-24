using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SamBoyer.VoxelEngine;

public class TerrainChunk : MonoBehaviour {
    MeshFilter meshFilter;

    //public WorldTerrainGenerator worldGenRef;

    public int chunkPosX, chunkPosY;

    public int chunkSize = 32;
    public WorldGenState worldGenState;

    float[,] heightmap;
    public int currentHeightmapResolution = 0;

    public int currentMeshResolution = 0;

    public int lastChunkCheck=-1;

    public GameObject TerrainObjectTemplate;

    [HideInInspector]
    public bool chunkDirty;

    void Start () {
        meshFilter = GetComponent<MeshFilter>();
    }


    List<short> naturalObjectsDestroyed = new List<short>();
    Dictionary<int,PlacedObject> placedObjectsDict = new Dictionary<int, PlacedObject>();

    #region SAVE/LOAD  

    public void LoadChunkData()
    {
#if UNITY_EDITOR
        SaveFileObjects.SaveFileChunkState state = null;
        if (WorldSessionController.file != null)
            state = WorldSessionController.file.LoadChunkData(chunkPosX, chunkPosY);
#else
        SaveFileObjects.SaveFileChunkState state = WorldSessionController.file.LoadChunkData(chunkPosX, chunkPosY);
#endif
        if (state != null)
        {
            naturalObjectsDestroyed = new List<short>(state.naturalObjectsDestroyed);
            placedObjectsDict = new Dictionary<int, PlacedObject>(state.placedObjects.Length);
            for(int i = 0; i < state.placedObjects.Length; i++)
            {
                placedObjectsDict.Add(i, state.placedObjects[i]);
            }
        }

    }

    void OnDisable()
    {
        if (chunkDirty) SaveChunkData();
    }

    public void SaveChunkData()
    {
        //first check if there's anything to save...
        if (placedObjectsDict.Count == 0 && naturalObjectsDestroyed.Count == 0) return;

        SaveFileObjects.SaveFileChunkState state = new SaveFileObjects.SaveFileChunkState
        {
            chunkPosX = chunkPosX, chunkPosY = chunkPosY, naturalObjectsDestroyed = naturalObjectsDestroyed.ToArray(), placedObjects = new PlacedObject[placedObjectsDict.Count]
        };
        int i = 0;
        foreach(PlacedObject p in placedObjectsDict.Values)
        {
            state.placedObjects[i] = p;
            i++;
        }


#if UNITY_EDITOR
        if(WorldSessionController.file != null)
            WorldSessionController.file.SaveChunkData(state, chunkPosX, chunkPosY);
#else
        WorldSessionController.file.SaveChunkData(state, chunkPosX, chunkPosY);

#endif
        chunkDirty = false;
    }

#endregion

    public void UpdateTerrain(int newMeshResolution)
    {
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
        Mesh m = GenerateSquareGridMesh(chunkSize, newMeshResolution);
        meshFilter.mesh = m;
        GetComponent<MeshCollider>().sharedMesh = m;

        currentMeshResolution = newMeshResolution;
    }

#region HEIGHTMAP

    public void PrecomputeFullHeightmap()
    {
        PopulateHeightmap(worldGenState.maxResolution);
    }

    void PopulateHeightmap(int mapResolution)
    {
        int vertsPerAxis = mapResolution + 1; //number of vertices on heightmap (+1 b.c. faces->verts)
        heightmap = new float[vertsPerAxis, vertsPerAxis];
        Vector3 pos = transform.position;

        float posStep = (float)chunkSize / (mapResolution); //the amount of distance to increment by when sampling.

        //populate heightmap
        for (int y = 0; y < vertsPerAxis; y++)
        {
            for (int x = 0; x < vertsPerAxis; x++)
            {
                heightmap[x, y] = GetValueAtPoint(x * posStep + pos.x, y * posStep + pos.z) * worldGenState.heightScale;
            }
        }
        currentHeightmapResolution = mapResolution;
    }

    float GetValueAtPoint(float x, float y)
    {
        Vector3 pt = new Vector3(x * worldGenState.noiseFrequency, y * worldGenState.noiseFrequency, 0);

        float height = NoiseGenerator.SumSimplex2D(pt, worldGenState.noiseOctaves, worldGenState.lacunarity, worldGenState.persistence);

        //evaluate heightcurve
        height = worldGenState.heightCurve.Evaluate(height);

        Vector3 ptDetail = new Vector3(x * worldGenState.detailNoiseFrequency, y * worldGenState.detailNoiseFrequency, 0);
        height += NoiseGenerator.SumSimplex2D(ptDetail,worldGenState.detailNoiseOctaves,worldGenState.lacunarity,worldGenState.persistence) * worldGenState.detailInfluence;

        return height;
    }

    #endregion

    Mesh GenerateSquareGridMesh(int chunkSize, int resolution)
    {
        //NOTE: since the map size should always be a power of 2 (+1, cause faces->verts), the heightmap array can be indexed using a scale factor.
        //for example, if heightmap has resolution 64 (65 verts, 64 faces) and the mesh wants resolution 4 (5 verts, 4 faces), the scale factor would be
        // currentHeightmapResolution/resolution  = 64/4 = 16.

        if (resolution == 0) resolution = 1; //1 face, 2 verts

        if (heightmap == null) //if it's not already computed (on creation)
        {
            PopulateHeightmap(resolution); //compute at this level
        }
        else
        {
            if (currentHeightmapResolution < resolution) //if heightmap needs computing to a higher resolution
            {
                PopulateHeightmap(resolution);
            }
        }
        int indexScaleFactor = currentHeightmapResolution / resolution;

        float posStep = (float)chunkSize / resolution; //incremention amount

        Mesh m = new Mesh();
        Vector3[] vertices = new Vector3[(resolution + 1) * (resolution + 1) * 2 * 3];
        int[] triangles = new int[(resolution + 1) * (resolution + 1) * 2 * 3];

        int vertCounter = 0;

        for (int y = 0; y < resolution; y++) //for each face,
        {
            for (int x = 0; x < resolution; x++)
            {
                float h00 = heightmap[x * indexScaleFactor, y * indexScaleFactor],
                    h10 = heightmap[(x + 1) * indexScaleFactor, y * indexScaleFactor],
                    h01 = heightmap[x * indexScaleFactor,(y + 1) * indexScaleFactor],
                    h11 = heightmap[(x + 1) * indexScaleFactor, (y + 1) * indexScaleFactor];

                float diagonalDiffRight = h00 - h11;
                float diagonalDiffLeft = h01 - h10;

                diagonalDiffRight = diagonalDiffRight > 0 ? diagonalDiffRight : checked(-diagonalDiffRight); //fast abs function
                diagonalDiffRight = diagonalDiffRight > 0 ? diagonalDiffRight : checked(-diagonalDiffRight);

                if (worldGenState.useParallelSeams && diagonalDiffRight > diagonalDiffLeft) //draw triangles using bottom-left to top right seam
                {
                    vertices[vertCounter] = new Vector3(x * posStep, h00, y * posStep);
                    vertices[vertCounter + 1] = new Vector3(x * posStep, h01, (y + 1) * posStep);
                    vertices[vertCounter + 2] = new Vector3((x + 1) * posStep, h11, (y + 1) * posStep);
                    vertices[vertCounter + 3] = new Vector3(x * posStep, h00, y * posStep);
                    vertices[vertCounter + 4] = new Vector3((x + 1) * posStep, h11, (y + 1) * posStep);
                    vertices[vertCounter + 5] = new Vector3((x + 1) * posStep, h10, y * posStep);
                }
                else //draw triangles using top-left to bottom-right seam
                {
                    vertices[vertCounter] = new Vector3(x * posStep, h00, y * posStep);
                    vertices[vertCounter + 1] = new Vector3(x * posStep, h01, (y + 1) * posStep);
                    vertices[vertCounter + 2] = new Vector3((x + 1) * posStep, h10, y * posStep);
                    vertices[vertCounter + 3] = new Vector3(x * posStep, h01, (y + 1) * posStep);
                    vertices[vertCounter + 4] = new Vector3((x + 1) * posStep, h11, (y + 1) * posStep);
                    vertices[vertCounter + 5] = new Vector3((x + 1) * posStep, h10, y * posStep);
                }

                triangles[vertCounter] = vertCounter;
                triangles[vertCounter + 1] = vertCounter + 1;
                triangles[vertCounter + 2] = vertCounter + 2;
                triangles[vertCounter + 3] = vertCounter + 3;
                triangles[vertCounter + 4] = vertCounter + 4;
                triangles[vertCounter + 5] = vertCounter + 5;

                vertCounter += 6;
            }
        }

        m.vertices = vertices;
        m.triangles = triangles;
        m.uv = new Vector2[(resolution + 1) * (resolution + 1) * 2 * 3];
        
        m.RecalculateNormals();
        m.UploadMeshData(false);
        return m;
    }

#region OBJECTS

    public float treeThreshold = 0.8f;
    public float forestationNoiseFrequency = 0.01f;
    public int forestationNoiseOctaves = 2;
    public float treeMaxGradient = 50f;
    [Range(0,1)]
    public float forestationNoiseInfluence = 0.5f;
    public int treesPerVertex = 2;

    bool foliagePlaced = false;

    public void PlaceAllObjects()
    {
        //scenarios when this function shouldn't be running
        if (foliagePlaced) return; //duh
        if (currentMeshResolution < worldGenState.maxResolution) return; //who wants floating trees?

        PlaceNaturalObjects();

        //place user objects
        foreach(KeyValuePair<int, PlacedObject> pair in placedObjectsDict)
        {
            PlacedObject p = pair.Value;
            GameObject gO = Instantiate(TerrainObjectTemplate, new Vector3(p.posX,p.posY,p.posZ), Quaternion.Euler(p.rotX,p.rotY,p.rotZ), transform);
            gO.name = p.modelName;
            VoxelRenderer rend = gO.transform.Find("MODEL").GetComponent<VoxelRenderer>();
            gO.GetComponent<WorldObject>().objectId = pair.Key;
            if (VoxelModelLibrary.Library.ContainsKey(p.modelName)) rend.Model = VoxelModelLibrary.Library[p.modelName];
            else rend.Model = VoxelModelLibrary.AddModelFromSaveFile("customObjects/" + p.modelName, WorldSessionController.file, true);
        }
    }

    void PlaceNaturalObjects()
    {
        short naturalObjectId = 0;

        float posStep = (float)chunkSize / currentHeightmapResolution;

        float verticesPerTree = 1 / (float)treesPerVertex;

        int lehmerSeed = unchecked(WorldTerrainGenerator.current.seed + chunkPosX * 1300021 + chunkPosY * 1300333); //just random huge primes for unique seeding, nothing vital
        LehmerRNG rng = new LehmerRNG(lehmerSeed);

        //for each coord
        for (int y = 0; y < currentHeightmapResolution; y++)
        {
            for (int x = 0; x < currentHeightmapResolution; x++)
            {
                //fetch heightmap values for interpolation
                float p00 = heightmap[x, y],
                    p01 = heightmap[x, y + 1],
                    p10 = heightmap[x + 1, y],
                    p11 = heightmap[x + 1, y + 1];

                for (int y1 = 0; y1 < treesPerVertex; y1++)
                {
                    for (int x1 = 0; x1 < treesPerVertex; x1++)
                    {
                        naturalObjectId++;

                        //NOTE: for deterministic reasons, these MUST be performed regardless of placement. If not, it'll put stuff out of sync.
                        float noiseTreeProb = rng.NextFraction(); //probability of tree going here.
                        float randPosX = rng.NextFraction(), randPosY = rng.NextFraction(), rotation = rng.NextFraction();

                        float typeFrac = rng.NextFraction(); //for object type later on

                        if (naturalObjectsDestroyed.Contains(naturalObjectId)) //if this object has been destroyed beforehand
                        {
                            continue;
                        }

                        float positionOnFaceX = randPosX * verticesPerTree + x1 * verticesPerTree;
                        float positionOnFaceY = randPosY * verticesPerTree + y1 * verticesPerTree;

                        //get point on this face
                        float placementHeight = Mathf.LerpUnclamped(
                            Mathf.LerpUnclamped(p00, p10, positionOnFaceX),
                            Mathf.LerpUnclamped(p01, p11, positionOnFaceX), positionOnFaceY);


                        //EXCLUSION SITUATIONS  
                        //exclude if below sea level (for now, height-dependant foliage could implement coral or smth) 
                        if (placementHeight < 0) continue;

                        //exclude if gradient too great (again, for now... maybe rocks at high gradients?)
                        float gradient = Mathf.Max(p00, p01, p10, p11) - Mathf.Min(p00, p01, p10, p11);

                        if (gradient > treeMaxGradient) continue;

                        //combine with forestation noise
                        Vector3 forestationNoisePt = new Vector3((x * posStep + transform.position.x) * forestationNoiseFrequency, (y * posStep + transform.position.z) * forestationNoiseFrequency, 0);
                        float treeProbability = NoiseGenerator.SumSimplex2D(forestationNoisePt, forestationNoiseOctaves);
                        treeProbability = Mathf.Lerp(noiseTreeProb, treeProbability * 0.5f + 0.5f, forestationNoiseInfluence);

                        if (treeProbability > treeThreshold)
                        {
                            //place the object
                            WorldObjectData wod = ChooseNaturalObject(typeFrac);

                            GameObject gO = Instantiate(TerrainObjectTemplate, new Vector3((x + positionOnFaceX) * posStep + transform.position.x, placementHeight, (y + positionOnFaceY) * posStep + transform.position.z), Quaternion.Euler(0, rotation*360, 0), transform);
                            gO.name = naturalObjectId.ToString();

                            WorldObject obj = gO.GetComponent<WorldObject>();
                            obj.InitialiseFromData(wod);
                            obj.objectId = naturalObjectId;
                            obj.isNaturalObject = true;
                        }
                    }
                }
            }
        }
        foliagePlaced = true;
    }

    static bool isObjectDataLoaded = false;
    static float naturalObjectFrequencySum = -1;
    static WorldObjectData[] naturalObjectData;

    WorldObjectData ChooseNaturalObject(float randomFraction01)
    {
        if (!isObjectDataLoaded) LoadWorldObjectData();

        float val = randomFraction01 * naturalObjectFrequencySum; //this gives us a random val between 0 and frequencySum

        foreach(WorldObjectData w in naturalObjectData)
        {
            if (val < w.frequency) return w;
            val -= w.frequency;
        }
        
        return naturalObjectData[0];
    }

    void LoadWorldObjectData()
    {
        float sum = 0;
        naturalObjectData = Resources.LoadAll<WorldObjectData>("ObjectData");
        foreach(WorldObjectData w in naturalObjectData)
        {
            sum += w.frequency;
        }
        naturalObjectFrequencySum = sum;
        isObjectDataLoaded = true;
    }

    public void DestroyAllObjects()
    {
        if (!foliagePlaced) return;
        //NOTE: this *really* needs improving

        //destroy all children
        int cc = transform.childCount;
        for(int i = 0; i < cc; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        foliagePlaced = false;
    }

    public void PlaceCustomObject(string modelName, Vector3 position, Quaternion rotation)
    {
        Vector3 rotEuler = rotation.eulerAngles;
        PlacedObject obj = new PlacedObject
        {
            modelName = modelName, posX = position.x, posY = position.y, posZ = position.z, rotX = rotEuler.x, rotY = rotEuler.y, rotZ = rotEuler.z, hp=100
        };
        GameObject gO = Instantiate(TerrainObjectTemplate, position, rotation, transform);
        gO.name = modelName;
        VoxelRenderer rend = gO.transform.Find("MODEL").GetComponent<VoxelRenderer>();

        if (VoxelModelLibrary.Library.ContainsKey(modelName)) rend.Model = VoxelModelLibrary.Library[modelName];
        else rend.Model = VoxelModelLibrary.AddModelFromSaveFile("customObjects/" + modelName, WorldSessionController.file, true);

        int id = Random.Range(0, int.MaxValue);
        gO.GetComponent<WorldObject>().objectId = id;
        StartCoroutine(gO.GetComponent<WorldObject>().PlaceModelAnimCo());
        placedObjectsDict.Add(id, obj);

        chunkDirty = true;
    }

    //old, should probably remove
    public void DestroyObject(GameObject obj, bool explode)
    {
        short id;
        if(short.TryParse(obj.name, out id)) //if object name is a number, meaning it's foliage
        {
            naturalObjectsDestroyed.Add(id);
            Destroy(obj);
        }
        else //must be a custom model
        {
            foreach(KeyValuePair<int, PlacedObject> pair in placedObjectsDict)
            {
                PlacedObject p = pair.Value;
                if(p.modelName == obj.name && p.posX == obj.transform.position.x && p.posY == obj.transform.position.y && p.posZ == obj.transform.position.z)
                {
                    placedObjectsDict.Remove(pair.Key);
                    Destroy(obj);
                    break;
                }
            }
        }
        if (explode)
            GameObject.Find("VoxelExplode").GetComponent<SplodeyVoxels>().ExplodeModel(obj.transform.Find("MODEL").GetComponent<VoxelRenderer>(), 0.5f);

        chunkDirty = true;
    }

    public void RemoveNaturalObject(int id)
    {
        naturalObjectsDestroyed.Add((short)id);
        chunkDirty = true;
    }
    public void RemovePlacedObject(int id)
    {
        if(placedObjectsDict.Remove(id) == false)
            print ("oh");
        chunkDirty = true;
    }

#endregion

    /// <summary>
    /// Gets the value of the heightmap at this position relative to the chunk (in local space).
    /// </summary>
    public float GetHeightAtLocalPoint(float x, float y)
    {
        if (x < 0 || y < 0 || x > chunkSize || y > chunkSize) return -1;

        //fetch heightmap values for interpolation

        float heightmapPosX = (x / chunkSize) * currentHeightmapResolution;
        float heightmapPosY = (y / chunkSize) * currentHeightmapResolution;

        int floorCoordX = Mathf.FloorToInt(heightmapPosX);
        int floorCoordY = Mathf.FloorToInt(heightmapPosY);

        float positionOnFaceX = heightmapPosX - floorCoordX;
        float positionOnFaceY = heightmapPosY - floorCoordY;

        float p00 = heightmap[floorCoordX, floorCoordY],
            p01 = heightmap[floorCoordX, floorCoordY + 1],
            p10 = heightmap[floorCoordX + 1, floorCoordY],
            p11 = heightmap[floorCoordX + 1, floorCoordY + 1];

        //get point on this face
        float placementHeight = Mathf.Lerp(
            Mathf.Lerp(p00, p10, positionOnFaceX),
            Mathf.Lerp(p01, p11, positionOnFaceX), positionOnFaceY);

        return placementHeight;
    }

    public float GetHeightAtPoint(float x, float y)
    {
        return GetHeightAtLocalPoint(x - transform.position.x, y - transform.position.z);
    }
}


public struct ChunkCoord
{
    public int x, y;
    public ChunkCoord(int x, int y) { this.x = x; this.y = y; }
}