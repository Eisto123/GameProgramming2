using System.Collections;
using System.Collections.Generic;
using TMPro;
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

        // Get current sorted cars
        List<CarControl> cars = GameManager.cars;

        for (int i = 0; i < cars.Count; i++)
        {
            CarControl car = cars[i];

            GameObject entry = Instantiate(carRankingTextTemplate, rankingPanel.transform);
            entry.SetActive(true);

            TMP_Text textComponent = entry.GetComponent<TMP_Text>();
            textComponent.text = $"{i + 1}. {car.name} - Lap {car.CurrentLap}";

            currentEntries.Add(entry);
        }
    }
}
