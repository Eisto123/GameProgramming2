using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public TrackPoints trackPointsEvent;
    public List<Vector3> trackPoints = new List<Vector3>();
    public List<GameObject> cars = new List<GameObject>();
    private Vector3 StartPoint;
    
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
            if (!cars.Contains(car.gameObject))
            {
                cars.Add(car.gameObject);
            }
        }
    }


}
