using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

namespace SamBoyer.VoxelEngine
{
    public class VoxelPalette
    {
        public byte[][] colors;
        public int numColors { get; private set; }

        List<VoxelModel> modelsUsingThis = new List<VoxelModel>();

        public void SubscribeToPalette(VoxelModel m)
        {
            if (!modelsUsingThis.Contains(m)) modelsUsingThis.Add(m);
        }

        public VoxelPalette Clone()
        {
            VoxelPalette pal2 = new VoxelPalette();
            pal2.colors = this.colors.Clone() as byte[][];
            pal2.numColors = this.colors.Length;
            return pal2;
        }

        public void AddColor(byte r, byte g, byte b)
        {
            if (numColors == 255) return;

            numColors++;

            //make a new copy of the colors, but bigger
            byte[][] newCols = new byte[numColors + 1][];
            for (int i = 0; i < colors.Length; i++)
            {
                newCols[i] = colors[i];
            }

            newCols[numColors] = new byte[] { r, g, b, 255 };

            colors = newCols;

            textureDirty = true;
        }

        public void AppendSimplifiedPalette(VoxelPalette newPalette)
        {
            //make a new palette, swap around the indexes of the new one
            Dictionary<byte, byte> conversions = new Dictionary<byte, byte>();

            for (byte i = 0; i < newPalette.colors.Length; i++)
            {
                byte[] col = newPalette.colors[i];
                bool found = false;
                for (byte j = 0; j < colors.Length; j++)
                {
  
                    if (col[0] == colors[j][0] && col[1] == colors[j][1] && col[2] == colors[j][2] && col[3] == colors[j][3])
                    {
                        found = true;
                        conversions.Add(i, j);
                        break;
                    }
                }
                if (!found)
                {
                    AddColor(col[0], col[1], col[2]);
                    conversions.Add(i, (byte)(numColors));
                }
            }

            foreach (VoxelModel m in newPalette.modelsUsingThis)
            {
                m.palette = this;
                for (int z = 0; z < m.size.z; z++)
                {
                    for (int y = 0; y < m.size.y; y++)
                    {
                        for (int x = 0; x < m.size.x; x++)
                        {
                            byte colIndex = m[x, y, z];
                            if (conversions.ContainsKey(colIndex)) m[x, y, z] = conversions[colIndex];
                        }
                    }
                }
                m.voxelMesh.Update();
            }
        }

        public void SimplifyPalette()
        {
            if(ReferenceEquals(this, VoxelPalette.defaultPalette))
            {
                Debug.Log("UH OH CAUGHT DEFAULT PALETTE SIMPLIFICAYIOn");
                //return;
            }
            List<byte> indexesUsed = new List<byte>();
            foreach(VoxelModel m in modelsUsingThis)
            {
                for (int z = 0; z < m.size.z; z++)
                {
                    for (int y = 0; y < m.size.y; y++)
                    {
                        for (int x = 0; x < m.size.x; x++)
                        {
                            byte index = m[x, y, z];
                            if (index != 0)
                            {
                                if (!indexesUsed.Contains(m[x, y, z]))
                                {
                                    indexesUsed.Add(index);
                                    m[x, y, z] = (byte)indexesUsed.Count; //since this colour just got added to the palette
                                }
                                else
                                {
                                    m[x, y, z] = (byte)(indexesUsed.IndexOf(index) + 1);
                                }
                            }
                        }
                    }
                }
                m.voxelMesh.meshDirty = true;
            }

            VoxelPalette pal = new VoxelPalette(indexesUsed.Count);
            for (int i = 0; i < indexesUsed.Count; i++)
            {
                pal.colors[i + 1] = this.colors[indexesUsed[i]];
            }

            foreach (VoxelModel m in modelsUsingThis)
            {
                m.palette = pal;
            }
        }

        #region DEFAULT_PALETTE
        public static VoxelPalette defaultPalette = new VoxelPalette
        {
            colors = new byte[256][]{
                new byte[4]{0, 0, 0, 0},
                new byte[4]{255, 255, 255, 255},
                new byte[4]{255, 255, 204, 255},
                new byte[4]{255, 255, 153, 255},
                new byte[4]{255, 255, 102, 255},
                new byte[4]{255, 255, 51, 255},
                new byte[4]{255, 255, 0, 255},
                new byte[4]{255, 204, 255, 255},
                new byte[4]{255, 204, 204, 255},
                new byte[4]{255, 204, 153, 255},
                new byte[4]{255, 204, 102, 255},
                new byte[4]{255, 204, 51, 255},
                new byte[4]{255, 204, 0, 255},
                new byte[4]{255, 153, 255, 255},
                new byte[4]{255, 153, 204, 255},
                new byte[4]{255, 153, 153, 255},
                new byte[4]{255, 153, 102, 255},
                new byte[4]{255, 153, 51, 255},
                new byte[4]{255, 153, 0, 255},
                new byte[4]{255, 102, 255, 255},
                new byte[4]{255, 102, 204, 255},
                new byte[4]{255, 102, 153, 255},
                new byte[4]{255, 102, 102, 255},
                new byte[4]{255, 102, 51, 255},
                new byte[4]{255, 102, 0, 255},
                new byte[4]{255, 51, 255, 255},
                new byte[4]{255, 51, 204, 255},
                new byte[4]{255, 51, 153, 255},
                new byte[4]{255, 51, 102, 255},
                new byte[4]{255, 51, 51, 255},
                new byte[4]{255, 51, 0, 255},
                new byte[4]{255, 0, 255, 255},
                new byte[4]{255, 0, 204, 255},
                new byte[4]{255, 0, 153, 255},
                new byte[4]{255, 0, 102, 255},
                new byte[4]{255, 0, 51, 255},
                new byte[4]{255, 0, 0, 255},
                new byte[4]{204, 255, 255, 255},
                new byte[4]{204, 255, 204, 255},
                new byte[4]{204, 255, 153, 255},
                new byte[4]{204, 255, 102, 255},
                new byte[4]{204, 255, 51, 255},
                new byte[4]{204, 255, 0, 255},
                new byte[4]{204, 204, 255, 255},
                new byte[4]{204, 204, 204, 255},
                new byte[4]{204, 204, 153, 255},
                new byte[4]{204, 204, 102, 255},
                new byte[4]{204, 204, 51, 255},
                new byte[4]{204, 204, 0, 255},
                new byte[4]{204, 153, 255, 255},
                new byte[4]{204, 153, 204, 255},
                new byte[4]{204, 153, 153, 255},
                new byte[4]{204, 153, 102, 255},
                new byte[4]{204, 153, 51, 255},
                new byte[4]{204, 153, 0, 255},
                new byte[4]{204, 102, 255, 255},
                new byte[4]{204, 102, 204, 255},
                new byte[4]{204, 102, 153, 255},
                new byte[4]{204, 102, 102, 255},
                new byte[4]{204, 102, 51, 255},
                new byte[4]{204, 102, 0, 255},
                new byte[4]{204, 51, 255, 255},
                new byte[4]{204, 51, 204, 255},
                new byte[4]{204, 51, 153, 255},
                new byte[4]{204, 51, 102, 255},
                new byte[4]{204, 51, 51, 255},
                new byte[4]{204, 51, 0, 255},
                new byte[4]{204, 0, 255, 255},
                new byte[4]{204, 0, 204, 255},
                new byte[4]{204, 0, 153, 255},
                new byte[4]{204, 0, 102, 255},
                new byte[4]{204, 0, 51, 255},
                new byte[4]{204, 0, 0, 255},
                new byte[4]{153, 255, 255, 255},
                new byte[4]{153, 255, 204, 255},
                new byte[4]{153, 255, 153, 255},
                new byte[4]{153, 255, 102, 255},
                new byte[4]{153, 255, 51, 255},
                new byte[4]{153, 255, 0, 255},
                new byte[4]{153, 204, 255, 255},
                new byte[4]{153, 204, 204, 255},
                new byte[4]{153, 204, 153, 255},
                new byte[4]{153, 204, 102, 255},
                new byte[4]{153, 204, 51, 255},
                new byte[4]{153, 204, 0, 255},
                new byte[4]{153, 153, 255, 255},
                new byte[4]{153, 153, 204, 255},
                new byte[4]{153, 153, 153, 255},
                new byte[4]{153, 153, 102, 255},
                new byte[4]{153, 153, 51, 255},
                new byte[4]{153, 153, 0, 255},
                new byte[4]{153, 102, 255, 255},
                new byte[4]{153, 102, 204, 255},
                new byte[4]{153, 102, 153, 255},
                new byte[4]{153, 102, 102, 255},
                new byte[4]{153, 102, 51, 255},
                new byte[4]{153, 102, 0, 255},
                new byte[4]{153, 51, 255, 255},
                new byte[4]{153, 51, 204, 255},
                new byte[4]{153, 51, 153, 255},
                new byte[4]{153, 51, 102, 255},
                new byte[4]{153, 51, 51, 255},
                new byte[4]{153, 51, 0, 255},
                new byte[4]{153, 0, 255, 255},
                new byte[4]{153, 0, 204, 255},
                new byte[4]{153, 0, 153, 255},
                new byte[4]{153, 0, 102, 255},
                new byte[4]{153, 0, 51, 255},
                new byte[4]{153, 0, 0, 255},
                new byte[4]{102, 255, 255, 255},
                new byte[4]{102, 255, 204, 255},
                new byte[4]{102, 255, 153, 255},
                new byte[4]{102, 255, 102, 255},
                new byte[4]{102, 255, 51, 255},
                new byte[4]{102, 255, 0, 255},
                new byte[4]{102, 204, 255, 255},
                new byte[4]{102, 204, 204, 255},
                new byte[4]{102, 204, 153, 255},
                new byte[4]{102, 204, 102, 255},
                new byte[4]{102, 204, 51, 255},
                new byte[4]{102, 204, 0, 255},
                new byte[4]{102, 153, 255, 255},
                new byte[4]{102, 153, 204, 255},
                new byte[4]{102, 153, 153, 255},
                new byte[4]{102, 153, 102, 255},
                new byte[4]{102, 153, 51, 255},
                new byte[4]{102, 153, 0, 255},
                new byte[4]{102, 102, 255, 255},
                new byte[4]{102, 102, 204, 255},
                new byte[4]{102, 102, 153, 255},
                new byte[4]{102, 102, 102, 255},
                new byte[4]{102, 102, 51, 255},
                new byte[4]{102, 102, 0, 255},
                new byte[4]{102, 51, 255, 255},
                new byte[4]{102, 51, 204, 255},
                new byte[4]{102, 51, 153, 255},
                new byte[4]{102, 51, 102, 255},
                new byte[4]{102, 51, 51, 255},
                new byte[4]{102, 51, 0, 255},
                new byte[4]{102, 0, 255, 255},
                new byte[4]{102, 0, 204, 255},
                new byte[4]{102, 0, 153, 255},
                new byte[4]{102, 0, 102, 255},
                new byte[4]{102, 0, 51, 255},
                new byte[4]{102, 0, 0, 255},
                new byte[4]{51, 255, 255, 255},
                new byte[4]{51, 255, 204, 255},
                new byte[4]{51, 255, 153, 255},
                new byte[4]{51, 255, 102, 255},
                new byte[4]{51, 255, 51, 255},
                new byte[4]{51, 255, 0, 255},
                new byte[4]{51, 204, 255, 255},
                new byte[4]{51, 204, 204, 255},
                new byte[4]{51, 204, 153, 255},
                new byte[4]{51, 204, 102, 255},
                new byte[4]{51, 204, 51, 255},
                new byte[4]{51, 204, 0, 255},
                new byte[4]{51, 153, 255, 255},
                new byte[4]{51, 153, 204, 255},
                new byte[4]{51, 153, 153, 255},
                new byte[4]{51, 153, 102, 255},
                new byte[4]{51, 153, 51, 255},
                new byte[4]{51, 153, 0, 255},
                new byte[4]{51, 102, 255, 255},
                new byte[4]{51, 102, 204, 255},
                new byte[4]{51, 102, 153, 255},
                new byte[4]{51, 102, 102, 255},
                new byte[4]{51, 102, 51, 255},
                new byte[4]{51, 102, 0, 255},
                new byte[4]{51, 51, 255, 255},
                new byte[4]{51, 51, 204, 255},
                new byte[4]{51, 51, 153, 255},
                new byte[4]{51, 51, 102, 255},
                new byte[4]{51, 51, 51, 255},
                new byte[4]{51, 51, 0, 255},
                new byte[4]{51, 0, 255, 255},
                new byte[4]{51, 0, 204, 255},
                new byte[4]{51, 0, 153, 255},
                new byte[4]{51, 0, 102, 255},
                new byte[4]{51, 0, 51, 255},
                new byte[4]{51, 0, 0, 255},
                new byte[4]{0, 255, 255, 255},
                new byte[4]{0, 255, 204, 255},
                new byte[4]{0, 255, 153, 255},
                new byte[4]{0, 255, 102, 255},
                new byte[4]{0, 255, 51, 255},
                new byte[4]{0, 255, 0, 255},
                new byte[4]{0, 204, 255, 255},
                new byte[4]{0, 204, 204, 255},
                new byte[4]{0, 204, 153, 255},
                new byte[4]{0, 204, 102, 255},
                new byte[4]{0, 204, 51, 255},
                new byte[4]{0, 204, 0, 255},
                new byte[4]{0, 153, 255, 255},
                new byte[4]{0, 153, 204, 255},
                new byte[4]{0, 153, 153, 255},
                new byte[4]{0, 153, 102, 255},
                new byte[4]{0, 153, 51, 255},
                new byte[4]{0, 153, 0, 255},
                new byte[4]{0, 102, 255, 255},
                new byte[4]{0, 102, 204, 255},
                new byte[4]{0, 102, 153, 255},
                new byte[4]{0, 102, 102, 255},
                new byte[4]{0, 102, 51, 255},
                new byte[4]{0, 102, 0, 255},
                new byte[4]{0, 51, 255, 255},
                new byte[4]{0, 51, 204, 255},
                new byte[4]{0, 51, 153, 255},
                new byte[4]{0, 51, 102, 255},
                new byte[4]{0, 51, 51, 255},
                new byte[4]{0, 51, 0, 255},
                new byte[4]{0, 0, 255, 255},
                new byte[4]{0, 0, 204, 255},
                new byte[4]{0, 0, 153, 255},
                new byte[4]{0, 0, 102, 255},
                new byte[4]{0, 0, 51, 255},
                new byte[4]{238, 0, 0, 255},
                new byte[4]{221, 0, 0, 255},
                new byte[4]{187, 0, 0, 255},
                new byte[4]{170, 0, 0, 255},
                new byte[4]{136, 0, 0, 255},
                new byte[4]{119, 0, 0, 255},
                new byte[4]{85, 0, 0, 255},
                new byte[4]{68, 0, 0, 255},
                new byte[4]{34, 0, 0, 255},
                new byte[4]{17, 0, 0, 255},
                new byte[4]{0, 238, 0, 255},
                new byte[4]{0, 221, 0, 255},
                new byte[4]{0, 187, 0, 255},
                new byte[4]{0, 170, 0, 255},
                new byte[4]{0, 136, 0, 255},
                new byte[4]{0, 119, 0, 255},
                new byte[4]{0, 85, 0, 255},
                new byte[4]{0, 68, 0, 255},
                new byte[4]{0, 34, 0, 255},
                new byte[4]{0, 17, 0, 255},
                new byte[4]{0, 0, 238, 255},
                new byte[4]{0, 0, 221, 255},
                new byte[4]{0, 0, 187, 255},
                new byte[4]{0, 0, 170, 255},
                new byte[4]{0, 0, 136, 255},
                new byte[4]{0, 0, 119, 255},
                new byte[4]{0, 0, 85, 255},
                new byte[4]{0, 0, 68, 255},
                new byte[4]{0, 0, 34, 255},
                new byte[4]{0, 0, 17, 255},
                new byte[4]{238, 238, 238, 255},
                new byte[4]{221, 221, 221, 255},
                new byte[4]{187, 187, 187, 255},
                new byte[4]{170, 170, 170, 255},
                new byte[4]{136, 136, 136, 255},
                new byte[4]{119, 119, 119, 255},
                new byte[4]{85, 85, 85, 255},
                new byte[4]{68, 68, 68, 255},
                new byte[4]{34, 34, 34, 255},
                new byte[4]{17, 17, 17, 255}}
        };
        #endregion

		public VoxelPalette()
		{
            this.numColors = 255;
			colors = new byte[256][];
            colors[0] = new byte[] { 0, 0, 0, 0 };
        }

        public VoxelPalette(int numColors)
        {
            this.numColors = numColors;
            colors = new byte[numColors+1][];
            colors[0] = new byte[]{0,0,0,0};
        }

        /// <summary>
        /// Generates a Color32 for the color at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Color32 MakeColorFromIndex(byte index)
        {
            if (index > numColors) return Color.clear;
            return new Color32(colors[index][0], colors[index][1], colors[index][2], colors[index][3]);
        }


        Material mat;

        Texture2D texture;
        bool textureDirty = false;

        /// <summary>
        /// Creates and stores a Material with this model's palette. Can reduce memory for multiple instances of the same model.
        /// </summary>
        /// <param name="baseMaterial">The base material to derive this material from.</param>
        /// <returns></returns>
        public Material ToMaterial(Material baseMaterial, bool useSharedMaterial = true)
        {
            if (useSharedMaterial)
            {
                if (mat != null && !textureDirty) return mat;
                mat = baseMaterial;
                baseMaterial.mainTexture = ToTexture();
                return mat;
            }else
            {
                baseMaterial.mainTexture = ToTexture();
                return baseMaterial;
            }
        }

        /// <summary>
        /// Generates a 256x1 texture of the palette.
        /// </summary>
        /// <returns></returns>
        public Texture2D ToTexture()
        {
            if (!textureDirty && texture != null) return texture;

            Texture2D tex = new Texture2D(256, 1);
            
            Color32[] cols = new Color32[256];
            
            for (int i = 0; i < numColors+1; i++)
            {
                cols[i] = new Color32(colors[i][0], colors[i][1], colors[i][2], colors[i][3]);
            }
            tex.SetPixels32(cols);

            tex.Apply();
            return texture=tex;
        }

	}
}
