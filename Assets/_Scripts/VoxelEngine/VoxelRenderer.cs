using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

namespace SamBoyer.VoxelEngine{ 
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class VoxelRenderer : MonoBehaviour {
    
        public VoxelModel Model { get { return model; } set { model = value; OnModelChanged(); } }
        VoxelModel model;

        void OnModelChanged()
        {
            UpdateMesh();
            UpdatePalette();
        }

        [Tooltip("Should this VoxelRenderer build its own model, or wait for it to be given?")]
        public bool autoLoadModel = true;
        public string ResourcePath="";

        public bool overrideCenter = false;
        public Vector3 ResourceCenter;

        public SharedPalette sharedPalette;

        [Tooltip("Materials are shared for efficiency, use this if this object needs a different material (e.g. wireframe)")]
        public bool customMaterial = false;

        public static bool loggingEnabled = false;
        static void Log(object message){
            if(loggingEnabled){
                Debug.Log(message);
            }
        }

        private void Start()
        {
            if (autoLoadModel && model==null && ResourcePath != "")
            {
                string pathEnd = ResourcePath.Substring(ResourcePath.LastIndexOf('/') + 1);

                if (VoxelModelLibrary.Library.ContainsKey(pathEnd)){
                    model = VoxelModelLibrary.GetVoxelModel(pathEnd);
                }
                else
                {
                    TextAsset file = Resources.Load("models/" + ResourcePath) as TextAsset;
                    if (file == null)
                    {
                        Debug.Log("model resource " + ResourcePath + " not found.");
                    }
                    Stream s = new MemoryStream(file.bytes);

                    model = VoxelModel.ReadVoxelModel(new BinaryReader(s), pathEnd);
                    if (overrideCenter)
                    {
                        model.modelCenter = ResourceCenter;
                    }

                    VoxelModelLibrary.AddModelToLibrary(model, true);
                }

                if (sharedPalette != null)
                {
                    if (sharedPalette.palette == null) sharedPalette.palette = model.palette;
                    else
                    {
                        sharedPalette.palette.AppendSimplifiedPalette(model.palette);
                    }
                }
                UpdateMesh();
                UpdatePalette();
            }
            if (spriteRend != null)
            {
                spriteRend.sprite = model.voxelMesh.billboards[2];
                spriteRend.sortingOrder = model.voxelMesh.billboardSharedOrder; // dynamic sorting order to save thousands of setpasses!
                spriteRend.gameObject.name = model.modelName;
            }
        }

        public SpriteRenderer spriteRend;

        public void UpdateMesh()
        {
            Destroy(GetComponent<MeshFilter>().mesh);

            //model.voxelMesh.UpdateMesh();

            GetComponent<MeshFilter>().mesh = model.voxelMesh.mesh;

            MeshCollider mc;
            if ((mc = GetComponent<MeshCollider>()) != null) mc.sharedMesh = model.voxelMesh.mesh;
        }

        public void UpdatePalette()
        {
            GetComponent<MeshRenderer>().material = model.palette.ToMaterial(GetComponent<MeshRenderer>().material, !customMaterial);  
        }
    }
}