using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SamBoyer.VoxelEngine;

public class WorldObject : MonoBehaviour {

    public int objectId;
    public bool isNaturalObject = false;

    public WorldObjectData data;

    public GameObject voxelPrefab;
    public GameObject bigVoxelPrefab;

    int bigVoxelValue = 8;
    public float bigVoxelProbability;
    public float voxelPlaceVariation;
    public float voxelPlaceVariationSmall;

    public int HP = 100;

    //VoxelModel model;

    public void InitialiseFromData(WorldObjectData data)
    {
        this.HP = data.HP;
        this.data = data;

        VoxelRenderer rend = transform.Find("MODEL").GetComponent<VoxelRenderer>();
        VoxelModel mdl = VoxelModelLibrary.GetVoxelModel(data.modelName, false);
        if (mdl == null)
            mdl = VoxelModelLibrary.AddModelFromResources(data.modelName, data.modelCenter, true);
        rend.Model = mdl;

        switch (data.scale)
        {
            case WorldObjectScale.Small:
                rend.transform.localScale = new Vector3(.25f, .25f, .25f);
                break;
            case WorldObjectScale.Large:
                rend.transform.localScale = new Vector3(4, 4, 4);
                break;
        }
    }


    public void DoDamage(int damage, float yieldPercentage)
    {
        HP -= damage;
        if (HP <= 0)
            DestroyObject(yieldPercentage);
        else
            StartCoroutine(HitAnimCo(hitShakeIntensity));
    }

    public void DestroyObject(float yieldPercentage) //delete this model and emit voxels
    {
        VoxelModel model = transform.Find("MODEL").GetComponent<VoxelRenderer>().Model;
        //must output at least one of each colour!

        int outputCount = Mathf.RoundToInt(model.voxelCount * yieldPercentage);
        int n = 0;

        for (int i = 0; i < model.palette.numColors; i++)
        {
            if (Random.value < bigVoxelProbability)
            {
                SpawnVoxel(model.palette.colors[i+1].ToColor(), true, model);
                n += bigVoxelValue;
            }
            else
            {
                SpawnVoxel(model.palette.colors[i+1].ToColor(), false, model);
                n++;
            }
        }
        while (n < outputCount)
        {
            int index = Random.Range(1, model.palette.numColors + 1);
            if (Random.value < bigVoxelProbability)
            {
                SpawnVoxel(model.palette.colors[index].ToColor(), true, model);
                n += bigVoxelValue;
            }
            else
            {
                SpawnVoxel(model.palette.colors[index].ToColor(), false, model);
                n++;
            }
        }

        if (isNaturalObject)
            transform.parent.GetComponent<TerrainChunk>().RemoveNaturalObject(objectId);
        else
            transform.parent.GetComponent<TerrainChunk>().RemovePlacedObject(objectId);

        Destroy(gameObject);
    }

    public void SpawnVoxel(Color32 color, bool big, VoxelModel model)
    {
        GameObject gO;
        Vector3 pos;

        Vector3 oldPos = transform.position + new Vector3(Random.Range(-model.size.x / 2f, model.size.x / 2f), Random.Range(0, model.size.z), Random.Range(-model.size.y / 2f, model.size.y / 2f)) * (data!=null && data.scale==WorldObjectScale.Small ? 0.25f : 1);

        if (data != null && data.scale == WorldObjectScale.Small)
            pos = new Vector3(Random.Range(-voxelPlaceVariationSmall, voxelPlaceVariationSmall), Random.value * voxelPlaceVariationSmall, Random.Range(-voxelPlaceVariationSmall, voxelPlaceVariationSmall));
        else
            pos = new Vector3(Random.Range(-voxelPlaceVariation, voxelPlaceVariation), Random.value * voxelPlaceVariation, Random.Range(-voxelPlaceVariation, voxelPlaceVariation));

        if (big)
        {
            gO = Instantiate(bigVoxelPrefab, oldPos, Quaternion.identity);
            gO.GetComponent<StupidCollectible>().destination = oldPos + pos;
        }
        else
        {
            gO = Instantiate(voxelPrefab, oldPos, Quaternion.identity);
            gO.GetComponent<StupidCollectible>().destination = oldPos + pos;
        }
        gO.GetComponent<StupidCollectible>().voxelColor = color;
    }

    //animations
    public float placeModelAnimDuration = .5f;
    public float placeModelAnimHeight = 3;
    public float placeModelAnimMaxAngle = 15;

    public IEnumerator PlaceModelAnimCo()
    {
        Transform modelT = transform.Find("MODEL");
        Vector3 endPos = modelT.localPosition;
        Quaternion endRot = modelT.localRotation;
        Quaternion startRot = Quaternion.Euler(endRot.eulerAngles.x+Random.Range(-placeModelAnimMaxAngle, placeModelAnimMaxAngle), endRot.eulerAngles.y, endRot.eulerAngles.z + Random.Range(-placeModelAnimMaxAngle, placeModelAnimMaxAngle));

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / placeModelAnimDuration;
            float val;
            if(t<0.6f)
                val = 2.777f*(t + 0.6f) * (0.6f-t);
            else
                val = 2 * (t - 1) * (t - 0.6f);

            modelT.localPosition = new Vector3(endPos.x, Mathf.Lerp(endPos.y, placeModelAnimHeight, Mathf.Abs(val)), endPos.z);
            modelT.localRotation = Quaternion.LerpUnclamped(endRot, startRot, val);

            yield return true;
        }
        modelT.localPosition = endPos;
        modelT.localRotation = endRot;
    }
    public float hitAnimDuration = .1f;
    public float hitShakeIntensity = .2f;
    public IEnumerator HitAnimCo(float intensity)
    {
        Vector3 pos = transform.localPosition;
        float t = 0;
        while (t < 1)
        {
            float i = Mathf.Lerp(intensity, 0, t);
            t += Time.deltaTime / hitAnimDuration;
            transform.localPosition = pos + new Vector3(Random.Range(-i, i), 0, Random.Range(-i, i));
            yield return true;
        }
        transform.localPosition = pos;
    }
}
