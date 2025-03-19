using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarControl : MonoBehaviour
{
    public WheelCollider[] wheelColliders = new WheelCollider[4];
    public GameObject[] wheelMeshes = new GameObject[4];
    public GameObject massCenter;
    public float torque = 200;
    public float steerAngle = 30;
    public float radius = 6f;
    public float downForce = 50;

    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = massCenter.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void FixedUpdate()
    {
        UpdateWheelMeshes();
        Moving();
        AddDownForce();
    }

    private void Moving(){
        if(Input.GetAxis("Vertical") != 0){
            for (int i = 0; i < 4; i++)
            {
                wheelColliders[i].motorTorque = Input.GetAxis("Vertical") * (torque/4);
            }
        }else{  
            for (int i = 0; i < 4; i++)
            {
                wheelColliders[i].motorTorque = 0;
            }
        }

        //ACKERMAN STEERING
        if(Input.GetAxis("Horizontal") > 0){
            wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * Input.GetAxis("Horizontal");
            wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius - (1.5f / 2))) * Input.GetAxis("Horizontal");
        }else if(Input.GetAxis("Horizontal") < 0){
            wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius - (1.5f / 2))) * Input.GetAxis("Horizontal");
            wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * Input.GetAxis("Horizontal");
        }else{
            wheelColliders[0].steerAngle = 0;
            wheelColliders[1].steerAngle = 0;
        }
    }

    private void AddDownForce(){
        rb.AddForce(-transform.up * downForce * rb.velocity.magnitude);
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

    
}
