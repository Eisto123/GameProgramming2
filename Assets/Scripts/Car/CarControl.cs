using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlanZucconi.AI.BT;
using System;
using Unity.Mathematics;

public class CarControl : MonoBehaviour
{
    public WheelCollider[] wheelColliders = new WheelCollider[4];
    public GameObject[] wheelMeshes = new GameObject[4];
    public GameObject massCenter;
    public float maxPower = 200;
    private float totalPower = 200;
    //public float steerAngle = 30;
    public float radius = 6f;
    public float downForce = 50;
    public float turnAngleThreshold;
    private float wheelRPM;

    public AnimationCurve engineCurve;

    public bool manualDriving = false;
    private float horizontalInput = 0;
    private float verticalInput = 0;

    public TrackPoints trackPointsEvent;
    private Vector3[] trackPoints;
    private Vector3 currentTrackPoint;
    private Vector3 closestTrackPoint;
    public int trackPointsOffset = 0;

    public CarAI carAI;
    private BehaviourTree Tree;
    public bool isBreaking;
    [Range(0,180)]public float detectionAngle = 5f;
    public float detectionRadius = 5f;
    public LayerMask detectionLayer;

    [HideInInspector] public bool isSavageDriver = false;


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

    public Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = massCenter.transform.localPosition;
        if(carAI != null){
            carAI.SetUp(this);
            Tree = new BehaviourTree(carAI.CreateBehaviourTree(this));
        }
    }
    void Update()
    {
        CheckIfFlipped();
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
            wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(0.7f / (radius + (0.58f / 2))) * horizontalInput;
            wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(0.7f / (radius - (0.58f / 2))) * horizontalInput;
        }else if(horizontalInput < 0){
            wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(0.7f / (radius - (0.58f / 2))) * horizontalInput;
            wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(0.7f / (radius + (0.58f / 2))) * horizontalInput;
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
                closestTrackPoint = trackPoints[i];
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
    private string DetectObjectsInFront()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, detectionRadius, detectionLayer);

        if (rangeChecks.Length != 0)
        {
            Transform target = rangeChecks[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToTarget) < detectionAngle / 2)
            {
                Debug.Log("Object detected in front: " + target.tag);
                return target.tag;
            }
            
        }
        return null;
    }
    private string DetectObjectsInLeft()
    {
        Vector3 origin = transform.position;
        Vector3 direction = -transform.right;

        if (Physics.SphereCast(origin, 2, direction, out RaycastHit hit, 2, detectionLayer))
        {
            Debug.DrawLine(origin, hit.point, Color.red);
            Debug.Log("Object on the left (sphere): " + hit.collider.name);
            return hit.collider.tag;
        }
        return null;
    }
    private string DetectObjectsInRight()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.right;

        if (Physics.SphereCast(origin, 2, direction, out RaycastHit hit, 2, detectionLayer))
        {
            Debug.DrawLine(origin, hit.point, Color.red);
            Debug.Log("Object on the left (sphere): " + hit.collider.name);
            return hit.collider.tag;
        }
        return null;
    }
    

    private float collisiontimer = 0f;
    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Fence"))
        {
            collisiontimer += Time.deltaTime;
            if (collisiontimer >= 5f) // 1 second delay before logging
            {
                Debug.Log("wasted");

                ResetCarPosition(closestTrackPoint+Vector3.up * 2f); // Reset the car's position
                collisiontimer = 0f; // Reset the timer after logging
            }
        }
    }
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Fence"))
        {
            collisiontimer = 0f; // Reset the timer when the car is no longer in contact with the fence
        }
    }
    private float flipTimmer = 0f;
    private void CheckIfFlipped()
    {
        if (transform.up.y < 0.5f)
        {
            flipTimmer += Time.deltaTime;
            Debug.Log("Flipped!");
        }
        else
        {
            flipTimmer = 0f; // Reset the timer if the car is not flipped
        }
        if (flipTimmer >= 5f) // 1 second delay before logging
        {
            Debug.Log("wasted!");
            ResetCarPosition(closestTrackPoint+Vector3.up * 2f);
            flipTimmer = 0f; // Reset the timer after logging
        }

    }
    public void ResetCarPosition(Vector3 position)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = position;
        Vector3 rotation = (currentTrackPoint - closestTrackPoint).normalized;
        rotation.y = 0f; // Keep the y component zero to avoid tilting
        transform.rotation = quaternion.Euler(rotation); 
    }

    #region AI

    public bool isFacingBackWard()
    {
        return Vector3.Dot(transform.forward, (currentTrackPoint - transform.position).normalized) < 0f;
    }
    public bool isFenceInFront()
    {
        String tag = DetectObjectsInFront();
        if(tag == "Fence" && !isFacingBackWard())
        {
            return true;
        }
        return false;
    }
    public bool isCarLeft()
    {
        String tag = DetectObjectsInLeft();
        if(tag == "Player" || tag == "Car")
        {
            return true;
        }
        return false;
    }
    public bool isCarRight()
    {
        String tag = DetectObjectsInRight();
        if(tag == "Player" || tag == "Car")
        {
            return true;
        }
        return false;
    }

    public void MoveTowardsCurrentTrackPoint(float SteerForce,float PowerForce)
    {
        isBreaking = false;
        Vector3 direction = transform.InverseTransformPoint(currentTrackPoint);
        direction /= direction.magnitude;
        
        horizontalInput = SteerForce * direction.x / direction.magnitude ;
        verticalInput = PowerForce;
        
    }

    public void MoveTowardsPoint(Vector3 point, float SteerForce, float PowerForce)
    {
        Vector3 direction = transform.InverseTransformPoint(point);
        direction /= direction.magnitude;

        horizontalInput = direction.x / direction.magnitude * SteerForce;
        verticalInput = PowerForce;
    }


    public void HitBrake(){
        isBreaking = true;
    }

    public void TurnRight(float PowerForce){
        horizontalInput = 1;
        verticalInput = PowerForce;
    }
    public void TurnLeft(float PowerForce){
        horizontalInput = -1;
        verticalInput = PowerForce;
    }
    
    

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(currentTrackPoint, 0.5f);
        if (!Application.isPlaying || currentTrackPoint == Vector3.zero)
        return;
    // Label the angle

    UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"totalpower: {totalPower:F1}°");
    }
#endregion
}

