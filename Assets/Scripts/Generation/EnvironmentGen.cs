using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentGen : MonoBehaviour
{
    [Header("Props")]
    public GameObject[] TreePrefabs;
    public Transform Centre;
    [Range(0,100)]
    public float Radius;
    [Range(1,1000)]
    public int Trees;

    [Header("Tree Collisions")]
    [Range(0f,10f)]
    public float TreeRadius;
    public LayerMask TreeLayerMask;

    [Header("Terrain")]
    public LayerMask TerrainMask;


    // Start is called before the first frame update
    void Start()
    {
        InstantiateTrees();
    }

    private void InstantiateTrees()
    {
        for (int i = 0; i < Trees; i ++)
        {
            InstantiateTree();
        }
    }

    private void InstantiateTree ()
    {
        Vector3 position;
        int i = 0;
        const int MaxAttempts = 100;
        do
        {
            // Too many attempts!
            // Don't create a tree
            if (i >= MaxAttempts)
                return;

            // insideUnitCircle returns (x,y), but we need (x,z)
            position = GetRandomPosition();
            /*
            Vector2 unitCircle = Random.insideUnitCircle * Radius;
            position = Centre.position +
                new Vector3(unitCircle.x, 0f, unitCircle.y);
            */


            // Position is on the sky
            // We need to raycast down to find the terrain height
            position = TerrainHeightAt(position);
            i++;
        } while (IsTreeAt(position));

        // Random quaternion around y axis
        Quaternion rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0f);
        
        // Prefab
        GameObject prefab = TreePrefabs[Random.Range(0,TreePrefabs.Length)];

        Instantiate(prefab, position, rotation, transform);
    }

    private Vector3 TerrainHeightAt(Vector3 position)
    {
        RaycastHit hitInfo;
        bool hit = Physics.Raycast(position, Vector3.down, out hitInfo, 10f, TerrainMask);
        // Tree is not on top of the ground! (error?)
        if (!hit)
            return position;

        position.y = hitInfo.point.y;
        return position;
    }

    private Vector3 GetRandomPosition()
    {
        return new Vector3(0,0,0);
    }

    private bool IsTreeAt (Vector3 position)
    {
        //Debug.DrawLine(position, position + Vector3.up * 10,
        //    Physics.CheckSphere(position, TreeRadius, TreeLayerMask) ?
        //    Color.red : Color.green, 10f
        //    );
        return Physics.CheckSphere(position, TreeRadius, TreeLayerMask);
    }

}
