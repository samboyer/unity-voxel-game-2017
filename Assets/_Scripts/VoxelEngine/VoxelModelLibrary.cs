using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SamBoyer.VoxelEngine;
using System.IO;

namespace SamBoyer.VoxelEngine
{
    /// <summary>
    /// A library to contain VoxelModel references at runtime. This is important to remove duplicate models in memory and improve performance.
    /// </summary>
    public class VoxelModelLibrary
    {

        public static Dictionary<string, VoxelModel> Library = new Dictionary<string, VoxelModel>();

        public static VoxelModel GetVoxelModel(string modelName, bool loadIfMissing = false)
        {
            if (Library.ContainsKey(modelName)) return Library[modelName];
            else {
                if (loadIfMissing)
                    return AddModelFromResources(modelName, true);
                else
                    return null;
            };
        }

        public static void AddModelToLibrary(VoxelModel model, bool preloadMesh)
        {
            if (!Library.ContainsKey(model.modelName)) Library.Add(model.modelName, model);

            if(preloadMesh) model.voxelMesh.Update();
        }

        public static VoxelModel AddModelFromResources(string resourcePath, bool preloadMesh)
        {
            TextAsset file = Resources.Load("models/" + resourcePath) as TextAsset;
            Stream s = new MemoryStream(file.bytes);

            VoxelModel mdl = VoxelModel.ReadVoxelModel(new BinaryReader(s), resourcePath);
            AddModelToLibrary(mdl, preloadMesh);
            return mdl;
        }

        public static VoxelModel AddModelFromResources(string resourcePath, Vector3 offset, bool preloadMesh)
        {
            TextAsset file = Resources.Load("models/" + resourcePath) as TextAsset;
            Stream s = new MemoryStream(file.bytes);

            VoxelModel mdl = VoxelModel.ReadVoxelModel(new BinaryReader(s), resourcePath);
            mdl.modelCenter = offset;
            AddModelToLibrary(mdl, preloadMesh);
            return mdl;
        }

        public static VoxelModel AddModelFromSaveFile(string resourcePath, SaveFileManager saveFile, bool preloadMesh)
        {
            VoxelModel mdl = saveFile.OpenVoxelModel(resourcePath);
            if (mdl != null)
            {
                AddModelToLibrary(mdl, preloadMesh);
                return mdl;
            }
            else //try and find in resources??
            {
                return AddModelFromResources(resourcePath, preloadMesh);
            }
        }
    }

}
