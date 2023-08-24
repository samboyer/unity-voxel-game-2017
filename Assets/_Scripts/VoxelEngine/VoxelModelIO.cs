using System;
using System.IO;
using UnityEngine;

namespace SamBoyer.VoxelEngine
{
    public partial class VoxelModel
    {
        const int VOXVersionNumber = 150; //version number for writing files

        const int SBVOXVersionNumber = 1;

        static class MagicNumbers //integer representations of 4 character words. Used to validate and read voxel files.
        {
            public static int XRAW = 1463898712;
            public static int VOX = 542658390; //(4th character is a space)
            public static int MAIN = 1313423693;
            public static int PACK = 1262698832;
            public static int SIZE = 1163544915;
            public static int XYZI = 1230657880;
            public static int RGBA = 1094862674;
            public static int MATT = 1414807885;

            public static int SBVX = 1482048083;
            public static int CENT = 1414415683;
        }

        /// <summary>
        /// Opens either a .sbvox, .vox or .xraw file and parses into a VoxelModel.
        /// </summary>
        /// <param name="reader">A BinaryReader of the voxel data</param>
        public static VoxelModel ReadVoxelModel(BinaryReader reader, string name)
        {
            reader.BaseStream.Position = 0;
            int header = reader.ReadInt32();
            reader.BaseStream.Position = 0;

            if (header == MagicNumbers.SBVX) return ReadSBVOXBinary(reader, name);
            else if (header == MagicNumbers.VOX) return ReadVOXBinary(reader, name);
            else if (header == MagicNumbers.XRAW) return ReadXRAWBinary(reader, name);

            return null;
        }

        #region SBVOX



        /// <summary>
        /// Opens a .sbvox file and parses into a VoxelModel.
        /// </summary>
        /// <param name="path">The path to the .sbvox file.</param>
        /// <returns>A VoxelModel of the .sbvox file's model</returns>
        public static VoxelModel OpenSBVOXFile(string path)
        {
            BinaryReader reader;

            try
            {
                var filestream = File.Open(path, FileMode.Open);
                reader = new BinaryReader(filestream);
            }
            catch
            {
                return null;
            }
            VoxelModel m = ReadSBVOXBinary(reader, path);
            reader.BaseStream.Close();
            return m;
        }

        /// <summary>
        /// Parses .sbvox file data from a BinaryReader into a VoxelModel.\nIf you need a file being opened, use VoxelEngine.OpenSBVOXFile instead.
        /// </summary>
        /// <param name="reader">An BinaryReader of the .sbvox data AT THE START OF THE FILE</param>
        public static VoxelModel ReadSBVOXBinary(BinaryReader reader, string name)
        {
            if (reader.ReadInt32() != MagicNumbers.SBVX) //magic number ('SBVX') is invalid
            {
                return null;
            }

            int versionNumber = reader.ReadInt32(); //unused atm.

            if (reader.ReadInt32() != MagicNumbers.MAIN) //magic number ('MAIN') is invalid
            {
                return null;
            }

            if (reader.ReadInt32() != 0) { return null; } //MAIN chunk has internal data...

            int mainChildBytes = reader.ReadInt32();
            long endbyte = reader.BaseStream.Position + mainChildBytes;

            VoxelModel model = new VoxelModel();
            model.modelName = name;
            bool paletteSet = false;

            while (reader.BaseStream.Position < endbyte) //iterate through internal chunks
            {
                int chunkID = reader.ReadInt32();
                int contentBytes = reader.ReadInt32();
                int childBytes = reader.ReadInt32();

                if (chunkID == MagicNumbers.SIZE)
                {
                    model.size = new ModelDimensions(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                    //model.voxels = new byte[model.size.x,model.size.y,model.size.z];
                    model.voxelsFlat = new byte[model.size.volume];
                }
                else if (chunkID == MagicNumbers.XYZI)
                {
                    int numVoxels = reader.ReadInt32();

                    model.voxelCount = numVoxels;
                    //voxel placement in VOX files declare their positions as well as their color index.
                    for (int i = 0; i < numVoxels; i++)
                    {
                        byte xPos = reader.ReadByte();
                        byte yPos = reader.ReadByte();
                        byte zPos = reader.ReadByte();
                        byte index = reader.ReadByte();
                        if (xPos < 0 || yPos < 0 || zPos < 0 || xPos >= model.size.x || yPos >= model.size.y || zPos >= model.size.z)
                        {
                            continue;
                        }
                        else
                        {
                            //model.voxels[xPos,yPos,zPos] = index;

                            model[xPos, yPos, zPos] = index;
                        }
                    }
                }
                else if (chunkID == MagicNumbers.RGBA)
                {
                    //NOTE ABOUT SBVOX: palette can be any size. (0,0,0,0) need not be present in the file, and will automatically be allocated to VoxelPalette.palette[0].
                    //this means that the 1st colour in the file will be palette[1], and so on.

                    int numColours = contentBytes / 4;
                    if (numColours > 255) return null;

                    model.palette = new VoxelPalette(numColours);

                    model.palette.colors[0] = new byte[4] { 0, 0, 0, 0 };

                    for (int i = 0; i < numColours; i++)
                    {
                        byte[] col = BitConverter.GetBytes(reader.ReadUInt32());

                        if (!BitConverter.IsLittleEndian) Array.Reverse(col);

                        model.palette.colors[i + 1] = col;
                    }
                    paletteSet = true;
                }
                else if (chunkID == MagicNumbers.CENT)
                {
                    model.modelCenter = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                }
                else //any unsupported chunk.
                {
                    reader.BaseStream.Seek(contentBytes + childBytes, SeekOrigin.Current); //skip ahead through any content or children it has.
                }
            }
            if (!paletteSet) model.palette = VoxelPalette.defaultPalette;

            model.voxelMesh = new VoxelMesh(model);
            return model;
        }

        /// <summary>
        /// Exports this model in a .sbvox format.
        /// </summary>
        /// <returns>A MemoryStream containing .sbvox data.</returns>
        public MemoryStream ExportToSBVOX()
        {
            this.RecalculateVoxelCount();
            //this.palette.SimplifyPalette();

            //determine MAIN children byte size
            //SIZE: 24 bytes (12 header, 12 size)
            //CENT: 24 bytes (12 header, 3*4b floats)
            //XYZI: 12 + numVoxels * 4 + 4 (12 header, 4 bytes for each voxel, 4 bytes for model size)
            //RGBA: 12 + numColours * 4 
            int mainContentsBytes = 24 + 24 + 16 + this.voxelCount * 4 + 12 + palette.numColors*4;

            BinaryWriter writer;
            MemoryStream memStream;
            try
            {
                memStream = new MemoryStream();
                writer = new BinaryWriter(memStream);
            }
            catch //file couldn't be opened/created
            {
                return null;
            }

            //start writing
            writer.Write(MagicNumbers.SBVX);
            writer.Write(SBVOXVersionNumber); //version number

            //MAIN
            writer.Write(MagicNumbers.MAIN);
            writer.Write((int)0); //MAIN contents bytes
            writer.Write(mainContentsBytes); //MAIN child bytes

            //SIZE
            writer.Write(MagicNumbers.SIZE);
            writer.Write((int)12);
            writer.Write((int)0);
            writer.Write(this.size.x);
            writer.Write(this.size.y);
            writer.Write(this.size.z);

            //CENT
            writer.Write(MagicNumbers.CENT);
            writer.Write((int)12);
            writer.Write((int)0);
            writer.Write(this.modelCenter.x);
            writer.Write(this.modelCenter.y);
            writer.Write(this.modelCenter.z);

            //XYZI
            writer.Write(MagicNumbers.XYZI);
            writer.Write(this.voxelCount * 4 + 4); //content bytes (plus 4 for numVoxels)
            writer.Write((int)0);
            writer.Write(this.voxelCount); //number of voxels

            int voxelsWritten = 0;

            for (byte z = 0; z < this.size.z; z++) //note: using byte for iterator might look a little weird but thr XYZI data is written in individual bytes. The dimensions can never exceed 255 using this file format :/
            {
                for (byte y = 0; y < this.size.y; y++)
                {
                    for (byte x = 0; x < this.size.x; x++)
                    {
                        byte index = this[x, y, z];
                        if (index != 0)
                        {
                            writer.Write(x);
                            writer.Write(y);
                            writer.Write(z);
                            writer.Write(index);
                            voxelsWritten++;
                        }
                    }
                }
            }

            if (voxelsWritten != this.voxelCount) { UnityEngine.Debug.Log("VOXELS WRITTEN NOT EQUAL TO VOXEL COUNT"); return null; } //shouldn't happen but yknow

            //RGBA
            writer.Write(MagicNumbers.RGBA);
            writer.Write((int)palette.numColors*4); //4 bytes * 256 colours
            writer.Write((int)0);

            for (int c = 1; c <= palette.numColors; c++)
            {
                writer.Write(this.palette.colors[c]);
            }

            memStream.Position = 0;
            return memStream;
        }

        /// <summary>
        /// Saves this model to a .vox file.
        /// </summary>
        /// <param name="path">File path to save in.</param>
        /// <returns>A boolean stating if the operation was successful.</returns>
        public bool SaveAsSBVOXFile(string path)
        {
            FileStream fileStream;
            try
            {
                fileStream = File.Open(path, FileMode.OpenOrCreate);
            }
            catch //file couldn't be opened/created
            {
                return false;
            }

            MemoryStream m = ExportToSBVOX();
            m.WriteTo(fileStream);
            fileStream.Close();
            return true;
        }
        #endregion

        #region VOX

        /// <summary>
        /// Opens a .vox file and parses into a VoxelModel.
        /// </summary>
        /// <param name="path">The path to the .vox file.</param>
        /// <returns>A VoxelModel of the .vox file's model</returns>
        public static VoxelModel OpenVOXFile(string path)
        {
            BinaryReader reader;

            try
            {
                var filestream = File.Open(path, FileMode.Open);
                reader = new BinaryReader(filestream);
            }
            catch
            {
                return null;
            }
            VoxelModel m = ReadVOXBinary(reader, path);
            reader.BaseStream.Close();
            return m;
        }

        /// <summary>
        /// Parses .vox file data from a BinaryReader into a VoxelModel.\nIf you need a file being opened, use VoxelEngine.OpenVOXFile instead.
        /// </summary>
        /// <param name="reader">An BinaryReader of the .vox data AT THE START OF THE FILE</param>
        public static VoxelModel ReadVOXBinary(BinaryReader reader, string name)
        {
            //MATT is not implemented, as well as PACK (i.e. multiple models) :/

            if (reader.ReadInt32() != MagicNumbers.VOX) //magic number ('VOX ') is invalid
            {
                return null;
            }

            int versionNumber = reader.ReadInt32(); //unused atm.

            if (reader.ReadInt32() != MagicNumbers.MAIN) //magic number ('MAIN') is invalid
            {
                return null;
            }

            if (reader.ReadInt32() != 0) { return null; } //MAIN chunk has internal data...

            int mainChildBytes = reader.ReadInt32();
            long endbyte = reader.BaseStream.Position + mainChildBytes;

            VoxelModel model = new VoxelModel();
            model.modelName = name;

            bool paletteSet = false;

            while (reader.BaseStream.Position < endbyte) //iterate through internal chunks
            {
                int chunkID = reader.ReadInt32();
                int contentBytes = reader.ReadInt32();
                int childBytes = reader.ReadInt32();

                if (chunkID == MagicNumbers.SIZE)
                {
                    model.size = new ModelDimensions(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                    //model.voxels = new byte[model.size.x,model.size.y,model.size.z];
                    model.voxelsFlat = new byte[model.size.volume];
                }
                else if (chunkID == MagicNumbers.XYZI)
                {
                    int numVoxels = reader.ReadInt32();

                    model.voxelCount = numVoxels;
                    //voxel placement in VOX files declare their positions as well as their color index.
                    for (int i = 0; i < numVoxels; i++)
                    {
                        byte xPos = reader.ReadByte();
                        byte yPos = reader.ReadByte();
                        byte zPos = reader.ReadByte();
                        byte index = reader.ReadByte();
                        if (xPos < 0 || yPos < 0 || zPos < 0 || xPos >= model.size.x || yPos >= model.size.y || zPos >= model.size.z)
                        {
                            continue;
                        }
                        else
                        {
                            //model.voxels[xPos,yPos,zPos] = index;

                            model[xPos, yPos, zPos] = index;
                        }
                    }
                }
                else if (chunkID == MagicNumbers.RGBA)
                {
                    if (contentBytes != 1024) //uh oh, palette isn't 256xRGBA...
                    {
                        return null;
                    }

                    model.palette = new VoxelPalette();
                    //VERY important! there are only 255 colours declared. color[0] must always be (0,0,0,0), but this is the last color in the palette here.

                    //model.palette.colors[0] = new byte[4] { 0, 0, 0, 0 };

                    for (int i = 0; i < 255; i++)
                    {
                        byte[] col = BitConverter.GetBytes(reader.ReadUInt32());

                        if (!BitConverter.IsLittleEndian)
                        {
                            Array.Reverse(col);
                        }
                        model.palette.colors[i + 1] = col;
                    }
                    reader.ReadInt32(); //to get to the end of the chunk. 
                    paletteSet = true;
                }
                else //any unsupported chunk.
                {
                    reader.BaseStream.Seek(contentBytes + childBytes, SeekOrigin.Current); //skip ahead through any content or children it has.
                }
            }
            if (!paletteSet) model.palette = VoxelPalette.defaultPalette;
            else
                model.palette.SimplifyPalette();
            model.voxelMesh = new VoxelMesh(model);

            return model;
        }


        /// <summary>
        /// Exports this model in a .vox format.
        /// </summary>
        /// <returns>A MemoryStream containing .vox data.</returns>
        public MemoryStream ExportToVox()
        {
            this.RecalculateVoxelCount();

            bool packPalette = !this.palette.Equals(VoxelPalette.defaultPalette); //if the palette is the same as the default palette, don't bother packing it

            //determine MAIN children byte size
            //SIZE: 24 bytes (12 header, 12 size)
            //XYZI: 12 + numVoxels * 4 + 4(12 header, 4 bytes for each voxel, 4 bytes for model size)
            //RGBA (if not default palette): 12 + 256*4 = 1036 (12 header, 256 colours) 
            int mainContentsBytes = 24 + 16 + this.voxelCount * 4 + (packPalette ? 1036 : 0);


            BinaryWriter writer;
            MemoryStream memStream;
            try
            {
                memStream = new MemoryStream();
                writer = new BinaryWriter(memStream);
            }
            catch //file couldn't be opened/created
            {
                return null;
            }

            //start writing
            writer.Write(MagicNumbers.VOX); //'VOX ' magic number
            writer.Write(VOXVersionNumber); //version number

            //MAIN
            writer.Write(MagicNumbers.MAIN);
            writer.Write((int)0); //MAIN contents bytes
            writer.Write(mainContentsBytes); //MAIN child bytes

            //SIZE
            writer.Write(MagicNumbers.SIZE);
            writer.Write((int)12);
            writer.Write((int)0);
            writer.Write(this.size.x);
            writer.Write(this.size.y);
            writer.Write(this.size.z);

            //XYZI
            writer.Write(MagicNumbers.XYZI);
            writer.Write(this.voxelCount * 4 + 4); //content bytes (plus 4 for numVoxels)
            writer.Write((int)0);
            writer.Write(this.voxelCount); //number of voxels

            int voxelsWritten = 0;

            for (byte z = 0; z < this.size.z; z++) //note: using byte for iterator might look a little weird but thr XYZI data is written in individual bytes. The dimensions can never exceed 255 using this file format :/
            {
                for (byte y = 0; y < this.size.y; y++)
                {
                    for (byte x = 0; x < this.size.x; x++)
                    {
                        byte index = this[x, y, z];
                        if (index != 0)
                        {
                            writer.Write(x);
                            writer.Write(y);
                            writer.Write(z);
                            writer.Write(index);
                            voxelsWritten++;
                        }
                    }
                }
            }

            if (voxelsWritten != this.voxelCount) { UnityEngine.Debug.Log("VOXELS WRITTEN NOT EQUAL TO VOXEL COUNT"); return null; } //shouldn't happen but yknow

            //RGBA (if needed)
            if (packPalette)
            {
                writer.Write(MagicNumbers.RGBA);
                writer.Write((int)1024); //4 bytes * 256 colours
                writer.Write((int)0);

                for (int c = 1; c < 256; c++)
                {
                    writer.Write(this.palette.colors[c]);
                }
                writer.Write(0); //transparent colour comes last for some reason :/
            }
            memStream.Position = 0;
            return memStream;
        }

        /// <summary>
        /// Saves this model to a .vox file.
        /// </summary>
        /// <param name="path">File path to save in.</param>
        /// <returns>A boolean stating if the operation was successful.</returns>
        public bool SaveAsVOXFile(string path)
        {
            FileStream fileStream;
            try
            {
                fileStream = File.Open(path, FileMode.OpenOrCreate);
            }
            catch //file couldn't be opened/created
            {
                return false;
            }

            MemoryStream m = ExportToVox();
            m.WriteTo(fileStream);
            fileStream.Close();
            return true;
        }
        #endregion

        #region XRAW

        /// <summary>
        /// Opens a .xraw file and parses into a VoxelModel.
        /// </summary>
        /// <param name="path">The path to the .xraw file.</param>
        /// <returns>A VoxelModel of the .xraw file's model</returns>
        public static VoxelModel OpenXRAWFile(string path, string name)
        {
            BinaryReader reader;

            try
            {
                var filestream = File.Open(path, FileMode.Open);
                reader = new BinaryReader(filestream);
            }
            catch
            {
                return null;
            }

            VoxelModel m = ReadXRAWBinary(reader, name);
            reader.BaseStream.Close();
            return m;
        }

        /// <summary>
        /// Parses .xraw file data from a BinaryReader into a VoxelModel.\nIf you need a file being opened, use VoxelEngine.OpenXRAWFile instead.
        /// </summary>
        /// <param name="reader">An BinaryReader of the .xraw data AT THE START OF THE FILE</param>
        public static VoxelModel ReadXRAWBinary(BinaryReader reader, string name)
        {
            if (reader.ReadInt32() != MagicNumbers.XRAW) //magic number ('XRAW') is invalid
            {
                return null;
            }

            // header, only used for verification
            int elemDataType = reader.ReadByte();
            int numChannels = reader.ReadByte();
            int numBitsPerElem = reader.ReadByte();
            int numBitsPerIndex = reader.ReadByte();

            if (elemDataType != 0 || numChannels != 4 || numBitsPerElem != 8 || numBitsPerIndex != 8) { return null; }

            //model dimensions
            ModelDimensions dimensions = new ModelDimensions(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
            if (dimensions.x <= 0 || dimensions.y <= 0 || dimensions.z <= 0) { return null; }

            int paletteSize = reader.ReadInt32();
            if (paletteSize != 256) { return null; }

            VoxelModel model = new VoxelModel();
            model.modelName = name;
            model.size = dimensions;

            //read voxels
            //model.voxels = new byte[dimensions.x, dimensions.y, dimensions.z];
            model.voxelsFlat = new byte[model.size.volume];

            for (int z = 0; z < dimensions.z; z++)
            {
                for (int y = 0; y < dimensions.y; y++)
                {
                    for (int x = 0; x < dimensions.x; x++)
                    {
                        model[x, y, z] = reader.ReadByte();
                    }
                }
            }

            //read palette
            model.palette = new VoxelPalette();
            for (int i = 0; i < 256; i++)
            {
                byte[] col = BitConverter.GetBytes(reader.ReadUInt32());

                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(col);
                }
                model.palette.colors[i] = col;
            }

            //calculate voxelCount
            int count = 0;
            foreach (byte index in model.voxelsFlat)
            {
                if (index != 0) { count++; }
            }
            model.voxelCount = count;
            model.voxelMesh = new VoxelMesh(model);
            return model;
        }

        #endregion

    }
}
