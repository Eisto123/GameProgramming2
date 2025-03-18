using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Generator : MonoBehaviour
{
    [SerializeField]private int gridSizeX = 15;
    [SerializeField]private int gridSizeY = 15;
    [SerializeField] private float noiseScale = 0.1f;  // Controls noise frequency
    [SerializeField] private float noiseStrength = 1.0f; // Controls how much the edge wobbles
    private Vector2 ellipseCenter;

    [SerializeField]private float horizontalRadius = 5;
    [SerializeField]private float verticalRadius = 4;
    [SerializeField] private float trackWidth = 1.0f;
    public bool visualizing;
    public float isolevel=0f;
    private float[,] scalarField = new float[0, 0];

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDrawGizmos()
    {
        if(!visualizing){
            return;
        }
        ellipseCenter = new Vector2(gridSizeX/2,gridSizeY/2);
        scalarField = new float[gridSizeX, gridSizeY];

    // Example: Fill the field with values based on distance to a sphere
        for (int x = 0; x < gridSizeX; x++) {
            for (int y = 0; y < gridSizeY; y++) {
                float noiseValue = Mathf.PerlinNoise(x, y)*noiseScale;
                Vector2 point = new Vector2(x, y);
                Vector2 offset = point - ellipseCenter;

                // Ellipse distance formula
                float normalizedDistance = Mathf.Sqrt(
                    (offset.x * offset.x) / (horizontalRadius * horizontalRadius) +
                    (offset.y * offset.y) / (verticalRadius * verticalRadius)
                );

                // Inside ellipse → value closer to 1, outside → less than 1
                scalarField[x, y] = 1.0f - normalizedDistance;

                // Generate Perlin noise for an irregular boundary
                float noise = Mathf.PerlinNoise(x * noiseScale, y * noiseScale) * noiseStrength;

                // Define track: inner and outer boundaries with noise
                float lowerBound = 1.0f - trackWidth / 2 + noise * 0.2f;
                float upperBound = 1.0f + trackWidth / 2 + noise * 0.2f;

                // Scalar field stores -1 for inside track, 1 for outside
                if (normalizedDistance >= lowerBound && normalizedDistance <= upperBound)
                    scalarField[x, y] = -1; // Inside track
                else
                    scalarField[x, y] = 1;  // Outside track
            
            }
        }
        for (int x = 0; x < gridSizeX; x++) {
            for (int y = 0; y < gridSizeY; y++) {
                if(scalarField[x,y]>isolevel){
                    Gizmos.DrawSphere(new Vector3(x,y),0.2f);
                }
            }
        }
    }
}
