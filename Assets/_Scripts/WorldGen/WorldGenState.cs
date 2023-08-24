using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// WorldGenState is a container for all the world generator variables.
/// These values are specified in the Editor and never changed.
/// </summary>
[System.Serializable]
public class WorldGenState
{
    public float heightScale = 1;
    public int maxResolution = 64;

    public float noiseFrequency = 0.001f;
    public int noiseOctaves = 2;

    public bool useParallelSeams = true;

    public float lacunarity = 2;
    [Range(0, 1)]
    public float persistence = 0.5f;

    public AnimationCurve heightCurve;

    public float detailNoiseFrequency = 0.00025f;
    public int detailNoiseOctaves = 2;

    public float detailInfluence = 0.001f;
}