using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlanZucconi.AI.BT;

public class CarControl : MonoBehaviour
{
    public WheelCollider[] wheelColliders = new WheelCollider[4];
    public GameObject[] wheelMeshes = new GameObject[4];
    public GameObject massCenter;
    public float maxPower = 200;
    private float totalPower = 200;
    public float steerAngle = 30;
    public float radius = 6f;
    public float downForce = 50;

    private float wheelRPM;
    private float engineRPM;
    public float smoothTime;

    public AnimationCurve engineCurve;

    public bool manualDriving = false;
    private float horizontalInput = 0;
    private float verticalInput = 0;

    public TrackPoints trackPointsEvent;
    private Vector3[] trackPoints;
    private Vector3 currentTrackPoint;
    public int trackPointsOffset = 0;

    public CarAI carAI;
    private BehaviourTree Tree;
    public bool isBreaking;

    void OnEnable()
    {
        trackPointsEvent.trackPointsEvent += OnTrackPointsEvent;
    }
    void OnDisable()
    {
        trackPointsEvent.trackPointsEvent -= OnTrackPointsEvent;
    }
    private void OnTrackPointsEvent(Vector3[] points)
    {
        trackPoints = points;
    }

    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = massCenter.transform.localPosition;
        if(carAI != null){
            Tree = new BehaviourTree(carAI.CreateBehaviourTree(this));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void FixedUpdate()
    {
        if(manualDriving){
            ReadInput();
        }
        else{
            if(trackPoints != null && trackPoints.Length > 0){
                InputFromAI();
            }
        }

        UpdateWheelMeshes();
        Moving();
        AddDownForce();
        CalculateEnginePower();
        CalculateDistanceToTrackPoints();
    }

    private void InputFromAI(){
        Tree.Update(); //Update the Behavior Tree
        //Geting vertical and horizontal input from Behavior Tree
    }

    private void ReadInput(){
        if(Input.GetAxis("Vertical") != 0){
            verticalInput = Input.GetAxis("Vertical");
        }
        if(Input.GetAxis("Horizontal") != 0){
            horizontalInput = Input.GetAxis("Horizontal");
        }
        if(Input.GetKeyDown(KeyCode.Space)){
            isBreaking = true;
        }
        else if(isBreaking && Input.GetKeyUp(KeyCode.Space)){
            isBreaking = false;
        }
    }

    private void Moving(){
        if(verticalInput != 0){
            for (int i = 0; i < 4; i++)
            {
                wheelColliders[i].motorTorque =  totalPower/4;
            }
        }else{  
            for (int i = 0; i < 4; i++)
            {
                wheelColliders[i].motorTorque = 0;
            }
        }

        //ACKERMAN STEERING
        if(horizontalInput > 0){
            wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * horizontalInput;
            wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius - (1.5f / 2))) * horizontalInput;
        }else if(horizontalInput < 0){
            wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius - (1.5f / 2))) * horizontalInput;
            wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * horizontalInput;
        }else{
            wheelColliders[0].steerAngle = 0;
            wheelColliders[1].steerAngle = 0;
        }

        //BREAKING
        if(isBreaking){
            for (int i = 0; i < 4; i++)
            {
                wheelColliders[i].brakeTorque = 10000;
            }
        }
        else{
            for (int i = 0; i < 4; i++)
            {
                wheelColliders[i].brakeTorque = 0;
            }
        }
    }

    private void AddDownForce(){
        rb.AddForce(-transform.up * downForce * rb.velocity.magnitude);
    }

    private void GetWheelRPM(){
        float sum = 0;
        int r = 0;
        for (int i = 0; i < 4; i++)
        {
            sum += wheelColliders[i].rpm;
            r++;
        }
        wheelRPM = (r!=0) ? sum / r : 0;
    }

    private void CalculateEnginePower(){
        GetWheelRPM();
        float normailizedRPM = Mathf.InverseLerp(0, 1, wheelRPM);
        totalPower = engineCurve.Evaluate(normailizedRPM) * verticalInput * maxPower;

        // This is for UI Displaying speed
        float velocity = 0.0f;
        engineRPM = Mathf.SmoothDamp(engineRPM, 1000 + Mathf.Abs(wheelRPM), ref velocity, smoothTime);
    }

    private void UpdateWheelMeshes(){
        Quaternion quat;
        Vector3 pos;
        for (int i = 0; i < 4; i++)
        {
            wheelColliders[i].GetWorldPose(out pos, out quat);
            wheelMeshes[i].transform.position = pos;
            wheelMeshes[i].transform.rotation = quat;
        }
    }

    public void CalculateDistanceToTrackPoints(){
        Vector3 carPosition = transform.position;
        float minDistance = Mathf.Infinity;

        for(int i = 0; i < trackPoints.Length; i++)
        {
            float distance = Vector3.Distance(carPosition, trackPoints[i]);
            if (distance < minDistance)
            {
                if(i + trackPointsOffset >= trackPoints.Length)
                {
                    currentTrackPoint = trackPoints[i+trackPointsOffset-trackPoints.Length];
                }
                else
                {
                    currentTrackPoint = trackPoints[i+trackPointsOffset];
                }
                minDistance = distance;
            }
        }
    }

#region AI
    public void MoveTowardsCurrentTrackPoint(float SteerForce,float PowerForce)
    {
        Vector3 direction = transform.InverseTransformPoint(currentTrackPoint);
        direction /= direction.magnitude;

        horizontalInput = direction.x / direction.magnitude * SteerForce;
        verticalInput = PowerForce;
    }

    public void MoveTowardsPoint(Vector3 point, float SteerForce, float PowerForce)
    {
        Vector3 direction = transform.InverseTransformPoint(point);
        direction /= direction.magnitude;

        horizontalInput = direction.x / direction.magnitude * SteerForce;
        verticalInput = PowerForce;
    }
    public bool isCarOnLeftSide()
    {
        Vector3 carPosition = transform.position;
        float minDistance = Mathf.Infinity;
        Vector3 closestPoint = Vector3.zero;

        for(int i = 0; i < trackPoints.Length; i++)
        {
            float distance = Vector3.Distance(carPosition, trackPoints[i]);
            if (distance < minDistance)
            {
                closestPoint = trackPoints[i];
                minDistance = distance;
            }
        }

        return (transform.position.x < closestPoint.x);
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(currentTrackPoint, 0.5f);
    }
#endregion
}

