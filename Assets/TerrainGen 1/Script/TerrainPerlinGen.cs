using System;
using UnityEngine;
using UnityEditor;

public class TerrainPerlinGen : MonoBehaviour
{
    public Terrain terrain;
    //public NavMeshSurface navMeshSurface;

    public int maxHeight = 600;
    public int terrainWidth = 512;  
    public int terrainHeight = 512;
    public int heightmapResolution = 512;
    public float[,] heightMap;

    [Header("Perlin")]
    public bool ifRandomSeed = false;
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
        //StartGenerate();
    }

    public void StartGenerate()
    {
        RandomPara();
        CreateNewTerrain(terrainWidth, terrainHeight);
        //navMeshSurface.BuildNavMesh();
    }

    private void RandomPara()
    {
        if (ifRandomSeed)
            seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
    }

    private void CreateNewTerrain(int width, int height)
    {
        TerrainData terrainData = terrain.terrainData;
        terrainData.size = new Vector3(width, maxHeight, height); 
        //terrain.terrainData.heightmapResolution = terrainWidth;
        //terrain.terrainData.alphamapResolution = terrainWidth;
        terrain.terrainData.heightmapResolution = heightmapResolution;
        //terrain.terrainData.alphamapResolution = heightmapResolution/2;

        heightMap = new float[heightmapResolution, heightmapResolution];

        // Create Perlin Noise Map
        PerlinNoiseMap heightNoiseMap = new PerlinNoiseMap(octaves, persistance, lacunarity, scale, seed, offset);

        for (int x = 0; x < heightmapResolution; x++)
        {
            for (int y = 0; y < heightmapResolution; y++)
            {
                //heightMap[x, y] = CreatePerlin(x, y);
                heightMap[x, y] = heightNoiseMap.CreatePerlin(x,y);
            }
        }

        // Normalize
        for (int x = 0; x < heightmapResolution; x++)
        {
            for (int y = 0; y < heightmapResolution; y++)
            {
                //heightMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, heightMap[x, y]);
                heightMap[x, y] = heightNoiseMap.Normalize(heightMap[x, y]);
                if (x == 0 || y == 0) heightMap[x, y] = 0;
            }
        }

        terrainData.SetHeights(0, 0, heightMap);
        terrainData.size = new Vector3(width, maxHeight, height); 

    }
}

// [CustomEditor(typeof(TerrainPerlinGen))]
// public class TerrainPerlinGenEditor : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         DrawDefaultInspector();

//         TerrainPerlinGen terrainPerlin = (TerrainPerlinGen)target;

//         if (GUILayout.Button("Start Generate"))
//         {
//             terrainPerlin.StartGenerate();
//         }
//     }
// }