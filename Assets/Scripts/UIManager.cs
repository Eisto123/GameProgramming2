using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using StarterAssets;

public class UIManager : MonoBehaviour
{
    public PlayableDirector director;
    public List<CinemachineVirtualCamera> cameras;
    public CinemachineVirtualCamera cameraPlayer;
    public CinemachineVirtualCamera cameraCar;
    public PlacePath paths;
    public GameObject carChooseMenu;
    public GameObject gameEndMenu;
    public GameObject gameMenu;
    public GameObject player;
    public PlayerController playerControl;

    void Awake()
    {
        director.stopped += OnTimelineStopped;
        director.gameObject.SetActive(false);
        carChooseMenu.SetActive(false);
        gameMenu.SetActive(false);
        player.SetActive(false);
    }

    void OnTimelineStopped(PlayableDirector pd)
    {
        Debug.Log("Timeline over");
        director.gameObject.SetActive(false);
        carChooseMenu.SetActive(true);
    }

    public void PlayStartAnim()
    {
        SetCameras();
        director.gameObject.SetActive(true);
        director.Play();
    }

    private void SetCameras()
    {
        Vector3[] knots = paths.GetKnotPositions();
        int interval = knots.Length/cameras.Count;
        for (int i = 0; i < cameras.Count; i++)
        {
            cameras[i].transform.position = knots[i*interval]+new Vector3(0,20,0);
            cameras[i].transform.forward = knots[i*interval+1]-cameras[i].transform.position;
            Vector3 rotation = cameras[i].transform.rotation.eulerAngles;
            cameras[i].transform.rotation = Quaternion.Euler(26,rotation.y,rotation.z);
        }
        player.transform.position = knots[0]+new Vector3(50,20,50);
    }

    public void ConfirmCar()
    {
        carChooseMenu.SetActive(false);
        gameMenu.SetActive(true);
        player.SetActive(true);
        GameObject car = GameObject.FindGameObjectWithTag("Player");
        cameraCar.Follow = car.transform;
        cameraCar.LookAt = car.transform;
    }

    public void RestartGame()
    {
        string scene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(scene);
    }

    public void ChangeView(int view)
    {
        if (view == 0) // car
        {
            cameraCar.Priority = 12;
            cameraPlayer.Priority = 10;
            playerControl.enabled = false;
        }
        else // player
        {
            cameraCar.Priority = 10;
            cameraPlayer.Priority = 12;
            playerControl.enabled = true;
        }
    }
    public void ShowGameEndMenu()
    {
        gameEndMenu.SetActive(true);
        gameMenu.SetActive(false);
        player.SetActive(false);
    }
}
