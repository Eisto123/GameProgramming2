using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Diagnostics;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.UIElements;

public class TerrainPerlin : MonoBehaviour
{
    public Terrain terrain;

    public int maxHeight = 600;
    public int terrainWidth = 512;  
    public int terrainHeight = 512;
    public float[,] heightMap;

    [Header("Perlin")]
    public bool ifUnityPerlin = true;
    public int octaves = 4;
    public float persistance = 0.5f;
    public float lacunarity = 2f;
    public int scale = 50;
    public int seed = 1;
    public UnityEngine.Vector2 offset = new UnityEngine.Vector2(100, 200);
    private UnityEngine.Vector2[] octaveOffsets;
    private float maxNoiseHeight = float.MinValue;
    private float minNoiseHeight = float.MaxValue;

    void Start()
    {
        
    }

    public void StartGenerate()
    {
        //OneDFFTTest();
        CreateNewTerrain(terrainWidth, terrainHeight);
    }

    private void CreateNewTerrain(int width, int height)
    {
        // Start Timer
        Stopwatch stopwatch = Stopwatch.StartNew();

        TerrainData terrainData = terrain.terrainData;
        terrainData.size = new UnityEngine.Vector3(width, maxHeight, height); 
        terrain.terrainData.heightmapResolution = terrainWidth;
        terrain.terrainData.alphamapResolution = terrainWidth;

        heightMap = new float[width, height];

        System.Random prng = new System.Random(seed);
        octaveOffsets = new UnityEngine.Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000,100000) + offset.x;
            float offsetY = prng.Next(-100000,100000) + offset.y;
            octaveOffsets[i] = new UnityEngine.Vector2(offsetX, offsetY);
        }

        for (int x = 0; x < height; x++)
        {
            for (int y = 0; y < width; y++)
            {
                heightMap[x, y] = CreatePerlin(x, y);
            }
        }

        // Normalize
        for (int x = 0; x < height; x++)
        {
            for (int y = 0; y < width; y++)
            {
                heightMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, heightMap[x, y]);
                if (x == 0 || y == 0) heightMap[x, y] = 0;
            }
        }

        terrainData.SetHeights(0, 0, heightMap);
        terrainData.size = new UnityEngine.Vector3(width, maxHeight, height); 

        // Stop the timer
        stopwatch.Stop(); 
        UnityEngine.Debug.Log("Perlin Execution Time: "+stopwatch.ElapsedMilliseconds+" ms");

    }

    public float CreatePerlin(int x, int y)
    {
        float amplitude = 1;
        float frequency = 1;
        float noiseHeight = 0;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = (float)x / scale * frequency + octaveOffsets[i].x;
            float sampleY = (float)y / scale * frequency + octaveOffsets[i].y;

            float perlinValue = 0;
            if(ifUnityPerlin) perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
            else perlinValue = PerlinNoise.GetPerlinNoise(sampleX, sampleY);
            //Debug.Log(perlinValue+" "+perlinValueCPP);
            noiseHeight += perlinValue * amplitude;

            amplitude *= persistance;
            frequency *= lacunarity;
        } 

        if (noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
        else if (noiseHeight < minNoiseHeight) minNoiseHeight = noiseHeight;

        //Debug.Log("now: "+noiseHeight +"max: "+maxNoiseHeight+" min: "+minNoiseHeight);

        return noiseHeight;
    }
}

public class PerlinNoise
{
    private static int[] permutation256 = new int[256]
    {
        151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69, 142, 8, 99, 37,
        240, 21, 10, 23, 190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177,
        33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165, 71, 134, 139, 48, 27, 166, 77, 146,
        158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54, 65, 25,
        63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 200, 196, 135, 130, 116, 188, 159, 86, 164, 100,
        109, 198, 173, 186, 3, 64, 52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206,
        59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153,
        101, 155, 167, 43, 172, 9, 129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104, 218,
        246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107,
        49, 192, 214, 31, 181, 199, 106, 157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205,
        93, 222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180
    };

    private static int[] permutation512 = new int[512];

    private static void CopyP(){
        for (int i=0; i < 256 ; i++) 
            permutation512[256+i] = permutation512[i] = permutation256[i];
    }

    private static float Fade(float x) => x * x * x * (x * (x * 6 - 15) + 10);

    private static float Interpolate(float x, float y, float t)
    {
        return Mathf.SmoothStep(x,y,t);
    }

    private static float Grad(int hash, float x, float y) 
    {
        // fake random gradient
        int h = hash & 7;

        switch(h)
        {
            case 0:
                return x+y;
            case 1:
                return -x+y;
            case 2:
                return x-y;
            case 3:
                return -x-y;
            case 4:
                return x;
            case 5:
                return y;
            case 6:
                return -x;
            case 7:
                return -y;
        }
        return 0;
    }

    static PerlinNoise()
    {
        CopyP();
    }

    public static float GetPerlinNoise(float x, float y)
    {
        int X = (int)Mathf.Floor(x) & 255;
        int Y = (int)Mathf.Floor(y) & 255;

        x -= Mathf.Floor(x);
        y -= Mathf.Floor(y);

        float u = Fade(x);
        float v = Fade(y);

        int hashLB = permutation512[X] + Y;
        int hashRB = permutation512[X + 1] + Y;

        return (Interpolate(Interpolate(
            Grad(permutation512[hashLB], x, y),
            Grad(permutation512[hashRB], x - 1, y), u
        ), Interpolate(
            Grad(permutation512[hashLB + 1], x, y - 1),
            Grad(permutation512[hashRB + 1], x - 1, y - 1), u
        ), v) + 1) * 0.5f;
    }
}
