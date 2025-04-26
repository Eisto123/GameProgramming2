using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using Unity.AI.Navigation;


public class BiomeGen : MonoBehaviour
{
    public NavMeshSurface navMeshSurface;
    [Header("terrain")]
    public Terrain terrain;
    public LayerMask terrainLayer;
    public float waterHeight = 10;
    private TerrainData terrainData;
    private Vector3 terrainSize;

    private PlacePath paths;
    private Vector3[] pathPoints;
    private float pathRadius;

    [Header("Biome Setting")]
    public float baseTemperature = 0.2f;
    public float baseHumidity = 0.2f;
    public float tempHeightInfluence = 0.2f; // how will height influence the temperature
    public float humidityHeightInfluence = 0.1f;
    public float tempPerlin = 0.4f;
    public float humidityPerlin = 0.5f;
    public List<BiomeConfig> biomeConfigs;
    public RenderTexture temperatureRenderTexture;
    public RenderTexture humidityRenderTexture;
    public RenderTexture biomeRenderTexture;
    private BiomeType[,] biomeMap;
    public LayerMask objLayer;

    [Header("Trees & Plants")]
    public int treeCount = 1000;
    public int grassCount = 5000;
    public int plantCount = 5000;
    public float treeRadius;
    public float grassRadius;
    public float plantRadius;
    public float rockRadius;
    private GameObject trees;
    private GameObject grass;
    private GameObject plants;

    [Header("Underwater")]
    public int rockCount = 200;
    public int waterplantCount = 200;
    public LayerMask rockLayer;
    public List<GameObject> waterRocks;
    public List<GameObject> waterPlants;
    [Range(0f,1f)]
    public float onRoadProbablity;
    private GameObject underwater;

    [Header("Rabbit Food")]
    public int norFoodCount = 300;
    public int colFoodCount = 50;
    public float foodRadius = 1;
    public LayerMask foodLayer;
    public GameObject normalFood;
    public GameObject colorFood;
    private GameObject food;

    [Header("Perlin Noise - Temperature")]
    public int tempOctave = 4;
    public float tempPersistance = 0.5f;
    public float tempLacunarity = 2f;
    public int tempScale = 50;
    public int tempSeed = 1;
    public Vector2 tempOffset = new Vector2(100, 200);

    [Header("Perlin Noise - Humidity")]
    public int humidityOctave = 4;
    public float humidityPersistance = 0.5f;
    public float humidityLacunarity = 2f;
    public int humidityScale = 50;
    public int humiditySeed = 10;
    public Vector2 humidityOffset = new Vector2(100, 200);


    // Start is called before the first frame update
    void Start()
    {
        // terrainData = terrain.terrainData;
        // terrainSize = terrainData.size;
        // trees = GameObject.Find("Trees");
        // grass = GameObject.Find("Grass");
        // plants = GameObject.Find("Plants");

        // Generate();
    }

    public void Reset()
    {
        for (int i = trees.transform.childCount-1; i >= 0; i--)
        {
            DestroyImmediate(trees.transform.GetChild(i).gameObject);
        }

        for (int i = grass.transform.childCount-1; i >= 0; i--)
        {
            DestroyImmediate(grass.transform.GetChild(i).gameObject);
        }

        for (int i = plants.transform.childCount-1; i >= 0; i--)
        {
            DestroyImmediate(plants.transform.GetChild(i).gameObject);
        }
    }

    public void Generate()
    {
        paths = GameObject.FindObjectOfType<PlacePath>();
        terrainData = terrain.terrainData;
        terrainSize = terrainData.size;
        trees = GameObject.Find("Trees");
        grass = GameObject.Find("Grass");
        plants = GameObject.Find("Plants");
        //rocks = GameObject.Find("Rocks");
        underwater = GameObject.Find("Underwater");
        food = GameObject.Find("Food");
        pathPoints = paths.GetKnotPositions();
        pathRadius = paths.GetPathRadius() - 3;

        tempSeed = Random.Range(int.MinValue, int.MaxValue);
        humiditySeed = Random.Range(int.MinValue, int.MaxValue);

        GenerateBiomeMap();
        RenderBiomeMap(biomeRenderTexture);
        PlaceObjs(treeCount, 0, treeRadius);
        PlaceObjs(grassCount, 1, grassRadius);
        PlaceObjs(plantCount, 2, plantRadius);
        PlaceWaterBiome(rockCount,0, rockRadius);
        PlaceWaterBiome(waterplantCount,1, plantRadius);

        
        PlaceObjs(norFoodCount, 3, foodRadius);
        PlaceObjs(colFoodCount, 4, foodRadius);

        navMeshSurface.BuildNavMesh();
    }
    
    private void GenerateBiomeMap()
    {
        int biomeMapX = (int)terrainSize.x;
        int biomeMapZ = (int)terrainSize.z;
        biomeMap = new BiomeType[biomeMapX, biomeMapZ];

        Texture2D tempTex = new Texture2D(biomeMapX, biomeMapZ);
        Texture2D humidTex = new Texture2D(biomeMapX, biomeMapZ);

        // Create Perlin noise map for temperature and humidity
        PerlinNoiseMap tempMap = new PerlinNoiseMap(tempOctave,tempPersistance, tempLacunarity, tempScale, tempSeed,tempOffset);
        PerlinNoiseMap humidityMap = new PerlinNoiseMap(humidityOctave, humidityPersistance, humidityLacunarity, humidityScale, humiditySeed, humidityOffset);

        for (int x = 0; x < biomeMapX; x++)
        {
            for (int z = 0; z < biomeMapZ; z++)
            {
                // get the normalized position inside the terrain
                float normX = x / terrainSize.x;
                float normZ = z / terrainSize.z;
                // get the height interpolated from 4 nearest heightmap point values
                float height = terrainData.GetInterpolatedHeight(normX, normZ) / terrainSize.y;

                // get temp noise and humidity noise map
                // calculate temp and humidity according to noise and height
                // float temperature = baseTemperature - height * tempHeightInfluence + Mathf.PerlinNoise(x * biomeMapScale, z * biomeMapScale) * tempPerlin;
                // float humidity = baseHumidity - height * humidityHeightInfluence + Mathf.PerlinNoise((x + 999) * biomeMapScale, (z + 999) * biomeMapScale) * humidityPerlin;
                float temperature = baseTemperature - height * tempHeightInfluence + tempMap.CreatePerlin(x, z) * tempPerlin;
                float humidity = baseHumidity - height * humidityHeightInfluence + humidityMap.CreatePerlin(x,z) * humidityPerlin;

                temperature = Mathf.Clamp01(temperature);
                humidity = Mathf.Clamp01(humidity);

                biomeMap[x, z] = GetBiome(temperature, humidity);

                tempTex.SetPixel(x, z, new Color(temperature, temperature, temperature));
                humidTex.SetPixel(x, z, new Color(humidity, humidity, humidity));
            }
        }

        // Visualization: Update temperature and humidity render texture
        tempTex.Apply();
        humidTex.Apply();

        if (temperatureRenderTexture != null)
        {
            Graphics.Blit(tempTex, temperatureRenderTexture);
        }

        if (humidityRenderTexture != null)
        {
            Graphics.Blit(humidTex, humidityRenderTexture);
        }
    }

    private BiomeType GetBiome(float temperature, float humidity)
    {
        if (temperature < 0.45f && humidity < 0.5f) return BiomeType.SnowyTundra;
        if (temperature >= 0.45f && humidity < 0.5f) return BiomeType.DryPlateau;
        if (temperature < 0.45f && humidity > 0.6f) return BiomeType.Wetlands;
        if (temperature > 0.55f && humidity > 0.5f) return BiomeType.Tropical;

        return BiomeType.TemperateForest;
    }

    private void RenderBiomeMap(RenderTexture biomeRenderTexture)
    {
        if (biomeMap == null || biomeRenderTexture == null) return;

        int biomeMapX = biomeMap.GetLength(0);
        int biomeMapZ = biomeMap.GetLength(1);

        Texture2D biomeTex = new Texture2D(biomeMapX, biomeMapZ);

        Dictionary<BiomeType, Color> biomeColors = new Dictionary<BiomeType, Color>
        {
            { BiomeType.TemperateForest, Color.green}, 
            { BiomeType.Tropical,        Color.red},
            { BiomeType.DryPlateau,      Color.yellow},
            { BiomeType.Wetlands,        Color.blue},
            { BiomeType.SnowyTundra,     Color.white}
        };

        for (int x = 0; x < biomeMapX; x++)
        {
            for (int z = 0; z < biomeMapZ; z++)
            {
                BiomeType type = biomeMap[x, z];
                Color color = biomeColors.ContainsKey(type) ? biomeColors[type] : Color.magenta;
                biomeTex.SetPixel(x, z, color);
            }
        }

        biomeTex.Apply();
        Graphics.Blit(biomeTex, biomeRenderTexture);
    }

    void PlaceObjs(int amount, int objType, float objRadius)
    {
        // objType = 0: tree; 1: grass; 2: plants; 3: rocks
        int attempts = 0;
        int placed = 0;
        while (placed < amount && attempts < amount * 10)
        {
            attempts++;

            // Randomly choose a place within terrain
            int x = Random.Range(0, (int)terrainSize.x);
            int z = Random.Range(0, (int)terrainSize.z);

            // Get the biome info at the point
            BiomeType biome = biomeMap[x, z];
            BiomeConfig config = biomeConfigs.Find(b => b.biomeType == biome);

            float density = 1;
            int prefabCount = 1;
            switch (objType)
            {
                case 0: // tress
                    density = config.treeDensity;
                    prefabCount = config.treePrefabs.Count;
                    break;
                case 1: // grass
                    density = config.grassDensity;
                    prefabCount = config.grassPrefab!=null? 1:0;
                    break;
                case 2: // plants
                    density = config.plantDensity;
                    prefabCount = config.plantPrefabs.Count;
                    break;
            }

            // Only put trees when this biome allows trees
            // Different biome type has different densities of trees
            if (config == null || prefabCount == 0) continue;
            if (Random.Range(0f,1f) > density) continue;
            if (x < 5 || x > terrainSize.x-5 || z < 5 || z > terrainSize.z-5 ) continue;

            // No objects on the road
            bool ifOnRoad = false;
            foreach (Vector3 point in pathPoints)
            {
                if (x > point.x - pathRadius - 2 && x < point.x + pathRadius + 2
                    && z > point.z - pathRadius - 2 && z < point.z + pathRadius + 2)
                {
                    ifOnRoad = true;
                    break;
                }
            }
            if (ifOnRoad) continue;

            // Emit raycast from the top to terrain
            Vector3 rayOrigin = new Vector3(x, 200f, z) + terrain.GetPosition();
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 500f, terrainLayer))
            {
                // No objects on the terrain edge
                if (objType!=3 && hit.point.y <= waterHeight) continue;

                // No objects in lake
                if (hit.point.y < waterHeight) continue;

                // Avoid overlap
                if (Physics.CheckSphere(hit.point, objRadius, objLayer)) 
                    continue;

                switch (objType)
                {
                    case 0: // tress
                        GameObject prefab = config.treePrefabs[Random.Range(0, config.treePrefabs.Count)];
                        GameObject obj = Instantiate(prefab, hit.point, Quaternion.identity, trees.transform);
                        obj.layer = LayerMask.NameToLayer("Tree");
                        CapsuleCollider col = obj.AddComponent<CapsuleCollider>();
                        //col.radius = 0.7f;
                        NavMeshObstacle obs = obj.AddComponent<NavMeshObstacle>();
                        obs.shape = NavMeshObstacleShape.Capsule;

                        // change color
                        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                        foreach (Material mat in renderer.materials)
                        {
                            if (mat.name.Contains("leafsGreen"))
                            {
                                mat.color = config.treeColors[Random.Range(0, config.treeColors.Count)];
                            }
                        }
                        break;
                    case 1: // grass
                        prefab = config.grassPrefab;
                        obj = Instantiate(prefab, hit.point, Quaternion.identity, grass.transform);
                        obj.layer = LayerMask.NameToLayer("Plant");
                        obj.AddComponent<BoxCollider>();
                        obj.transform.up = hit.normal;
                        break;
                    case 2: // plants
                        prefab = config.plantPrefabs[Random.Range(0, config.plantPrefabs.Count)];
                        obj = Instantiate(prefab, hit.point, Quaternion.identity, plants.transform);
                        obj.layer = LayerMask.NameToLayer("Plant");
                        obj.AddComponent<BoxCollider>();
                        obs = obj.AddComponent<NavMeshObstacle>();
                        obs.shape = NavMeshObstacleShape.Capsule;
                        break;
                    case 3: // food
                        obj = Instantiate(normalFood, hit.point, Quaternion.identity, food.transform);
                        obj.layer = LayerMask.NameToLayer("Food");
                        obj.AddComponent<CapsuleCollider>();
                        break;
                    case 4: // coolor food
                        obj = Instantiate(colorFood, hit.point, Quaternion.identity, food.transform);
                        obj.layer = LayerMask.NameToLayer("ColorFood");
                        obj.AddComponent<CapsuleCollider>();
                        break;
                }
                placed++;
            }
        }
    }

    private void PlaceWaterBiome(int amount, int objType, float objRadius)
    {
        // objType = 0: rock; 1: plants
        int attempts = 0;
        int placed = 0;
        while (placed < amount && attempts < amount * 10)
        {
            attempts++;

            // Randomly choose a place within terrain
            int x = Random.Range(0, (int)terrainSize.x);
            int z = Random.Range(0, (int)terrainSize.z);

            // No objects on the road
            bool ifOnRoad = false;
            foreach (Vector3 point in pathPoints)
            {
                if (x > point.x - pathRadius && x < point.x + pathRadius 
                    && z > point.z - pathRadius && z < point.z + pathRadius)
                {
                    ifOnRoad = true;
                    break;
                }
            }
            // p[whether the rock will be put] = P_onroad
            if (ifOnRoad && Random.Range(0f,1f) > onRoadProbablity) continue;

            Vector3 rayOrigin = new Vector3(x, 200f, z) + terrain.GetPosition();
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 500f, terrainLayer))
            {
                // Only put near water
                if (hit.point.y > waterHeight) continue;

                // Avoid overlap
                if (Physics.CheckSphere(hit.point, objRadius, objLayer)) 
                    continue;
                
                GameObject prefab = objType==0? waterRocks[Random.Range(0, waterRocks.Count)]:waterPlants[Random.Range(0, waterPlants.Count)];
                GameObject obj = Instantiate(prefab, hit.point, Quaternion.identity, underwater.transform);
                obj.layer = objType==0? LayerMask.NameToLayer("Rock") : LayerMask.NameToLayer("Plant");
                obj.AddComponent<BoxCollider>();
                obj.transform.up = hit.normal;
                
                placed++;
            }
        }

    }
}

[CustomEditor(typeof(BiomeGen))]
public class BiomeGenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BiomeGen biome = (BiomeGen)target;

        if (GUILayout.Button("Start Generate"))
        {
            biome.Generate();
        }

        if (GUILayout.Button("Reset"))
        {
            biome.Reset();
        }
    }
}
