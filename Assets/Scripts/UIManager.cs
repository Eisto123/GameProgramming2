using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;

public class UIManager : MonoBehaviour
{
    public PlayableDirector director;
    public List<CinemachineVirtualCamera> cameras;
    public CinemachineVirtualCamera cameraPlayer;
    public CinemachineVirtualCamera cameraCar;
    public PlacePath paths;
    public GameObject carChooseMenu;
    public GameObject gameMenu;

    void Awake()
    {
        director.stopped += OnTimelineStopped;
        director.gameObject.SetActive(false);
        carChooseMenu.SetActive(false);
        gameMenu.SetActive(false);
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
        StartCoroutine(PlayAnim());
    }

    private IEnumerator PlayAnim()
    {
        yield return new WaitForSeconds(1);

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
    }

    public void ConfirmCar()
    {
        carChooseMenu.SetActive(false);
        gameMenu.SetActive(true);
    }

    public void RestartGame()
    {
        string scene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(scene);
    }
}
