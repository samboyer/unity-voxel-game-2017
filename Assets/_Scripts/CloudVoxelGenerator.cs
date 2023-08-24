using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SamBoyer.VoxelEngine;

public class CloudVoxelGenerator : MonoBehaviour {

    public int sizeX, sizeY, sizeZ;

    public float edgeThreshold;

    [Range(0, 1)]
    public float noiseInfluence;

    public float noiseFrequency = 1;

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 100, 25), "GenCloud")) GenerateCloud(new ModelDimensions(sizeX, sizeY, sizeZ), new Vector3(Random.value*10000, Random.value * 10000, Random.value * 10000));
    }

    public void GenerateCloud(ModelDimensions size, Vector3 noiseOffset)
    {
        Vector3 modelCenter = new Vector3(size.x / 2 - 0.5f, size.y / 2 - 0.5f, size.z / 2 - 0.5f);

        float sizeXSqr = size.x * size.x, sizeYSqr = size.y * size.y, sizeZSqr = size.z * size.z;

        VoxelModel mdl = new VoxelModel(size);

        for (int z=0 ; z < size.z; z++)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {

                    float centerDistX = Mathf.Abs(x - modelCenter.x), centerDistY = Mathf.Abs(y - modelCenter.y), centerDistZ = Mathf.Abs(z - modelCenter.z);

                    //float edgeDistX = modelCenter.x - Mathf.Abs(x - modelCenter.x), edgeDistY = modelCenter.y - Mathf.Abs(y - modelCenter.y), edgeDistZ =  modelCenter.z - Mathf.Abs(z - modelCenter.z);

                    float value = Mathf.Sqrt((centerDistX * centerDistX) / sizeXSqr + (centerDistY * centerDistY) / sizeYSqr + (centerDistZ * centerDistZ) / sizeZSqr);

                    value += NoiseGenerator.PerlinNoise3D(new Vector3(x, y, z)*noiseFrequency + noiseOffset) * noiseInfluence;

                    if (value <= edgeThreshold) mdl[x, y, z] = 1;
                }
            }
        }
        GetComponent<VoxelRenderer>().Model = mdl;
    }
}
