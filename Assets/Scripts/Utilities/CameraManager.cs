using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;

    public void SetFollowPlayer()
    {
        virtualCamera.Follow = GameObject.FindGameObjectWithTag("Player").transform;
        virtualCamera.LookAt = GameObject.FindGameObjectWithTag("Player").transform;
    }
}
