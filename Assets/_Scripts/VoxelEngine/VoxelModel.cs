using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

namespace SamBoyer.VoxelEngine
{
    public struct ModelDimensions
	{
		public int x;
        public int y;
        public int z;
        public int volume { get { return x * y * z; } }

        public ModelDimensions(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
	}

    public partial class VoxelModel
    {

        /// <summary>
        /// Flattened array of voxel index data. Index 0 represents no voxel present.
        /// </summary>
        public byte[] voxelsFlat;

        public byte this[int x, int y, int z]
        {
            get { return this.voxelsFlat[x + size.x * (y + size.y * z)]; }
            set{this.voxelsFlat[x + size.x * (y + size.y * z)] = value;}
        }

        public string modelName;

        /// <summary>
        /// The VoxelMesh associated with this model.
        /// </summary>
        public VoxelMesh voxelMesh;

        public ModelDimensions size { get; private set; }

        /// <summary>
        /// The (Y-up) center point of the voxelModel. Used as an offset when generating meshes.
        /// </summary>
        public Vector3 modelCenter;

        /// <summary>
        /// The VoxelPalette associated with this model.
        /// </summary>
        public VoxelPalette palette
        {
            get { return _palette; }
            set { _palette = value; _palette.SubscribeToPalette(this); }
        }
        VoxelPalette _palette;

        /// <summary>
        /// The number of voxels present in the model (Read Only).
        /// </summary>
        public int voxelCount{get;private set;}


        #region CONSTRUCTORS
        public VoxelModel()
        {
            this.palette = new VoxelPalette(1);
            this.palette.colors[1] = new byte[] { 255, 255, 255, 255};
            this.voxelMesh = new VoxelMesh(this);
            this.size = new ModelDimensions(1, 1, 1);
            this.voxelsFlat = new byte[1];
        }
        public VoxelModel(ModelDimensions size)
        {
            this.size = size;
            this.voxelsFlat = new byte[this.size.volume];
            this.palette = new VoxelPalette(1);
            this.palette.colors[1] = new byte[] { 255, 255, 255, 255 };
            this.voxelMesh = new VoxelMesh(this);
        }

        #endregion

        /// <summary>
        /// Returns the color index of the selected voxel, while checking model boundaries.
        /// </summary>
        public byte GetVoxelColorIndex(int x, int y, int z)
        {
            if(x<0 || y<0 || z<0 || x>=size.x || y >= size.y || z >= size.z)
            {
                return 0;
            }
            return this[x,y,z];
        }

        /// <summary>
        /// Returns the color index of the selected voxel.
        /// </summary>
        /// <returns>A boolean stating if the operation was successful.</returns>
        public bool SetVoxelColorIndex(int x, int y, int z, byte colorIndex)
        {
            if (x < 0 || y < 0 || z < 0) { return false; }
            if (x >= size.x || y >= size.y || z >= size.z) { return false; }
            byte oldIndex = this[x, y, z];
            if (oldIndex == 0) { return false; } //voxel not here
            
            this[x, y, z] = colorIndex;
            if(oldIndex!=colorIndex) voxelMesh.meshDirty = true;

            return true;
        }

        /// <summary>
        /// Adds a voxel with specified color index to the specified location.
        /// <returns>A boolean stating if the operation was successful.</returns>
        public bool AddVoxel(int x, int y, int z, byte colorIndex)
        {
            if(colorIndex == 0) { return false; }
            if(x<0 || y<0 || z < 0) { return false; }
            if(x>=size.x || y>=size.y || z>= size.z) { return false; }
            this[x, y, z] = colorIndex;
            voxelMesh.meshDirty = true;
            voxelCount++;
            return true;
        }

        /// <summary>
        /// Removes the voxel in the specified location.
        /// <returns>A boolean stating if the operation was successful.</returns>
        public bool SubtractVoxel(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0) { return false; }
            if (x >= size.x || y >= size.y || z >= size.z) { return false; }
            this[x, y, z] = 0;
            voxelMesh.meshDirty = true;
            voxelCount--;
            return true;
        }

        public void RecalculateVoxelCount()
        {
            int count = 0;
            for (int z = 0; z < this.size.z; z++)
            {
                for (int y = 0; y < this.size.y; y++)
                {
                    for (int x = 0; x < this.size.x; x++)
                    {
                        byte index = this[x, y, z];
                        if (index != 0)
                        {
                            count++;
                        }
                    }
                }
            }
            this.voxelCount = count;
        }

        public enum Direction
        {
            positiveX=1,
            negativeX=-1,
            positiveY=2,
            negativeY=-2,
            positiveZ=3,
            negativeZ=-2
        }
        public void ExpandDimensions(int amount, Direction direction)
        {
            ModelDimensions newDimensions = new ModelDimensions(size.x, size.y, size.z);

            int shiftX = 0, shiftY = 0, shiftZ = 0;

            switch ((int)direction)
            {
                case 1: //X
                    newDimensions.x += amount;
                    break;
                case 2: //Y
                    newDimensions.y += amount;
                    break;
                case 3: //Z
                    newDimensions.z += amount;
                    break;
                case -1: //-X
                    newDimensions.x += amount;
                    shiftX = amount;
                    break;
                case -2: //-Y
                    newDimensions.y += amount;
                    shiftY = amount;
                    break;
                case -3: //-Z
                    newDimensions.z += amount;
                    shiftZ = amount;
                    break;
            }

            byte[] newVoxelsFlat = new byte[newDimensions.volume];

            //go through each voxel in the model, map to new coords and put em into newVoxelsFlat
            for (int z = 0; z < this.size.z; z++)
            {
                for (int y = 0; y < this.size.y; y++)
                {
                    for (int x = 0; x < this.size.x; x++)
                    {
                        //if direction is positive, the coordinates don't actually have to be remapped... oh well
                        int newX = x + shiftX, newY = y + shiftY, newZ = z + shiftZ;

                        if (amount < 0)
                            if (newX < 0 || newX >= newDimensions.x || newY < 0 || newY >= newDimensions.y || newZ < 0 || newZ >= newDimensions.z)
                                continue;

                        newVoxelsFlat[newX + newDimensions.x * (newY + newDimensions.y * newZ)] = this[x, y, z];
                    }
                }
            }
            size = newDimensions;
            voxelsFlat = newVoxelsFlat;
            if ((int)direction < 0 || amount < 0)
                voxelMesh.meshDirty = true;
        }
    }
}
