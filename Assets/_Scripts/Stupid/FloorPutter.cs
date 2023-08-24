using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorPutter : MonoBehaviour {

    WorldTerrainGenerator worldTerrainGen;

    public float offset;

    public float updateInterval = 0.1f;

	void Start () {
        worldTerrainGen = WorldTerrainGenerator.current;

        StartCoroutine(WaitToPlaceChunk());
    }
	
    IEnumerator WaitToPlaceChunk()
    {
        ChunkCoord coord = new ChunkCoord(Mathf.FloorToInt(transform.position.x / worldTerrainGen.chunkSize), Mathf.FloorToInt(transform.position.z / worldTerrainGen.chunkSize));
        int requiredRes = worldTerrainGen.worldGenState.maxResolution;

        while (worldTerrainGen.CheckCurrentResolutionOfChunk(coord) != requiredRes)
            yield return new WaitForSeconds(updateInterval);

        Vector3 pos = transform.position;
        pos.y = worldTerrainGen.loadedChunks[coord].GetHeightAtLocalPoint(pos.x - coord.x * worldTerrainGen.chunkSize, pos.z - coord.y * worldTerrainGen.chunkSize) + offset;
        transform.position = pos;
    }
}
