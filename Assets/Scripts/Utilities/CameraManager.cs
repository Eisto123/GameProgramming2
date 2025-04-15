using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraManager : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;
    [ContextMenu("TestRegenerate")]
    public void TestRegenerate(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void SetFollowPlayer()
    {
        virtualCamera.Follow = GameObject.FindGameObjectWithTag("Player").transform;
        virtualCamera.LookAt = GameObject.FindGameObjectWithTag("Player").transform;
    }
}
