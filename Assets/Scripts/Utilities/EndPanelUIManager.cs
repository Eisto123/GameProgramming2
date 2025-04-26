using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EndPanelUIManager : MonoBehaviour
{
    public Sprite[] Metals;
    public GameManager gameManager;
    public GameObject traitContainer;
    public GameObject traitPrefab;
    public TMP_Text rankingText;
    public Image metalImage;

    void Start()
    {
        // Initialize the end panel UI with the final leaderboard
        UpdateEndPanelUI();
    }

    void UpdateEndPanelUI()
    {
        // Clear previous entries
        foreach (Transform child in traitContainer.transform)
        {
            Destroy(child.gameObject);
        }
        for(int i = 0; i < 2; i++)
        {
            GameObject traitGO = Instantiate(traitPrefab, traitContainer.transform, false);
            TMP_Text[] texts = traitGO.GetComponentsInChildren<TMP_Text>();
            texts[0].text = TraitManager.instance.PlayerSelectedTrait[i].Trait.ToString();
            texts[1].text = TraitManager.instance.PlayerSelectedTrait[i].Description;
        }
        int playerRanking = gameManager.GetPlayerRanking();
        rankingText.text = playerRanking.ToString();
        if (playerRanking == 1)
        {
            metalImage.sprite = Metals[0];
        }
        else if (playerRanking == 2)
        {
            metalImage.sprite = Metals[1];
        }
        else if (playerRanking == 3)
        {
            metalImage.sprite = Metals[2];
        }
        else
        {
            metalImage.sprite = Metals[3];
        }

        
    }
}
