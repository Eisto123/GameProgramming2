using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName ="TrackPoints",menuName ="EventSO/TrackPoints")]
public class TrackPoints : ScriptableObject
{
    public UnityAction<Vector3[]> trackPointsEvent;

    public void InvokeTrackPointEvent(Vector3[] points)
    {
        trackPointsEvent?.Invoke(points);
    }
}
