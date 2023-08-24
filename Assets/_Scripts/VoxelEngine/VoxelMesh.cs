using System.Collections.Generic;

using UnityEngine;

namespace SamBoyer.VoxelEngine
{
    public class VoxelMesh
    {
        public enum MeshGenMethod { Culled, Greedy}

        public Mesh mesh {
            get { if (_mesh == null || meshDirty) return Update(); return _mesh; }
            set { _mesh = value; }
        }
        Mesh _mesh;

        public Sprite[] billboards
        {
            get {
                if (_billboards == null || meshDirty) {
                    Update();
                }
                return _billboards;
            }
            set { _billboards = value; }
        }
        Sprite[] _billboards;

        public bool meshDirty = false;

        VoxelModel model;

        public VoxelMesh(VoxelModel model)
        {
            this.model = model;
        }

        public Mesh Update()
        {
            billboards = MakeBillboards(model);
            return mesh = MeshGenAlgorithms.GenerateGreedyMesh(model, false);
        }

        #region MESHGENS

        class MeshGenAlgorithms
        {
            static Vector3[] cubeCorners = new Vector3[8]{
            new Vector3(0,0,0),
            new Vector3(0,0,1),
            new Vector3(0,1,0),
            new Vector3(0,1,1),
            new Vector3(1,0,0),
            new Vector3(1,0,1),
            new Vector3(1,1,0),
            new Vector3(1,1,1)};

            public static Mesh GenerateCulledMesh(VoxelModel model)
            {
                Mesh mesh = new Mesh();

                int maxVertexCount = model.voxelCount * 6 * 2 * 3; //voxelCount number of cubes, 6 faces per cube (for now), 2 tris per face

                Vector3[] vertices = new Vector3[maxVertexCount];
                Vector2[] uvs = new Vector2[maxVertexCount];
                int[] triangles = new int[maxVertexCount];

                int vertCounter = 0; //this is used as an index for vertices and to resize the array.
                int triCounter = 0; //counts the entries into the triangles array.
                for (int z = 0; z < model.size.z; z++)
                {
                    for (int y = 0; y < model.size.y; y++)
                    {
                        for (int x = 0; x < model.size.x; x++)
                        {
                            byte voxVal = model[x, y, z];
                            if (voxVal != 0) //if there's solid voxel here,
                            {
                                Vector3 voxelOffset = new Vector3(x, z, y); //get a 3D offset for this voxel.
                                                                            //(Unity is Y-up whereas Magica is Z-up)

                                Vector2 texCoord = new Vector2((model[x, y, z] + 0.5f) / 256, 0.5f); //get a texture coordinate from color index

                                if (y == model.size.y - 1 || model[x, y + 1, z] == 0) //if the front face is visible
                                {
                                    //verts
                                    vertices[vertCounter] = voxelOffset + cubeCorners[1]; //FRONT
                                    vertices[vertCounter + 1] = voxelOffset + cubeCorners[5];
                                    vertices[vertCounter + 2] = voxelOffset + cubeCorners[7];
                                    vertices[vertCounter + 3] = voxelOffset + cubeCorners[3];
                                    //tris
                                    triangles[triCounter] = vertCounter;
                                    triangles[triCounter + 1] = vertCounter + 1;
                                    triangles[triCounter + 2] = vertCounter + 2;
                                    triangles[triCounter + 3] = vertCounter + 3;
                                    triangles[triCounter + 4] = vertCounter;
                                    triangles[triCounter + 5] = vertCounter + 2;

                                    //uvs
                                    uvs[vertCounter] = texCoord;
                                    uvs[vertCounter + 1] = texCoord;
                                    uvs[vertCounter + 2] = texCoord;
                                    uvs[vertCounter + 3] = texCoord;
                                    vertCounter += 4;
                                    triCounter += 6;
                                }
                                if (y == 0 || model[x, y - 1, z] == 0) //if the back face is visible
                                {
                                    vertices[vertCounter] = voxelOffset + cubeCorners[4]; //BACK
                                    vertices[vertCounter + 1] = voxelOffset + cubeCorners[0];
                                    vertices[vertCounter + 2] = voxelOffset + cubeCorners[6];
                                    vertices[vertCounter + 3] = voxelOffset + cubeCorners[2];
                                    //tris
                                    triangles[triCounter] = vertCounter;
                                    triangles[triCounter + 1] = vertCounter + 1;
                                    triangles[triCounter + 2] = vertCounter + 2;
                                    triangles[triCounter + 3] = vertCounter + 1;
                                    triangles[triCounter + 4] = vertCounter + 3;
                                    triangles[triCounter + 5] = vertCounter + 2;
                                    //uvs
                                    uvs[vertCounter] = texCoord;
                                    uvs[vertCounter + 1] = texCoord;
                                    uvs[vertCounter + 2] = texCoord;
                                    uvs[vertCounter + 3] = texCoord;
                                    vertCounter += 4;
                                    triCounter += 6;
                                }
                                if (x == 0 || model[x - 1, y, z] == 0) //if the right face is visible
                                {
                                    vertices[vertCounter] = voxelOffset + cubeCorners[2]; //RIGHT
                                    vertices[vertCounter + 1] = voxelOffset + cubeCorners[0];
                                    vertices[vertCounter + 2] = voxelOffset + cubeCorners[3];
                                    vertices[vertCounter + 3] = voxelOffset + cubeCorners[1];
                                    //tris
                                    triangles[triCounter] = vertCounter;
                                    triangles[triCounter + 1] = vertCounter + 1;
                                    triangles[triCounter + 2] = vertCounter + 2;
                                    triangles[triCounter + 3] = vertCounter + 1;
                                    triangles[triCounter + 4] = vertCounter + 3;
                                    triangles[triCounter + 5] = vertCounter + 2;
                                    //uvs
                                    uvs[vertCounter] = texCoord;
                                    uvs[vertCounter + 1] = texCoord;
                                    uvs[vertCounter + 2] = texCoord;
                                    uvs[vertCounter + 3] = texCoord;

                                    vertCounter += 4;
                                    triCounter += 6;
                                }
                                if (x == model.size.x - 1 || model[x + 1, y, z] == 0) //if the left face is visible
                                {
                                    vertices[vertCounter] = voxelOffset + cubeCorners[4]; //LEFT
                                    vertices[vertCounter + 1] = voxelOffset + cubeCorners[6];
                                    vertices[vertCounter + 2] = voxelOffset + cubeCorners[7];
                                    vertices[vertCounter + 3] = voxelOffset + cubeCorners[5];

                                    //tris
                                    triangles[triCounter] = vertCounter;
                                    triangles[triCounter + 1] = vertCounter + 1;
                                    triangles[triCounter + 2] = vertCounter + 2;
                                    triangles[triCounter + 3] = vertCounter + 3;
                                    triangles[triCounter + 4] = vertCounter;
                                    triangles[triCounter + 5] = vertCounter + 2;
                                    //uvs
                                    uvs[vertCounter] = texCoord;
                                    uvs[vertCounter + 1] = texCoord;
                                    uvs[vertCounter + 2] = texCoord;
                                    uvs[vertCounter + 3] = texCoord;

                                    vertCounter += 4;
                                    triCounter += 6;
                                }
                                if (z == model.size.z - 1 || model[x, y, z + 1] == 0) //if the top face is visible
                                {
                                    vertices[vertCounter] = voxelOffset + cubeCorners[6]; //TOP FACE
                                    vertices[vertCounter + 1] = voxelOffset + cubeCorners[2];
                                    vertices[vertCounter + 2] = voxelOffset + cubeCorners[7];
                                    vertices[vertCounter + 3] = voxelOffset + cubeCorners[3];
                                    //tris
                                    triangles[triCounter] = vertCounter;
                                    triangles[triCounter + 1] = vertCounter + 1;
                                    triangles[triCounter + 2] = vertCounter + 2;
                                    triangles[triCounter + 3] = vertCounter + 1;
                                    triangles[triCounter + 4] = vertCounter + 3;
                                    triangles[triCounter + 5] = vertCounter + 2;
                                    //uvs
                                    uvs[vertCounter] = texCoord;
                                    uvs[vertCounter + 1] = texCoord;
                                    uvs[vertCounter + 2] = texCoord;
                                    uvs[vertCounter + 3] = texCoord;

                                    vertCounter += 4;
                                    triCounter += 6;
                                }
                                if (z == 0 || model[x, y, z - 1] == 0) //if the bottom face is visible
                                {
                                    vertices[vertCounter] = voxelOffset + cubeCorners[0]; //BOTTOM FACE
                                    vertices[vertCounter + 1] = voxelOffset + cubeCorners[4];
                                    vertices[vertCounter + 2] = voxelOffset + cubeCorners[5];
                                    vertices[vertCounter + 3] = voxelOffset + cubeCorners[1];

                                    //tris
                                    triangles[triCounter] = vertCounter;
                                    triangles[triCounter + 1] = vertCounter + 1;
                                    triangles[triCounter + 2] = vertCounter + 2;
                                    triangles[triCounter + 3] = vertCounter + 3;
                                    triangles[triCounter + 4] = vertCounter;
                                    triangles[triCounter + 5] = vertCounter + 2;
                                    //uvs
                                    uvs[vertCounter] = texCoord;
                                    uvs[vertCounter + 1] = texCoord;
                                    uvs[vertCounter + 2] = texCoord;
                                    uvs[vertCounter + 3] = texCoord;

                                    vertCounter += 4;
                                    triCounter += 6;
                                }
                            }
                        }
                    }
                }
                if (vertCounter > 65000)
                {
                    UnityEngine.Debug.Log("Vertex limit reached :(");
                    return null;
                }

                //reduce the size of the vertices array and produce a simplistic triangles array... might fix
                Vector3[] newVertices = new Vector3[vertCounter];
                Vector2[] newUvs = new Vector2[vertCounter];
                int[] newTriangles = new int[triCounter];

                for (int i = 0; i < vertCounter; i++)
                {
                    newVertices[i] = vertices[i];
                    newUvs[i] = uvs[i];
                }
                for (int t = 0; t < triCounter; t++)
                {
                    newTriangles[t] = triangles[t];
                }

                mesh.vertices = newVertices;
                mesh.uv = newUvs;
                mesh.triangles = newTriangles;
                mesh.RecalculateNormals(); //cause I can't be bothered to
                mesh.RecalculateBounds();
                mesh.UploadMeshData(false);
                return mesh;
            }

            public static Mesh GenerateGreedyMesh(VoxelModel model, bool makeUv1)
            {
                Vector3 center = model.modelCenter;

                int[] modelDimensions = new int[3] { model.size.x, model.size.y, model.size.z };

                List<Vector3> vertices = new List<Vector3>();
                List<int> triangles = new List<int>();
                List<Vector2> uvs = new List<Vector2>();
                List<Vector2> uv1 = new List<Vector2>();

                int vertCounter = 0;

                for (int flip = 0; flip < 2; flip++)
                {
                    for (int dimension = 0; dimension < 3; dimension++)
                    {
                        int localX = (dimension + 1) % 3;
                        int localY = (dimension + 2) % 3;

                        for (int slice = 0; slice < modelDimensions[dimension]; slice++) //sweep through all slices of this dimension & direction
                        {
                            byte[,] mask = new byte[modelDimensions[localX], modelDimensions[localY]]; //the 2D mask for this slice

                            int[] coord = new int[3];
                            coord[dimension] = slice; //set which slice we're on

                            int[] topFacingCoord = new int[3];
                            topFacingCoord[dimension] = slice + (flip == 0 ? -1 : 1);

                            //iterate through this slice and populate the mask
                            for (int y = 0; y < modelDimensions[localY]; y++)
                            {
                                for (int x = 0; x < modelDimensions[localX]; x++)
                                {
                                    coord[localX] = x;
                                    coord[localY] = y;
                                    topFacingCoord[localX] = x;
                                    topFacingCoord[localY] = y;

                                    byte index1 = model[coord[0], coord[1], coord[2]];

                                    //check to see if this face should be rendered
                                    if (index1 != 0 && ((flip == 0 && slice == 0) || (flip == 1 && slice == modelDimensions[dimension] - 1) || model[topFacingCoord[0], topFacingCoord[1], topFacingCoord[2]] == 0)) //if there's solid voxel here,
                                    {
                                        mask[x, y] = index1;
                                    }
                                }
                            }

                            //work through the mask 'lexicographically', maximising rectangle sizes.
                            for (int y = 0; y < modelDimensions[localY]; y++)
                            {
                                for (int x = 0; x < modelDimensions[localX];)
                                {
                                    coord[dimension] = slice; //since this gets messed with later.

                                    if (mask[x, y] != 0) //if there's a voxel here
                                    {
                                        byte thisIndex = mask[x, y];
                                        Vector2 textureCoord = new Vector2((thisIndex + 0.5f) / 256, 0.5f);

                                        //maximise the rectangle size.
                                        int blockWidth, blockHeight;

                                        for (blockWidth = 1; x + blockWidth < modelDimensions[localX] && mask[x + blockWidth, y] == thisIndex; blockWidth++) ; //maximise width

                                        bool doneExpanding = false;

                                        blockHeight = 1;
                                        for (blockHeight = 1; y + blockHeight < modelDimensions[localY]; blockHeight++) //maximise height
                                        {
                                            for (int blockX = 0; blockX < blockWidth; blockX++)
                                            { //each x value in the width of this rectangle must be checked in order for it to expand downwards
                                                if (mask[x + blockX, y + blockHeight] != thisIndex) { doneExpanding = true; break; } //if it can't expand here, break the loop
                                            }
                                            if (doneExpanding) break;
                                        }

                                        //add this face to the verts/tris list

                                        int[] topRight = new int[3];
                                        int[] bottomLeft = new int[3];
                                        topRight[localX] = blockWidth;
                                        bottomLeft[localY] = blockHeight;

                                        coord[localX] = x;
                                        coord[localY] = y;

                                        coord[dimension] += flip; //nudge the reverse faces upwards

                                        //Vector3 topLeftVector = new Vector3(coord[0], coord[2], coord[1]);
                                        //Vector3 topRightVector = new Vector3(topRight[0], topRight[2], topRight[1]);	//since we don't know the directions of these, we have to use all 3 values :/
                                        //Vector3 bottomLeftVector = new Vector3(bottomLeft[0], bottomLeft[2], bottomLeft[1]);

                                        // 0 1
                                        // 3 2

                                        vertices.Add(new Vector3(coord[0], coord[2], coord[1]) - center);
                                        vertices.Add(new Vector3(coord[0] + topRight[0], coord[2] + topRight[2], coord[1] + topRight[1]) - center);
                                        vertices.Add(new Vector3(coord[0] + bottomLeft[0] + topRight[0], coord[2] + bottomLeft[2] + topRight[2], coord[1] + bottomLeft[1] + topRight[1]) - center);
                                        vertices.Add(new Vector3(coord[0] + bottomLeft[0], coord[2] + bottomLeft[2], coord[1] + bottomLeft[1]) - center);

                                        uvs.Add(textureCoord);
                                        uvs.Add(textureCoord);
                                        uvs.Add(textureCoord);
                                        uvs.Add(textureCoord);

                                        if (makeUv1)
                                        {
                                            uv1.Add(Vector2.zero);
                                            uv1.Add(new Vector2(blockWidth, 0));
                                            uv1.Add(new Vector2(blockWidth, blockHeight));
                                            uv1.Add(new Vector2(0, blockHeight));
                                        }

                                        if (flip == 0)
                                        {
                                            triangles.Add(vertCounter);
                                            triangles.Add(vertCounter + 1);
                                            triangles.Add(vertCounter + 3);
                                            triangles.Add(vertCounter + 1);
                                            triangles.Add(vertCounter + 2);
                                            triangles.Add(vertCounter + 3);
                                        }
                                        else
                                        {
                                            triangles.Add(vertCounter);
                                            triangles.Add(vertCounter + 3);
                                            triangles.Add(vertCounter + 1);
                                            triangles.Add(vertCounter + 1);
                                            triangles.Add(vertCounter + 3);
                                            triangles.Add(vertCounter + 2);
                                        }

                                        vertCounter += 4;

                                        //DON'T FORGET! we need to invalidate this rect's area from the mask.
                                        for (int j = 0; j < blockHeight; j++)
                                        {
                                            for (int i = 0; i < blockWidth; i++)
                                            {
                                                mask[x + i, y + j] = 0;
                                            }
                                        }
                                        x += blockWidth;
                                    }
                                    else
                                    {
                                        x++;
                                    }
                                }
                            }
                        } //end of 'per slice'
                    } //end of 'per dimension'
                } //end of 'per flip'

                Mesh m = new Mesh();
                m.vertices = vertices.ToArray();
                m.triangles = triangles.ToArray();
                m.uv = uvs.ToArray();
                if (makeUv1)
                {
                    m.uv2 = uv1.ToArray();
                }
                m.RecalculateNormals();
                m.UploadMeshData(false);

                return m;
            }
        }
        #endregion

        #region BILLBOARDGEN

        //4 billboards are made for each rotation of the model
        //0=front (z-), 1=left (x-), 2=back (z+), 3=right(x+)
        //

        Sprite[] MakeBillboards(VoxelModel model)
        {
            Sprite[] sprites = new Sprite[4];

            for(int direction = 0; direction<=3; direction++)
            {
                int width = (direction % 2 == 0) ? model.size.x : model.size.y;
                int height = model.size.z;
                int maxDepth = (direction % 2 == 0) ? model.size.y : model.size.x;

                Texture2D t = new Texture2D(width, height, TextureFormat.RGBA32, false);

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        bool coloured = false;
                        for (int depth = 0; depth < maxDepth; depth++)
                        {
                            byte col=0;
                            switch (direction) {
                                case 0:
                                    col = model[x, depth, y];
                                    break;
                                case 1:
                                    col = model[depth, x, y];
                                    break;
                                case 2:
                                    col = model[x, maxDepth-1-depth, y];
                                    break;
                                case 3:
                                    col = model[maxDepth - 1 - depth, x, y];
                                    break;
                            }

                            if (col != 0)
                            {
                                t.SetPixel(x, y, model.palette.MakeColorFromIndex(col));
                                coloured = true;
                                break;
                            }
                        }
                        if (!coloured)
                            t.SetPixel(x, y, Color.clear);
                    }
                }
                t.Apply(false, true);
                t.filterMode = FilterMode.Point;

                sprites[direction] = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0));
                
            }
            billboardSharedOrder = billboardSharedOrderNext;
            billboardSharedOrderNext = (billboardSharedOrderNext+1)%10000;

            return sprites;
        }

        public int billboardSharedOrder;
        static int billboardSharedOrderNext=0;

        #endregion

    }
}
