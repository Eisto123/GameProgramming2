using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject cameraPivot;

    public float cameraMoveSpeed = 1;
    public float cameraRotateSpeed = 1;
    public float cameraScaleSpeed = 1;

    private float moveX = 0;
    private float moveY = 0;
    private float moveZ = 0;
    
    private float rotateY = 0;
    private float rotateX = 0;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        moveX = 0;
        moveY = 0;
        moveZ = 0;
        
        rotateY = 0;
        rotateX = 0;

        if (Input.GetMouseButton(2))
        {
            moveX = -Input.GetAxis("Mouse X") * cameraMoveSpeed;
            moveY = -Input.GetAxis("Mouse Y") * cameraMoveSpeed;
        }

        moveZ = Input.GetAxis("Mouse ScrollWheel") * cameraScaleSpeed;


        mainCamera.transform.position += transform.right * moveX + transform.up * moveY + transform.forward * moveZ;
        

        if (Input.GetMouseButton(1))
        {
            // yaw
            rotateY = Input.GetAxis("Mouse X") * cameraMoveSpeed;

            // pitch
            rotateX = -Input.GetAxis("Mouse Y") * cameraMoveSpeed;
        }

        Vector3 eulerAngles = cameraPivot.transform.eulerAngles;
        eulerAngles.y += rotateY;
        eulerAngles.x += rotateX;
        cameraPivot.transform.eulerAngles = eulerAngles;

    }
}
