using AlanZucconi.Voronoi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoronatorSharp;
using System.Linq;

using AlanZucconi.AI.PF;
using AlanZucconi;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class VoronoiVillage : MonoBehaviour
{
    public Vector2 Size;

    [Header("Voronoi")]
    [Range(3,100)]
    public int CellsCount;
    public Voronoi Voronoi;


    [Header("Street")]
    public LineRenderer Line;
    public float StreetLength;

    [Header("Buildings")]
    public CellBuilding BuildingPrefab;

    [ContextMenu("TestRegenerate")]
    public void TestRegenerate(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Start is called before the first frame update
    void Start()
    {
        GenerateVillage();
    }
    


    private void GenerateVillage()
    {
        // Generate voronoi cells
        GenerateVoronoiCells();
        //DrawVoronoiCells();

        GenerateStreet();

        GenerateBuildings();
    }

    private void GenerateVoronoiCells()
    {
        // Voronator work on V2(XY),
        // but Transform works on V3(XYZ)
        Vector2 centre = transform.position.XZ(); // From V3(XYZ) to V2(XZ)

        // Generates random points
        List<Vector2> points = new List<Vector2>(CellsCount);
        for (int i = 0; i < CellsCount; i ++)
            points.Add
            (   
                centre + new Vector2
                (
                    Random.Range(-Size.x, +Size.y) / 2f,
                    Random.Range(-Size.x, +Size.y) / 2f
                )
            );

        // Generates the voronoi cells
        Voronoi = new Voronoi(points, -Size / 2f, Size /2f);
    }


    private void GenerateStreet()
    {
        // Finds a point at the centre
        Vector2 centre = transform.position;
        Vector2 centreVertex = Voronoi.ClosestVertex(centre);

        // Finds a point at the bottom left
        //Vector2 corner0 = (Vector2) transform.position - Size / 2f;
        //Vector2 corner0Vertex = Voronoi.ClosestVertex(corner0);

        // Finds a point at the top right
        //Vector2 corner1 = (Vector2)transform.position + Size / 2f;
        //Vector2 corner1Vertex = Voronoi.ClosestVertex(corner1);

        // Vector2 StartPointVertex = FindRandomStartPoint();
        // while(StartPointVertex == (Vector2) transform.position - Size / 2f){
        //     StartPointVertex = FindRandomStartPoint();
        // }
        Vector2 FirstPoint= GetVertexAtDegreeAndDistance(centre,90, StreetLength);
        Vector2 FirstPointVertex = Voronoi.ClosestVertex(FirstPoint);
        Debug.Log(FirstPoint);
        
        Vector2 SecondPoint= GetVertexAtDegreeAndDistance(centre,-30, StreetLength);
        Vector2 SecondPointVertex = Voronoi.ClosestVertex(SecondPoint);
        Debug.Log(SecondPoint);
        
        // 
        Vector2 ThirdPoint= GetVertexAtDegreeAndDistance(centre, 210, StreetLength);
        Vector2 ThirdPointVertex = Voronoi.ClosestVertex(ThirdPoint);
        Debug.Log(ThirdPoint);
        
        IEnumerable<Vector2> path = Voronoi.Pathfinding(FirstPointVertex, SecondPointVertex);
        IEnumerable<Vector2> path2 = Voronoi.Pathfinding(SecondPointVertex, ThirdPointVertex);
        IEnumerable<Vector2> path3 = Voronoi.Pathfinding(ThirdPointVertex, FirstPointVertex);

        path = path.Concat(path2).Concat(path3);

        path = RemoveIntermediateDuplicates(path);

        GenerateLine(Line, path);

    }
    
    private Vector2 GetVertexAtDegreeAndDistance(Vector2 vertex, float degree, float distance){
        float radian = degree * Mathf.Deg2Rad;
        return new Vector2(vertex.x + distance * Mathf.Cos(radian), vertex.y + distance * Mathf.Sin(radian));
    }
    private Vector2 FindRandomStartPoint(){
        Vector2 randomOffset = new Vector2(Random.Range(-Size.x, +Size.y) / 4f, Random.Range(-Size.x, +Size.y) / 4f);
        Vector2 StartPoint1 = (Vector2) transform.position - Size / 2f + randomOffset;
        Vector2 StartPoint1Vertex = Voronoi.ClosestVertex(StartPoint1);
        return StartPoint1Vertex;
    }

private IEnumerable<Vector2> RemoveIntermediateDuplicates(IEnumerable<Vector2> path)
{
    List<Vector2> nodes = path.ToList();

    // Step 1: Find the true starting index
    int startIndex = 0;
    int count = nodes.Count;

    while (startIndex < count / 2 && nodes[startIndex] == nodes[count - 1 - startIndex]&& nodes[startIndex+1] == nodes[count - 2 - startIndex])
    {
        startIndex++; // Move start index forward as long as it matches the mirrored end
    }

    Vector2 firstNode = nodes[startIndex];
    Vector2 lastNode = nodes[count - 1-startIndex];

    List<Vector2> cleanedPath = new List<Vector2> { firstNode };
    HashSet<Vector2> visited = new HashSet<Vector2> { firstNode };

    // Step 2: Process all nodes except first and last
    for (int i = startIndex + 1; i < count - 1-startIndex; i++)
    {
        Vector2 node = nodes[i];
        if (visited.Contains(node))
        {
            // If duplicate found, remove everything in between
            int duplicateIndex = cleanedPath.IndexOf(node);
            cleanedPath = cleanedPath.Take(duplicateIndex + 1).ToList();
        }
        else
        {
            visited.Add(node);
            cleanedPath.Add(node);
        }
    }

    cleanedPath.Add(lastNode); // Ensure last node is always added

    return cleanedPath;
}


    private void GenerateLine (LineRenderer line, IEnumerable<Vector2> path)
    {
        line.positionCount = path.Count();

        for (int i = 0; i < path.Count(); i++)
            line.SetPosition(i, path.ElementAt(i).X0Y());
    }


    private void GenerateBuildings()
    {
        foreach (Cell cell in Voronoi.Cells)
        {
            // Makes sure the centre of the buildings is at the centroid of the cell
            // So if they are rescaled, the scale nicely "inside" the cell itself
            var vertices = cell.Vertices
                .Select(v => v - cell.Centroid)
                .ToList();
            Mesh mesh = MeshUtils.ExtrudePolygon(vertices, Random.Range(0f, 1f));

            CellBuilding building = Instantiate(BuildingPrefab, cell.Centroid.X0Y(), Quaternion.identity, transform);
            building.GetComponent<MeshFilter>().mesh = mesh;
            building.Cell = cell;

            // Random size
            building.transform.localScale = Vector3.one * Random.Range(0.75f, 1f);
        }
    }

    #region DebugDrag
    /*
    private void DrawVoronoiCells()
    {
        // Loops through all cells
        //for (var i = 0; i < CellsCount; i++)
        foreach (Cell cell in Voronoi.Cells)
        {
            // Loops trough all vertices of the cell
            for (int vi = 0; vi < cell.Vertices.Count; vi ++)
            {
                Debug.DrawLine
                (
                    XYtoX_Z(cell.Vertices[vi]),
                    XYtoX_Z(cell.Vertices[(vi +1) % cell.Vertices.Count]),
                    Color.red, 60f
                );
            }

        }
    }
    */
    [Header("Debug")]
    // 
    public Transform DebugSelector;

    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.white;
    //     Gizmos.DrawWireCube(transform.position, Size);

    //     if (Voronoi == null)
    //         return;

    //     foreach (Cell cell in Voronoi.Cells)
    //     {
    //         Gizmos.color = Color.white;
    //         //Gizmos.DrawSphere(XYtoX_Z(cell.Centre), 0.1f);
    //         Gizmos.DrawSphere(cell.Centre.X0Y(), 0.1f);

    //         Gizmos.color = Color.green;
    //         Gizmos.DrawSphere(cell.Centroid.X0Y(), 0.1f);
    //         //Gizmos.DrawSphere(XYtoX_Z(cell.Centroid), 0.1f);

    //         cell.DebugDraw(Color.red);
    //     }

    //     if (DebugSelector != null)
    //     {
    //         //Cell cell = Voronoi.ClosestCell(X_ZtoXY(DebugSelector.position));
    //         Cell cell = Voronoi.ClosestCell(DebugSelector.position.XZ());
    //         cell.DebugDraw(Color.green);
    //         Debug.DrawLine(DebugSelector.position, cell.Centre.X0Y(), Color.green);
    //         //Debug.DrawLine(DebugSelector.position, XYtoX_Z(cell.Centre), Color.green);
    //     }

    // }
    #endregion
    /*
    private Vector3 XYtoX_Z (Vector2 v2)
    {
        return new Vector3
        (
            v2.x,
            transform.position.y,
            v2.y
        );
    }

    private Vector2 X_ZtoXY (Vector3 v3)
    {
        return new Vector2
        (
            v3.x,
            v3.z
        );
    }
    */
}
