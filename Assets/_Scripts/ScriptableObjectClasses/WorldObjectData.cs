using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="NewWorldObject",menuName ="World Object Data")]
public class WorldObjectData : ScriptableObject {

    [Tooltip("Display name of the object.")]
    public string displayName;

    [Tooltip("Path to the model file in Resources/models.")]
    public string modelName;

    public WorldObjectScale scale;

    [Tooltip("The center of the object, used when placing on ground")]
    public Vector3 modelCenter;

    public WorldObjectType type;

    public int HP = 100;

    [Header("Generation Rates")]
    public float frequency = 1;
}
public enum WorldObjectType
{
    NaturalGeneric,
    NaturalTree, //axe
    NaturalPlant, //machete?
    NaturalRock, //pickaxe
    NaturalArtifact, //pickaxe again maybe, but important to keep distinct from rock... for reasons...
    Artificial
}

public enum WorldObjectScale
{
    Small, //people, flowers,
    Medium, //trees
    Large
}