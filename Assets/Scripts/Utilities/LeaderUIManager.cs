using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LeaderUIManager : MonoBehaviour
{
    public GameManager GameManager;
    public GameObject rankingPanel;
    public GameObject carRankingTextTemplate;

    private List<GameObject> currentEntries = new List<GameObject>();

    void Update()
    {
        UpdateLeaderboard();
    }

    void UpdateLeaderboard()
    {
        // Clear previous entries
        foreach (var entry in currentEntries)
        {
            Destroy(entry);
        }
        currentEntries.Clear();
        List<CarControl> cars = new List<CarControl>();
        // Get current sorted cars
        if(GameManager.noWinnerYet){
            cars = GameManager.cars;
        }
        else
        {
            cars = GameManager.FinalLeaderBoard;
        }
         

        for (int i = 0; i < cars.Count; i++)
        {
            CarControl car = cars[i];

            GameObject entry = Instantiate(carRankingTextTemplate, rankingPanel.transform,false);
            entry.SetActive(true);

            TMP_Text textComponent = entry.GetComponent<TMP_Text>();
            textComponent.text = $"{i + 1}. {car.carAI.name} - Lap {car.CurrentLap} Traits: {car.carAI.Trait1Type} {car.carAI.Trait2Type} ";

            currentEntries.Add(entry);
        }
    }
}
