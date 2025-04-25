using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public TrackPoints trackPointsEvent;
    public List<Vector3> trackPoints = new List<Vector3>();
    public List<CarControl> cars = new List<CarControl>();
    

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

        // Debug print ranking
        for (int i = 0; i < cars.Count; i++)
        {
            Debug.Log($"{i + 1}: {cars[i].name} - Lap {cars[i].CurrentLap}, CP Index {GetCheckpointIndex(cars[i].currentTrackPoint)}");
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
        int indexA = GetCheckpointIndex(a.currentTrackPoint);
        int indexB = GetCheckpointIndex(b.currentTrackPoint);

        return indexB.CompareTo(indexA); // Descending
    }

    int GetCheckpointIndex(Vector3 checkpoint)
    {
        return trackPoints.IndexOf(checkpoint);
    }

}
