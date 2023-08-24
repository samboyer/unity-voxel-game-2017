using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SamBoyer.VoxelEngine;


[RequireComponent(typeof(ParticleSystem))]

public class SplodeyVoxels : MonoBehaviour {

    ParticleSystem particles;
    public float velocity = 1;

    private void Start()
    {
        particles = GetComponent<ParticleSystem>();
    }

    public void ExplodeModel(VoxelModel model, float percentage, Vector3 center)
    {
        Vector3 offset = center - model.modelCenter;

        for(int z=0; z<model.size.z; z++)
        {
            for (int y = 0; y < model.size.y; y++)
            {
                for (int x = 0; x < model.size.x; x++)
                {
                    int index = model.GetVoxelColorIndex(x, y, z);

                    if (index != 0 && Random.value < percentage)
                    {
                        
                        ParticleSystem.EmitParams p = new ParticleSystem.EmitParams();
                        byte[] col = model.palette.colors[index];
                        p.startColor = new Color32(col[0], col[1], col[2], 255);
                        p.position = new Vector3(offset.x+x, offset.y+z, offset.z+y);
                        p.velocity = Random.onUnitSphere * velocity;
                        particles.Emit(p, 1);
                    }
                }
            }
        }
    }
    public void ExplodeModel(VoxelRenderer renderer, float percentage)
    {
        if(renderer==null) return;
        ExplodeModel(renderer.Model, percentage, renderer.transform.position);
    }
}
