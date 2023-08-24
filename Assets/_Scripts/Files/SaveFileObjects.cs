using System.Collections.Generic;
using UnityEngine;

public class SaveFileObjects
{
    public class SaveFileWorldState
    {
        public string worldName; //this *is* needed, as savefile archive titles could be subject to sanitisation.
        public int seed;
        public float worldTime;
        public double lastModified;
    }

    public class SaveFileChunkState
    {
        public int chunkPosX, chunkPosY; //needed?

        public short[] naturalObjectsDestroyed; //ooookkaaayyyy.... yes it's a short, because natural ObjIDs are small nums... 32*32*4=4096 max possible trees
        public PlacedObject[] placedObjects;
    }

    public class SaveFilePlayerState
    {
        public string name; //needed?

        public float posX, posY, posZ;
        public float rotX, rotY, rotZ;
        public float camRotX, camRotY, camZoom;

        public int voxels;
        public string[] collectedColors;
        //public string[][] test;
    }

}

public class PlacedObject
{
    public string modelName;
    public float posX, posY, posZ;
    public float rotX, rotY, rotZ;
    public int hp;
}