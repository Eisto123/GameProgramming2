using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingMenu : MonoBehaviour
{
    public GameObject Loading1;
    public GameObject Loading2;
    public GameObject Loading3;
    public GameObject Loading4;
    private int i = 0;
    //public Slider progressBar;
    public TextMeshProUGUI progressText;
    private void Start()
    {
        StartCoroutine(LoadNextSceneAsync());
    }

    IEnumerator LoadNextSceneAsync()
    {
        string nextSceneName = PlayerPrefs.GetString("NextScene");

        AsyncOperation operation = SceneManager.LoadSceneAsync(nextSceneName);

        while (!operation.isDone)
        {
            // operation.progress 是 0 ~ 0.9，加载完成后直接跳1
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            // if (progressBar != null)
            //     progressBar.value = progress;
            if (progressText != null)
                progressText.text = (progress * 100f).ToString("F0") + "%";

            yield return null;
        }

        // while (!operation.isDone)
        // {
            
        //     i++;
        //     switch(i % 4)
        //     {
        //         case 0:
        //             Loading1.SetActive(true);
        //             Loading2.SetActive(false);
        //             Loading3.SetActive(false);
        //             Loading4.SetActive(false);
        //             break;
        //         case 1:
        //             Loading2.SetActive(true);
        //             Loading1.SetActive(false);
        //             Loading3.SetActive(false);
        //             Loading4.SetActive(false);
        //             break;
        //         case 2:
        //             Loading3.SetActive(true);
        //             Loading1.SetActive(false);
        //             Loading2.SetActive(false);
        //             Loading4.SetActive(false);
        //             break;
        //         case 3:
        //             Loading4.SetActive(true);
        //             Loading1.SetActive(false);
        //             Loading3.SetActive(false);
        //             Loading2.SetActive(false);
        //             break;
        //     }

        //     yield return null;
        // }
    }
}
