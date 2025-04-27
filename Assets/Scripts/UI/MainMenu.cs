using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    public string mainScene;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartButton()
    {
        PlayerPrefs.SetString("NextScene", mainScene);
        PlayerPrefs.Save();

        SceneManager.LoadScene("LoadingScene");
        //SceneManager.LoadScene(mainScene);
    }
}
