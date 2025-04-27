using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

public class PlacePath : MonoBehaviour
{
    public bool isVisualizing = false;
    public bool isTilting = false;
    [SerializeField] private SplineContainer splineContainer;

    [SerializeField] private int splineIndex;

    [SerializeField] [Range(0f,1f)] private float splineTime;

    private float3 position;
    private float3 forward;
    private float3 upVector;

    // private List<float3> rightPoints;
    // private List<float3> leftPoints;
    // [SerializeField]private float roadWidth;
    public float resolution;
    [SerializeField]private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    public SplineContainer Fence;

    public int randomRange;
    public GameObject finishLinePrefab;
    public int carAmount;

    public UnityEvent<Vector3[]> OnGenerationComplete;

    private float[,] origHeightMap;
    private BezierKnot[] knotArray;


    // Start is called before the first frame update
    void Start()
    {
        // ResetSplatMap();
        // meshCollider = GetComponent<MeshCollider>();
        // RandomizeSpline();
        // MapKnot();
    }

    public void Generate()
    {
        ResetSplatMap();
        meshCollider = GetComponent<MeshCollider>();
        RandomizeSpline();
        MapKnot();
    }
    void Update()
    {
    }

    // void OnDestroy()
    // {
    //     Debug.Log("reset");
    //     ResetSplatMap();
    //     terrain.terrainData.SetHeights(0, 0, origHeightMap);
    // }

    // void OnDrawGizmos()
    // {
    //     if (!isVisualizing) return;
    //     Handles.matrix = transform.localToWorldMatrix;

    //     if (rightPoints == null || leftPoints == null) return;

    //     for (int i = 0; i < rightPoints.Count - 1; i++)
    //     {
    //         Handles.color = Color.red;
    //         Handles.SphereHandleCap(0, rightPoints[i], Quaternion.identity, 0.3f, EventType.Repaint);
    //         Handles.color = Color.blue;
    //         Handles.SphereHandleCap(0, leftPoints[i], Quaternion.identity, 0.3f, EventType.Repaint);
    //     }

    //     if (knotPositions == null) return;

    //     for (int i = 0; i < knotPositions.Length; i++)
    //     {
    //         Gizmos.DrawSphere(knotPositions[i], 0.5f);
    //     }
    // }

    private void RandomizeSpline(){
        var knotArray = splineContainer.Spline.ToArray();
        for(int i = 0; i<knotArray.Count(); i++){
            var knot = knotArray[i];
            var currPos = knot.Position;
            knot.Position = currPos + new float3(UnityEngine.Random.Range(-randomRange,randomRange),0,UnityEngine.Random.Range(-randomRange,randomRange));
            Vector3 worldPos = splineContainer.transform.TransformPoint(knot.Position);
            while (worldPos.x < 25 || worldPos.x > terrain.terrainData.size.x-25 || worldPos.z < 25 || worldPos.z > terrain.terrainData.size.z-25 )
            {
                knot.Position = currPos + new float3(UnityEngine.Random.Range(-randomRange,randomRange),0,UnityEngine.Random.Range(-randomRange,randomRange));
                worldPos = splineContainer.transform.TransformPoint(knot.Position);
            }
            
            splineContainer.Splines[0].SetKnot(i,knot);
        }


    }

    Vector3[] knotPositions;

    public Vector3[] GetKnotPositions()
    {
        return knotPositions;
    }

    public float GetPathRadius()
    {
        return Mathf.RoundToInt(brushSize / terrain.terrainData.size.x * terrain.terrainData.alphamapWidth);
    }


    private void MapKnot(){
        SetKnotAmount();
        //var knotArray = splineContainer.Splines[1].ToArray();
        knotArray = splineContainer.Splines[1].ToArray();
        knotPositions = new Vector3[knotArray.Length];
        for(int i = 0; i<knotArray.Length; i++){
            Vector3 position = FindTerrainPosition(knotArray[i].Position,out RaycastHit hit);
            if(position != Vector3.zero){
                //Vector3 smoothPosition = i==0? position : Vector3.Lerp(position,knotArray[i-1].Position,0.5f);
                //knotArray[i].Position = smoothPosition;
                //knotPositions[i] = smoothPosition;
                knotPositions[i] = position;
                knotArray[i].Position = position;
                splineContainer.Splines[1].SetKnot(i,knotArray[i]);
            }
            else{
                Debug.LogError("No terrain found at knot position");
            }
        }

        // Vector3[] knotPosition = new Vector3[knotArray.Length];
        // for(int i = 0; i<knotArray.Length; i++){
        //     Vector3 position = knotArray[i].Position;
        //     knotPosition[i] = position;
        // }

        

        OnGenerationComplete.Invoke(knotPositions);


        for(int i = 0; i<knotArray.Length; i++){
            Vector3 forward;
            if(i == knotArray.Length-1){
                forward = knotArray[0].Position - knotArray[i].Position;
            }else{
                forward = knotArray[i+1].Position - knotArray[i].Position;
            }
            forward.Normalize();
            Vector3 up = Vector3.up;
            Vector3 left = Vector3.Cross(forward, up).normalized;
            Fence.Spline.Add(knotArray[i].Position+(float3)left*GetPathRadius()+new float3(0,100f,0), TangentMode.AutoSmooth);
        }

        var fenceArray = Fence.Spline.ToArray();
        for(int i = 0; i<fenceArray.Length; i++){
            Vector3 position = FindTerrainPosition(fenceArray[i].Position,out RaycastHit hit);
            if(position != Vector3.zero){
                fenceArray[i].Position = position;
                fenceArray[i].Rotation = quaternion.Euler(hit.normal);
                Fence.Spline.SetKnot(i,fenceArray[i]);
            }
            else{
                Debug.LogError("No terrain found at knot position");
            }
        }
        Fence.GetComponent<SplineInstantiate>().enabled = true;
        GenerateFinishLine();

        // map to terrain heightmap
        Debug.Log(knotPositions.Length);
        PaintPathOnTerrain(knotPositions);

        //mapping complete
        
        //var car = Instantiate(carPrefab);
        //PlaceCarOnPosition(car, knotArray[0].Position + new float3(0,0.2f,0), knotArray[0].Rotation);

        
        //OnGenerationComplete.Invoke();

    }

    private void GenerateFinishLine(){
        GameObject finishiline = Instantiate(finishLinePrefab, knotArray[0].Position, knotArray[0].Rotation);
        //finishiline.transform.localScale = new Vector3(GetPathRadius()*2-2,0.1f,1f);
    }

    [Header("Terrain Path Setting")]
    public Terrain terrain;
    public int pathTextureIndex = 1; // the layer of the path
    public float brushSize = 2f;
    public AnimationCurve blur;

    private void ResetSplatMap()
    {
        TerrainData terrainData = terrain.terrainData;
        int mapWidth = terrainData.alphamapWidth;
        int mapHeight = terrainData.alphamapHeight;
        int numTextures = terrainData.alphamapLayers;
        float[,,] splatmapData = terrainData.GetAlphamaps(0, 0, mapWidth, mapHeight);

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                for (int i = 0; i < numTextures; i++)
                    splatmapData[x, y, i] = (i == 0) ? 1 : 0;
            }
        }

        terrainData.SetAlphamaps(0, 0, splatmapData);
    }

    void PaintPathOnTerrain(Vector3[] pathPoints)
    {
        TerrainData terrainData = terrain.terrainData;
        int mapWidth = terrainData.alphamapWidth;
        int mapHeight = terrainData.alphamapHeight;
        int heightresolution = terrainData.heightmapResolution;
        int numTextures = terrainData.alphamapLayers;
        Debug.Log("heightresolution "+heightresolution);

        Debug.Log(mapWidth+" "+mapHeight);
        //Debug.Log("numTextures "+numTextures);

        float[,,] splatmapData = terrainData.GetAlphamaps(0, 0, mapWidth, mapHeight);
        float[,] heightMap = terrainData.GetHeights(0,0,heightresolution,heightresolution);
        origHeightMap = terrain.terrainData.GetHeights(0,0,heightresolution,heightresolution);

        int radius = Mathf.RoundToInt(brushSize / terrainData.size.x * mapWidth);
        int heightRadius = Mathf.RoundToInt(brushSize / terrainData.size.x * heightresolution);

        foreach (Vector3 worldPos in pathPoints)
        {
            Vector3 terrainPos = FindTerrainHeightmapPoint(worldPos, terrain, mapWidth, mapHeight);
            //Vector3 heightPos = FindTerrainHeightmapPoint(worldPos, terrain, heightresolution, heightresolution);
            //Debug.Log("height position: "+heightPos);

            //Debug.Log(worldPos+" "+terrainPos);
            //Debug.Log("r "+radius);
            int centerX = Mathf.RoundToInt(terrainPos.x);
            int centerY = Mathf.RoundToInt(terrainPos.z);
            float height = origHeightMap[centerX, centerY];
            //Debug.Log("height: "+height);

            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    int finalX = centerX+x;
                    int finalY = centerY+y;

                    if (finalX < 0 || finalY < 0 || finalX >= mapWidth || finalY >= mapHeight)
                        continue;

                    //heightMap[finalY, finalX] = height;

                    float dist = Mathf.Sqrt(x * x + y * y) / radius;
                    dist = Mathf.Clamp(dist,0f,1f);
                    //float strength = Mathf.SmoothStep(1f, 0f, dist);
                    float strength = Mathf.Lerp(0, 1, blur.Evaluate(dist));
                    //Debug.Log("s "+strength);

                    /*float[] weights = new float[numTextures];
                    for (int i = 0; i < numTextures; i++)
                        weights[i] = (i == pathTextureIndex) ? strength : 1-strength;

                    for (int i = 0; i < numTextures; i++)
                        splatmapData[finalY, finalX, i] = weights[i];*/

                    float[] currentWeights = new float[numTextures];
                    for (int i = 0; i < numTextures; i++)
                        currentWeights[i] = splatmapData[finalY, finalX, i];
                    
                    float[] newWeights = new float[numTextures];
                    for (int i = 0; i < numTextures; i++)
                    {
                        float target = (i == pathTextureIndex) ? 1f : 0f;
                        newWeights[i] = Mathf.Lerp(currentWeights[i], target, strength);
                    }

                    // normalize the weight
                    float sum = newWeights.Sum();
                    for (int i = 0; i < numTextures; i++)
                        splatmapData[finalY, finalX, i] = newWeights[i] / sum;
                }
            }

            // for (int y = -heightRadius; y <= heightRadius; y++)
            //     for (int x = -heightRadius; x <= heightRadius; x++)
            //     {
            //         int finalX = centerX+x;
            //         int finalY = centerY+y;

            //         heightMap[finalY, finalX] = height;
            //     }
        }
        

        terrainData.SetHeights(0, 0, heightMap);
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }

    private Vector3 FindTerrainPosition(Vector3 position,out RaycastHit hit){
        
        if(Physics.Raycast(position, Vector3.down, out hit, 1000f)){
            return hit.point;
        }
        Debug.LogError("No terrain hit");
        return Vector3.zero;
    }

    private void SetKnotAmount(){
        float step = 1f/resolution;
        for(int i = 0; i<resolution; i++){
            float t = i * step;
            splineContainer.Evaluate(splineIndex, t, out position, out forward, out upVector);
            splineContainer.Splines[1].Add(position,TangentMode.AutoSmooth);
        }
    }

    private Vector3 FindTerrainHeightmapPoint(Vector3 worldPos, Terrain terrain, int mapWidth, int mapHeight)
    {
        Vector3 terrainPos = worldPos - terrain.transform.position;
        float normX = terrainPos.x / terrain.terrainData.size.x;
        float normZ = terrainPos.z / terrain.terrainData.size.z;
        //Debug.Log(normX+" "+normZ);
        return new Vector3(normX * mapWidth, 0, normZ * mapHeight);
    }

}
