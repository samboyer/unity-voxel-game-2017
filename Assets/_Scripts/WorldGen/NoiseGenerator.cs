using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGenerator : MonoBehaviour
{
    #region HASH
    private static int[] hash = {
		151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
		140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
		247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
		 57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
		 74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
		 60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
		 65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
		200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
		 52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
		207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
		119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
		129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
		218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
		 81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
		184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
		222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180,
        151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
		140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
		247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
		 57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
		 74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
		 60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
		 65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
		200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
		 52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
		207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
		119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
		129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
		218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
		 81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
		184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
		222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180
	};
    #endregion

    private const int hashMask = 255;   //this is a mask of bits (i.e. 0x0000ff). Useful bitwise AND operation, where 'x & hashMask' is the same as 'x % hashMask'


    public static void RebuildHash(int seed)
    {
        int hashSize = 256;
        Random.InitState(seed);

        int[] hash = new int[hashSize * 2];

        //fill values 0-255 with ascending numbers
        for (int i = 0; i < hashSize; i++)
        {
            hash[i] = i;
        }

        //fisher-yates shuffle
        for (int i = 0; i < hashSize - 2; i++)
        {
            int j = Random.Range(i, hashSize);

            int temp = hash[i];
            hash[i] = hash[j];
            hash[hashSize + i] = hash[j];   //add to duplicate list
            hash[hashSize + j] = temp;
            hash[j] = temp;
        }
        NoiseGenerator.hash = hash;
    }


    private static float Smooth(float t)
    {
        return t * t * t * (t * (t * 6f - 15f) + 10f);
    }

    public delegate float NoiseMethod(Vector3 point);
    public static NoiseMethod[] noiseMethods = {PerlinNoise3D, SimplexValueNoise3D, SimplexNoise3D, PerlinNoise2D, SimplexValueNoise2D, SimplexNoise2D, PerlinNoise1D, SimplexValueNoise1D, SimplexNoise1D };
    public enum NoiseMethods
    {
        Perlin3D,
        SimplexValue3D,
        Simplex3D,
        Perlin2D,
        SimplexValue2D,
        Simplex2D,
        Perlin1D,
        SimplexValue1D,
        Simplex1D,
    }

    private static float Dot(Vector2 g, float x, float y)
    {
        return g.x * x + g.y * y;
    }
    private static float Dot(Vector3 g, float x, float y, float z)
    {
        return g.x * x + g.y * y + g.z * z;
    }

    #region SUM_FUNCTIONS

    /// <summary>
    /// Sums multiple octaves of noise. 
    /// </summary>
    /// <param name="method">The NoiseMethod to use.</param>
    /// <param name="point">Point to lookup.</param>
    /// <param name="octaves">Number of octaves to apply.</param>
    /// <param name="lacunarity">How quickly the lower octaves enlarge</param>
    /// <param name="persistence">How much influence the lower octaves have on the sum</param>
    /// <returns></returns>
    public static float Sum(NoiseMethod method, Vector3 point, int octaves, float lacunarity = 2, float persistence = 0.5f)
    {
        float sum = method(point);
        float influence = 1;
        float frequency = 1;
        float range = 1; //need to keep track of this. The more octaves done, the bigger the range becomes (1+0.5+0.25 = 1.75)
        for (int i = 1; i < octaves; i++)
        {
            frequency *= lacunarity;
            influence *= persistence;
            range += influence;
            Vector3 p2 = new Vector3(point.x * frequency, point.y * frequency, point.z * frequency);
            sum += method(p2) * influence;
        }
        return sum / range;
    }

    /// <summary>
    /// Sums multiple octaves of noise. 
    /// </summary>
    /// <param name="method">The NoiseMethod to use.</param>
    /// <param name="point">Point to lookup.</param>
    /// <param name="octaves">Number of octaves to apply.</param>
    /// <param name="lacunarity">How quickly the lower octaves enlarge</param>
    /// <param name="persistence">How much influence the lower octaves have on the sum</param>
    /// <returns></returns>
    public static float SumSimplex2D(Vector3 point, int octaves, float lacunarity = 2, float persistence = 0.5f)
    {
        float sum = SimplexNoise2D(point);
        float influence = 1;
        float frequency = 1;
        float range = 1; //need to keep track of this. The more octaves done, the bigger the range becomes (1+0.5+0.25 = 1.75)
        for (int i = 1; i < octaves; i++)
        {
            frequency *= lacunarity;
            influence *= persistence;
            range += influence;
            Vector3 p2 = new Vector3(point.x * frequency, point.y * frequency, point.z * frequency);
            sum += SimplexNoise2D(p2) * influence;
        }
        return sum / range;
        //return sum;
    }

    #endregion

    #region VALUE_NOISE
    public static float ValueNoise1D(Vector3 point, bool smooth = true)
    {
        if (smooth)
        {
            int i = Mathf.FloorToInt(point.x);
            float t = point.x - i; //fractional part of point.x
            i &= hashMask;
            int h0 = hash[i];
            int h1 = hash[i + 1];
            float sample = Mathf.Lerp(h0, h1, Smooth(t));
            return sample * (2f / hashMask)-1;
        }
        else
        {
            int i = Mathf.FloorToInt(point.x);
            i &= hashMask;
            float sample = hash[i];
            return sample * (2f / hashMask) - 1;
        }
    }

    public static float ValueNoise2D(Vector3 point, bool smooth = true)
    {
        if (smooth)
        {
            int ix = Mathf.FloorToInt(point.x);
            int iy = Mathf.FloorToInt(point.y);
            float tx = point.x - ix;
            float ty = point.y - iy;
            ix &= hashMask;
            iy &= hashMask;

            int h0 = hash[ix];
            int h1 = hash[ix + 1];

            int h00 = hash[h0 + iy];
            int h10 = hash[h1 + iy];
            int h01 = hash[h0 + iy + 1];
            int h11 = hash[h1 + iy + 1];

            float sample = Mathf.Lerp(Mathf.Lerp(h00, h10, tx), Mathf.Lerp(h01, h11, tx), ty);
            return sample * (2f / hashMask)-1;          
        }
        else
        {
            int ix = Mathf.FloorToInt(point.x);
            int iy = Mathf.FloorToInt(point.y);
            ix &= hashMask;
            iy &= hashMask;
            float sample = hash[hash[ix] + iy];
            return sample * (2f / hashMask) - 1;
        }
    }

    public static float ValueNoise3D(Vector3 point, bool smooth = true)
    {
        if (smooth)
        {
            int ix = Mathf.FloorToInt(point.x);
            int iy = Mathf.FloorToInt(point.y);
            int iz = Mathf.FloorToInt(point.z);

            float tx = Smooth(point.x - ix);
            float ty = Smooth(point.y - iy);
            float tz = Smooth(point.z - iz);

            ix &= hashMask;
            iy &= hashMask;
            iz &= hashMask;

            int h0 = hash[ix];
            int h1 = hash[ix + 1];

            int h00 = hash[h0 + iy];
            int h10 = hash[h1 + iy];
            int h01 = hash[h0 + iy + 1];
            int h11 = hash[h1 + iy + 1];

            int h000 = hash[h00 + iz];
            int h100 = hash[h10 + iz];
            int h010 = hash[h01 + iz];
            int h110 = hash[h11 + iz];
            int h001 = hash[h00 + iz+1];
            int h101 = hash[h10 + iz+1];
            int h011 = hash[h01 + iz+1];
            int h111 = hash[h11 + iz+1];

            float sample = Mathf.Lerp(Mathf.Lerp(Mathf.Lerp(h000, h100, tx), Mathf.Lerp(h010, h110, tx), ty), Mathf.Lerp(Mathf.Lerp(h001, h101, tx), Mathf.Lerp(h011, h111, tx), ty), tz);
            return sample * (2f / hashMask) - 1;
        }
        else
        {
            int ix = Mathf.FloorToInt(point.x);
            int iy = Mathf.FloorToInt(point.y);
            int iz = Mathf.FloorToInt(point.z);

            ix &= hashMask;
            iy &= hashMask;
            iz &= hashMask;
            float sample = hash[hash[hash[ix] + iy]+iz];
            return sample * (2f / hashMask) - 1;
        }
    }

    #endregion

    #region PERLIN_NOISE

    private static float[] gradients1D = { 1, -1 };

    private const int gradientsMask1D = 1; //see notes on hashMask.

    private static Vector2[] gradients2D = {
		new Vector2( 1f, 0f),
		new Vector2(-1f, 0f),
		new Vector2( 0f, 1f),
		new Vector2( 0f,-1f),
		new Vector2( 1f, 1f).normalized,
		new Vector2(-1f, 1f).normalized,
		new Vector2( 1f, -1f).normalized,
		new Vector2( -1f,-1f).normalized,
	};

    private const int gradientsMask2D =7; //see notes on hashMask.

    private static float sqr2 = Mathf.Sqrt(2f);

    private static Vector3[] gradients3D = {
		new Vector3( 1f, 1f, 0f),
		new Vector3(-1f, 1f, 0f),
		new Vector3( 1f,-1f, 0f),
		new Vector3(-1f,-1f, 0f),
		new Vector3( 1f, 0f, 1f),
		new Vector3(-1f, 0f, 1f),
		new Vector3( 1f, 0f,-1f),
		new Vector3(-1f, 0f,-1f),
		new Vector3( 0f, 1f, 1f),
		new Vector3( 0f,-1f, 1f),
		new Vector3( 0f, 1f,-1f),
		new Vector3( 0f,-1f,-1f),
		
		new Vector3( 1f, 1f, 0f),
		new Vector3(-1f, 1f, 0f),
		new Vector3( 0f,-1f, 1f),
		new Vector3( 0f,-1f,-1f)
	};

    private const int gradientsMask3D = 15;

    public static float PerlinNoise1D(Vector3 point)
    {
        int ix = Mathf.FloorToInt(point.x);
        float t0 = point.x - ix; //fractional part of point.x
        float t1 = t0-1;

        ix &= hashMask;
        float g0 = gradients1D[hash[ix] & gradientsMask1D];
        float g1 = gradients1D[hash[ix + 1] & gradientsMask1D];

        float v0 = g0 * t0;
        float v1 = g1 * t1;

        float t = Smooth(t0);

        //float sample = Mathf.Lerp(h0, h1, Smooth(t));
        //return sample*2-1;

        return Mathf.Lerp(v0, v1, t)*2f;
    }

    public static float PerlinNoise2D(Vector3 point)
    {

        //int ix = Mathf.FloorToInt(point.x);
        //int iy = Mathf.FloorToInt(point.y);
        int ix = point.x > 0 ? (int)point.x : (int)point.x - 1; //faster than Mathf.FloorToInt
        int iy = point.y > 0 ? (int)point.y : (int)point.y - 1;

        float tx0 = point.x - ix; //fractional parts of x and y
        float ty0 = point.y - iy;

        float tx1 = tx0 - 1;
        float ty1 = ty0 - 1;

        ix &= hashMask;
        iy &= hashMask;

        int h0 = hash[ix];
        int h1 = hash[ix + 1];

        Vector2 g00 = gradients2D[hash[h0 + iy] & gradientsMask2D];
        Vector2 g10 = gradients2D[hash[h1 + iy] & gradientsMask2D];
        Vector2 g01 = gradients2D[hash[h0 + iy + 1] & gradientsMask2D];
        Vector2 g11 = gradients2D[hash[h1 + iy + 1] & gradientsMask2D];

        float v00 = Dot(g00, tx0, ty0);
        float v10 = Dot(g10, tx1, ty0);
        float v01 = Dot(g01, tx0, ty1);
        float v11 = Dot(g11, tx1, ty1);

        float tx = Smooth(tx0);
        float ty = Smooth(ty0);
        //tx and ty are always in range 0-1 so unclamped is okay...?
        return Mathf.LerpUnclamped(Mathf.LerpUnclamped(v00, v10, tx), Mathf.LerpUnclamped(v01, v11, tx), ty) *sqr2;
    }

    public static float BrickNoise(Vector3 point) //lol this was a broken PerlinNoise3D that looked cool
    {
        int ix = Mathf.FloorToInt(point.x);
        int iy = Mathf.FloorToInt(point.y);
        int iz = Mathf.FloorToInt(point.z);

        float tx0 = Smooth(point.x - ix);
        float ty0 = Smooth(point.y - iy);
        float tz0 = Smooth(point.z - iz);
        float tx1 = tx0 - 1;
        float ty1 = ty0 - 1;
        float tz1 = tz0 - 1;

        ix &= hashMask;
        iy &= hashMask;
        iz &= hashMask;

        int h0 = hash[ix];
        int h1 = hash[ix + 1];

        int h00 = hash[h0 + iy];
        int h10 = hash[h1 + iy];
        int h01 = hash[h0 + iy + 1];
        int h11 = hash[h1 + iy + 1];

        Vector3 g000 = gradients3D[hash[h00 + iz] & gradientsMask3D];
        Vector3 g100 = gradients3D[hash[h10 + iz] & gradientsMask3D];
        Vector3 g010 = gradients3D[hash[h01 + iz] & gradientsMask3D];
        Vector3 g110 = gradients3D[hash[h11 + iz] & gradientsMask3D];
        Vector3 g001 = gradients3D[hash[h00 + iz + 1] & gradientsMask3D];
        Vector3 g101 = gradients3D[hash[h10 + iz + 1] & gradientsMask3D];
        Vector3 g011 = gradients3D[hash[h01 + iz + 1] & gradientsMask3D];
        Vector3 g111 = gradients3D[hash[h11 + iz + 1] & gradientsMask3D];


        float v000 = Dot(g000, tx0, ty0, tz0);
        float v100 = Dot(g100, tx1, ty0, tz0);
        float v010 = Dot(g010, tx0, ty1, tz0);
        float v110 = Dot(g110, tx1, ty1, tz0);
        float v001 = Dot(g001, tx0, ty0, tz1);
        float v101 = Dot(g101, tx1, ty0, tz1);
        float v011 = Dot(g011, tx0, ty1, tz1);
        float v111 = Dot(g111, tx1, ty1, tz1);


        float tx = Smooth(tx0);
        float ty = Smooth(ty0);
        float tz = Smooth(tz0);

        float sample = Mathf.Lerp(Mathf.Lerp(Mathf.Lerp(v000, v100, tx), Mathf.Lerp(v010, v110, tx), ty), Mathf.Lerp(Mathf.Lerp(v001, v101, tx), Mathf.Lerp(v011, v111, tx), ty), tz);
        return sample;

    }
    
    public static float PerlinNoise3D(Vector3 point)
    {
        int ix = Mathf.FloorToInt(point.x);
        int iy = Mathf.FloorToInt(point.y);
        int iz = Mathf.FloorToInt(point.z);

        float tx0 = point.x - ix;
        float ty0 = point.y - iy;
        float tz0 = point.z - iz;
        float tx1 = tx0 - 1;
        float ty1 = ty0 - 1;
        float tz1 = tz0 - 1;

        ix &= hashMask;
        iy &= hashMask;
        iz &= hashMask;

        int h0 = hash[ix];
        int h1 = hash[ix + 1];

        int h00 = hash[h0 + iy];
        int h10 = hash[h1 + iy];
        int h01 = hash[h0 + iy + 1];
        int h11 = hash[h1 + iy + 1];

        Vector3 g000 = gradients3D[hash[h00 + iz] & gradientsMask3D];
        Vector3 g100 = gradients3D[hash[h10 + iz] & gradientsMask3D];
        Vector3 g010 = gradients3D[hash[h01 + iz] & gradientsMask3D];
        Vector3 g110 = gradients3D[hash[h11 + iz] & gradientsMask3D];
        Vector3 g001 = gradients3D[hash[h00 + iz + 1] & gradientsMask3D];
        Vector3 g101 = gradients3D[hash[h10 + iz + 1] & gradientsMask3D];
        Vector3 g011 = gradients3D[hash[h01 + iz + 1] & gradientsMask3D];
        Vector3 g111 = gradients3D[hash[h11 + iz + 1] & gradientsMask3D];


        float v000 = Dot(g000,tx0,ty0,tz0);
        float v100 = Dot(g100,tx1,ty0,tz0);
        float v010 = Dot(g010,tx0,ty1,tz0);
        float v110 = Dot(g110,tx1,ty1,tz0);
        float v001 = Dot(g001,tx0,ty0,tz1);
        float v101 = Dot(g101,tx1,ty0,tz1);
        float v011 = Dot(g011,tx0,ty1,tz1);
        float v111 = Dot(g111,tx1,ty1,tz1);


        float tx = Smooth(tx0);
        float ty = Smooth(ty0);
        float tz = Smooth(tz0);

        float sample = Mathf.Lerp(Mathf.Lerp(Mathf.Lerp(v000, v100, tx), Mathf.Lerp(v010, v110, tx), ty), Mathf.Lerp(Mathf.Lerp(v001, v101, tx), Mathf.Lerp(v011, v111, tx), ty), tz);
        return sample;

    }
    #endregion

    #region SIMPLEX_VALUE_NOISE

    private static float squaresToTriangles = (3f - Mathf.Sqrt(3f)) / 6f;
    private static float trianglesToSquares = (Mathf.Sqrt(3f) - 1f) / 2f;

    public static float SimplexValueNoise1D(Vector3 point){
        int ix = Mathf.FloorToInt(point.x);
        float sample = SimplexValue1DPart(point, ix);
        sample += SimplexValue1DPart(point, ix + 1);
        return sample * (2f/hashMask) - 1;
    }

    private static float SimplexValue1DPart(Vector3 point, int ix)
    {
        float x = point.x - ix; //fractional part of x
        float f = (1 - x * x); //falloff function thingy
        float f2 = f * f * f;
        float h = hash[ix & hashMask];
        return f2 * h;
    }

    public static float SimplexValueNoise2D(Vector3 point)
    {
        //skew the 2D square grid space into equilateral triangle space
        float skewAmount = (point.x + point.y) * trianglesToSquares;
        float sx = point.x + skewAmount;
        float sy = point.y + skewAmount;

        int ix = Mathf.FloorToInt(sx);
        int iy = Mathf.FloorToInt(sy);

        float sample = SimplexValue2DPart(point, ix,iy);
        sample += SimplexValue2DPart(point, ix + 1,iy+1);

        //determine if the coordinate is in the upper or lower triangle
        if (sx - ix >= sy - iy) //upper tri
        {
            sample += SimplexValue2DPart(point, ix + 1, iy);
        }
        else //lower tri
        {
            sample += SimplexValue2DPart(point, ix, iy+1);
        }

        return sample * (8f*2f /hashMask) - 1;
    }

    private static float SimplexValue2DPart(Vector3 point, int ix, int iy)
    {
        float unskewAmount = (ix + iy) * squaresToTriangles;
        float x = point.x - ix + unskewAmount; //fractional part of x
        float y = point.y - iy + unskewAmount;
        float f = (0.5f - x * x - y*y); //falloff function thingy
        if (f > 0)
        {
            float f2 = f * f * f;
            //get hashvalue
            float h = hash[hash[ix & hashMask] + iy & hashMask];
            return f2 * h;
        }
        return 0;
        //float h = hash[ix & hashMask];
    }

    public static float SimplexValueNoise3D(Vector3 point)
    {
        //skew the 3D cube grid space into tetrahedron space
        float skewAmount = (point.x + point.y + point.z) * (1/3f);
        float sx = point.x + skewAmount;
        float sy = point.y + skewAmount;
        float sz = point.z + skewAmount;

        int ix = Mathf.FloorToInt(sx);
        int iy = Mathf.FloorToInt(sy);
        int iz = Mathf.FloorToInt(sz);

        float sample = SimplexValue3DPart(point, ix, iy,iz);
        sample += SimplexValue3DPart(point, ix + 1, iy + 1,iz + 1);

        //determine which tetrahedron this coord is in
        float x = sx - ix; //fractional components of x y and z
        float y = sy - iy;
        float z = sz - iz;

        if (x >= y)
        {
            if (x >= z)
            {
                sample += SimplexValue3DPart(point, ix + 1, iy, iz);
                if (y >= z)
                {
                    sample += SimplexValue3DPart(point, ix + 1, iy+1, iz);
                }
                else
                {
                    sample += SimplexValue3DPart(point, ix + 1, iy, iz+1);
                }
            }
            else
            {
                sample += SimplexValue3DPart(point, ix, iy, iz+1);
                sample += SimplexValue3DPart(point, ix + 1, iy, iz + 1);
            }
        }
        else
        {
            if (y >= z)
            {
                sample += SimplexValue3DPart(point, ix, iy+1, iz);
                if (x >= z)
                {
                    sample += SimplexValue3DPart(point, ix+1, iy + 1, iz);
                }
                else
                {
                    sample += SimplexValue3DPart(point, ix, iy + 1, iz+1);
                }
            }
            else
            {
                sample += SimplexValue3DPart(point, ix, iy, iz + 1);
                sample += SimplexValue3DPart(point, ix, iy + 1, iz + 1);
            }
        }   


        return sample *  (8f* 2f/hashMask) - 1;
    }

    private static float SimplexValue3DPart(Vector3 point, int ix, int iy, int iz)
    {
        float unskewAmount = (ix + iy + iz) * (1f / 6f);
        float x = point.x - ix + unskewAmount; //fractional part of x
        float y = point.y - iy + unskewAmount;
        float z = point.z - iz + unskewAmount;

        float f = (0.5f - x * x - y * y - z*z); //falloff function thingy
        if (f > 0)
        {
            float f2 = f * f * f;
            float h = hash[hash[hash[ix & hashMask]+iy&hashMask]+iz&hashMask];
            return f2 * h;
        }
        return 0;
    }

    #endregion

    #region SIMPLEX_NOISE
        
    public static float SimplexNoise1D(Vector3 point)
    {
        int ix = Mathf.FloorToInt(point.x);
        float sample = Simplex1DPart(point, ix);
        sample += Simplex1DPart(point, ix + 1);
        return sample * (64f / 27f);
    }

    private static float Simplex1DPart(Vector3 point, int ix)
    {
        float x = point.x - ix; //fractional part of x
        float f = (1 - x * x); //falloff function thingy
        float f2 = f * f * f;

        float g = gradients1D[hash[ix & hashMask] & gradientsMask1D];
        float v = g * x;
        return f2*v;
    }

    private static float simplexScale2D = 2916f * sqr2 / 125f;

    public static float SimplexNoise2D(Vector3 point)
    {
        //skew the 2D square grid space into equilateral triangle space
        float skewAmount = (point.x + point.y) * trianglesToSquares;
        float sx = point.x + skewAmount;
        float sy = point.y + skewAmount;

        //int ix = Mathf.FloorToInt(sx);
        //int iy = Mathf.FloorToInt(sy);
        int ix = sx > 0 ? (int)sx : (int)sx - 1; //faster than Mathf.FloorToInt
        int iy = sy > 0 ? (int)sy : (int)sy - 1;

        float sample = Simplex2DPart(point, ix, iy);
        sample += Simplex2DPart(point, ix + 1, iy + 1);

        //determine if the coordinate is in the upper or lower triangle
        if (sx - ix >= sy - iy) //upper tri
        {
            sample += Simplex2DPart(point, ix + 1, iy);
        }
        else //lower tri
        {
            sample += Simplex2DPart(point, ix, iy + 1);
        }

        return sample * simplexScale2D;
    }

    private static float Simplex2DPart(Vector3 point, int ix, int iy)
    {
        float unskewAmount = (ix + iy) * squaresToTriangles;
        float x = point.x - ix + unskewAmount; //fractional part of x
        float y = point.y - iy + unskewAmount;
        float f = (0.5f - x * x - y * y); //falloff function thingy
        if (f > 0)
        {
            float f2 = f * f * f;

            Vector2 g = gradients2D[hash[hash[ix & hashMask] + iy & hashMask] & gradientsMask2D];
            float v = Dot(g, x, y);

            return f2 * v;
        }
        return 0;
        //float h = hash[ix & hashMask];
    }

    private static float simplexScale3D = 8192f * Mathf.Sqrt(3f) / (sqr2 * 375f);

    public static float SimplexNoise3D(Vector3 point)
    {
        //skew the 3D cube grid space into tetrahedron space
        float skewAmount = (point.x + point.y + point.z) * (1 / 3f);
        float sx = point.x + skewAmount;
        float sy = point.y + skewAmount;
        float sz = point.z + skewAmount;

        int ix = Mathf.FloorToInt(sx);
        int iy = Mathf.FloorToInt(sy);
        int iz = Mathf.FloorToInt(sz);

        float sample = Simplex3DPart(point, ix, iy, iz);
        sample += Simplex3DPart(point, ix + 1, iy + 1, iz + 1);

        //determine which tetrahedron this coord is in
        float x = sx - ix; //fractional components of x y and z
        float y = sy - iy;
        float z = sz - iz;

        if (x >= y)
        {
            if (x >= z)
            {
                sample += Simplex3DPart(point, ix + 1, iy, iz);
                if (y >= z)
                {
                    sample += Simplex3DPart(point, ix + 1, iy + 1, iz);
                }
                else
                {
                    sample += Simplex3DPart(point, ix + 1, iy, iz + 1);
                }
            }
            else
            {
                sample += Simplex3DPart(point, ix, iy, iz + 1);
                sample += Simplex3DPart(point, ix + 1, iy, iz + 1);
            }
        }
        else
        {
            if (y >= z)
            {
                sample += Simplex3DPart(point, ix, iy + 1, iz);
                if (x >= z)
                {
                    sample += Simplex3DPart(point, ix + 1, iy + 1, iz);
                }
                else
                {
                    sample += Simplex3DPart(point, ix, iy + 1, iz + 1);
                }
            }
            else
            {
                sample += Simplex3DPart(point, ix, iy, iz + 1);
                sample += Simplex3DPart(point, ix, iy + 1, iz + 1);
            }
        }


        return sample * simplexScale3D;
    }

    private static float Simplex3DPart(Vector3 point, int ix, int iy, int iz)
    {
        float unskewAmount = (ix + iy + iz) * (1f / 6f);
        float x = point.x - ix + unskewAmount; //fractional part of x
        float y = point.y - iy + unskewAmount;
        float z = point.z - iz + unskewAmount;

        float f = (0.5f - x * x - y * y - z * z); //falloff function thingy
        if (f > 0)
        {
            float f2 = f * f * f;
            Vector3 g = gradients3D[hash[hash[hash[ix & hashMask] + iy & hashMask] + iz & hashMask]&gradientsMask3D];
            float v = Dot(g, x, y, z);
            return f2 * v;
        }
        return 0;
    }

    #endregion
}

public class LehmerRNG
{
    static ulong g = 48271, n = 2147483647;
    uint last;
    public uint Next()
    {
        return last = (uint)(((ulong)last * g) % n);
    }
    public float NextFraction()
    {
        last = (uint)(((ulong)last * g) % n);
        return (float)last / n;
    }
    public LehmerRNG(int seed) { this.last = (uint)((seed==0 ? g : (ulong)seed *g) % n); }
}