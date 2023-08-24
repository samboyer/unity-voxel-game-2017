using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventoryController : MonoBehaviour {

    public int voxelsCarrying;

    public static List<byte[]> collectedColors = new List<byte[]>();

    private ByteArrayComparer comparer = new ByteArrayComparer();

    public Text textVoxelCount;

    public AudioClip sfxVoxelCollect;
    public AudioClip sfxVoxelCollectNewColour;

    AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void CollectVoxel(Color32 color)
    {
        CollectVoxel(1, color);
    }
    public void CollectVoxel(int value, Color32 color)
    {
        byte[] col = color.ToArray();
        if (collectedColors.BinarySearch(col, comparer) <0) //wow new colour!
        {
            collectedColors.Add(col);
            SortColors();
            audioSource.PlayOneShot(sfxVoxelCollectNewColour);
        }
        else
        {
            audioSource.PlayOneShot(sfxVoxelCollect);
        }
        voxelsCarrying+= value;
    }

    public void SortColors()
    {
        collectedColors.Sort(comparer);
    }

    class ByteArrayComparer : IComparer<byte[]>
    {
        public int Compare(byte[] x, byte[] y)
        {
            for (int ix=0; ix<x.Length; ix++)
            {
                if (ix >= y.Length) return -1;
                if (x[ix] < y[ix]) return -1;
                if (x[ix] == y[ix]) continue;
                return 1;
            }

            return 0;
        }
    }

    int oldVal=-1;
    private void Update()
    {
        if(oldVal!=voxelsCarrying)
            textVoxelCount.text = voxelsCarrying + " Voxels";
        oldVal = voxelsCarrying;
    }
}
