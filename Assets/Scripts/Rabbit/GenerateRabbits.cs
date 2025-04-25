using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GenerateRabbits : MonoBehaviour
{
    [Header("Rabbits")]
    public int rabbitAmount;
    public float rabbitRadius;
    public GameObject rabbitPrefab;
    public LayerMask rabbitLayer;
    private GameObject rabbits;

    [Header("Terrain")]
    public Terrain terrain;
    public LayerMask terrainLayer;
    public float waterHeight = 10;
    [Range(0f,1f)]
    public float onRoadProbablity = 0.1f;

    private PlacePath paths;
    private Vector3[] pathPoints;
    private float pathRadius;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Generate()
    {
        paths = GameObject.FindObjectOfType<PlacePath>();
        rabbits = GameObject.Find("Rabbits");
        pathPoints = paths.GetKnotPositions();
        pathRadius = paths.GetPathRadius() - 3;
        PlaceRabbits();
    }

    public void Reset()
    {
        for (int i = rabbits.transform.childCount-1; i >= 0; i--)
        {
            DestroyImmediate(rabbits.transform.GetChild(i).gameObject);
        }
    }

    void PlaceRabbits()
    {
        Vector3 terrainSize = terrain.terrainData.size;
        // objType = 0: tree; 1: grass; 2: plants; 3: rocks
        int attempts = 0;
        int placed = 0;
        while (placed < rabbitAmount && attempts < rabbitAmount * 10)
        {
            attempts++;

            // Randomly choose a place within terrain
            int x = (int)RandomGaussian.Range(0, (int)terrainSize.x);
            int z = (int)RandomGaussian.Range(0, (int)terrainSize.z);

            // int x = Random.Range(0, (int)terrainSize.x);
            // int z = Random.Range(0, (int)terrainSize.z);

            if (x < 5 || x > terrainSize.x-5 || z < 5 || z > terrainSize.z-5 ) continue;

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
            if (ifOnRoad && Random.Range(0f,1f) > onRoadProbablity) continue;

            // Emit raycast from the top to terrain
            Vector3 rayOrigin = new Vector3(x, 200f, z) + terrain.GetPosition();
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 500f, terrainLayer))
            {
                // No objects on the terrain edge
                if (hit.point.y <= waterHeight) continue;

                // Avoid overlap
                if (Physics.CheckSphere(hit.point, rabbitRadius, rabbitLayer)) 
                    continue;
                    
                GameObject obj = Instantiate(rabbitPrefab, hit.point, Quaternion.identity, rabbits.transform);
                obj.layer = LayerMask.NameToLayer("Rabbit");
                obj.AddComponent<BoxCollider>();
            }
            placed++;
        }
    }
}

[CustomEditor(typeof(GenerateRabbits))]
public class RabbitGenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GenerateRabbits biome = (GenerateRabbits)target;

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
