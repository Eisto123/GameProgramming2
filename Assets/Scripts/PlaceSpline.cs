using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

public class PlaceSpline : MonoBehaviour
{
    public bool isVisualizing = false;
    [SerializeField] private SplineContainer splineContainer;

    [SerializeField] private int splineIndex;

    [SerializeField] [Range(0f,1f)] private float splineTime;

    private float3 position;
    private float3 forward;
    private float3 upVector;

    private List<float3> rightPoints;
    private List<float3> leftPoints;
    [SerializeField]private float roadWidth;
    public float resolution;
    [SerializeField]private MeshFilter meshFilter;

    public int randomRange;


    // Start is called before the first frame update
    void Start()
    {
        RandomizeSpline();
        MapKnot();
    }
    void Update()
    {
        InitializeMeshConnection();
        GenerateMesh();
    }



    private void GetRightAndLeftPoint(float t,out float3 p1, out float3 p2){
        splineContainer.Evaluate(1, t, out position, out forward, out upVector);
        float3 rightPoint = Vector3.Cross(forward, upVector).normalized;
        p1 = position + rightPoint * roadWidth;
        p2 = position - rightPoint * roadWidth;
    }

    private void InitializeMeshConnection(){
        rightPoints = new List<float3>();
        leftPoints = new List<float3>();

        float step = 1f/resolution;
        for(int i = 0; i<resolution; i++){
            float t = i * step;
            GetRightAndLeftPoint(t, out float3 p1, out float3 p2);
            rightPoints.Add(p1);
            leftPoints.Add(p2);
        }

    }

    private void GenerateMesh(){
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        int offset = 0;

        int length = rightPoints.Count;

        for(int i = 1; i<=length; i++){
            Vector3 p1 = rightPoints[i-1];
            Vector3 p2 = leftPoints[i-1];
            Vector3 p3;
            Vector3 p4;

            if(i == length){
                p3 = rightPoints[0];
                p4 = leftPoints[0];
            }
            else{
                p3 = rightPoints[i];
                p4 = leftPoints[i];
            }

            offset = 4*(i-1);

            int t1 = offset+0;
            int t2 = offset+2;
            int t3 = offset+3;
            int t4 = offset+3;
            int t5 = offset+1;
            int t6 = offset+0;

            vertices.AddRange(new List<Vector3>(){p1,p2,p3,p4});
            triangles.AddRange(new List<int>(){t1,t2,t3,t4,t5,t6});
        }
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles,0);
        meshFilter.mesh = mesh;

        
        
    }

    void OnDrawGizmos()
    {
        if (!isVisualizing) return;
        Handles.matrix = transform.localToWorldMatrix;

        if (rightPoints == null || leftPoints == null) return;

        for (int i = 0; i < rightPoints.Count - 1; i++)
        {
            Handles.color = Color.red;
            Handles.SphereHandleCap(0, rightPoints[i], Quaternion.identity, 0.3f, EventType.Repaint);
            Handles.color = Color.blue;
            Handles.SphereHandleCap(0, leftPoints[i], Quaternion.identity, 0.3f, EventType.Repaint);
        }
    }

    private void RandomizeSpline(){
        var knotArray = splineContainer.Spline.ToArray();
        for(int i = 0; i<knotArray.Count(); i++){
            var knot = knotArray[i];
            knot.Position = knot.Position + new float3(UnityEngine.Random.Range(-randomRange,randomRange),0,UnityEngine.Random.Range(-randomRange,randomRange));
            
            splineContainer.Splines[0].SetKnot(i,knot);
        }
    }

    private void MapKnot(){
        SetKnotAmount();
        var knotArray = splineContainer.Splines[1].ToArray();
        for(int i = 0; i<knotArray.Length; i++){
            Vector3 position = FindTerrainPosition(knotArray[i].Position);
            if(position != Vector3.zero){
                knotArray[i].Position = position;
                splineContainer.Splines[1].SetKnot(i,knotArray[i]);
            }
            else{
                Debug.LogError("No terrain found at knot position");
            }
        }
    }

    private Vector3 FindTerrainPosition(Vector3 position){
        RaycastHit hit;
        if(Physics.Raycast(position, Vector3.down, out hit, 1000f)){
            return hit.point + Vector3.up * 0.2f;
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
}
