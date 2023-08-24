using UnityEngine;

using SamBoyer.VoxelEngine;
using System.IO;

public class VoxelModelLibraryPreloader : MonoBehaviour
{
    [Tooltip("A list of models or directories of models to preload.")]
    public string[] ResourceModelsToLoad;

    private void Start()
    {
        LoadModels();    
    }

    void LoadModels()
    {
        foreach(string path in ResourceModelsToLoad)
        {
            TextAsset[] assets = Resources.LoadAll<TextAsset>("models/" + path);
            foreach(TextAsset asset in assets)
            {
                Stream s = new MemoryStream(asset.bytes);

                VoxelModel mdl = VoxelModel.ReadVoxelModel(new BinaryReader(s), asset.name);

                //mdl.modelCenter = ResourceCenter;
                //print(asset.name);
                VoxelModelLibrary.AddModelToLibrary(mdl, true);
            }
        }
        print(VoxelModelLibrary.Library.Count+" VoxelModels preloaded.");
    }
}
