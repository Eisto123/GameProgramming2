using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public TrackPoints trackPointsEvent;
    public int totalLaps = 10;
    public bool noWinnerYet = true;
    public List<Vector3> trackPoints = new List<Vector3>();
    public List<CarControl> cars = new List<CarControl>();
    public List<CarControl> FinalLeaderBoard = new List<CarControl>();
    public bool GameEnded = false;
    public UnityEvent OnGameEndEvent;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void OnEnable()
    {
        trackPointsEvent.trackPointsEvent += OnTrackPointsEvent;
    }
    void OnDisable()
    {
        trackPointsEvent.trackPointsEvent -= OnTrackPointsEvent;
    }
    void OnTrackPointsEvent(Vector3[] points)
    {
        trackPoints.Clear();
        trackPoints.AddRange(points);
        var allCars = FindObjectsOfType<CarControl>();
        foreach (var car in allCars)
        {
            if (!cars.Contains(car))
            {
                cars.Add(car);
            }
        }
    }
    void Update()
    {
        cars.Sort(CompareCars);
        CheckWinner();

        // Debug print ranking
        // for (int i = 0; i < cars.Count; i++)
        // {
        //     Debug.Log($"{i + 1}: {cars[i].name} - Lap {cars[i].CurrentLap}, CP Index {GetCheckpointIndex(cars[i].currentTrackPoint)}");
        // }
    }
    void CheckWinner()
    {
        if (cars.Count == 0) return;
        for (int i = 0; i < cars.Count; i++)
        {
            if (cars[i].CurrentLap >= totalLaps)
            {
                FinalLeaderBoard.Add(cars[i]);
                noWinnerYet = false;
                Debug.Log($"{cars[i].name} is the winner!");
                cars.RemoveAt(i);
                if (cars.Count == 0)
                {
                    OnGameEndEvent?.Invoke();
                    Debug.Log("All cars have finished!");
                }
                break;
                
            }
        }
        
    }


    int CompareCars(CarControl a, CarControl b)
    {
        // Compare laps first
        if (a.CurrentLap != b.CurrentLap)
        {
            return b.CurrentLap.CompareTo(a.CurrentLap); // Descending
        }

        // If laps are equal, compare checkpoint index
        int indexA = GetCheckpointIndex(a.closestTrackPoint);
        int indexB = GetCheckpointIndex(b.closestTrackPoint);

        return indexB.CompareTo(indexA); // Descending
    }

    int GetCheckpointIndex(Vector3 checkpoint)
    {
        return trackPoints.IndexOf(checkpoint);
    }
    public int GetPlayerRanking(){
        foreach(var car in FinalLeaderBoard){
            if(car.gameObject.CompareTag("Player")){
                return FinalLeaderBoard.IndexOf(car)+1;
            }
        }
        return -1; // Player not found in the leaderboard
    }

}
