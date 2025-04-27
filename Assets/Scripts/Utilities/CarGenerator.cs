using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class CarGenerator : MonoBehaviour
{
    [SerializeField] private List<GameObject> Cars;
    [SerializeField] private int carAmount = 4;
    public UnityEvent<Vector3[]> GenerationComplete;
    private List<Vector3> trackPoints = new List<Vector3>();
    public TrackPoints trackPointsEvent;

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
    }
    public void Shuffle(List<GameObject> Cars) {
		var count = Cars.Count;
		var last = count - 1;
		for (var i = 0; i < last; ++i) {
			var r = UnityEngine.Random.Range(i, count);
			var tmp = Cars[i];
			Cars[i] = Cars[r];
			Cars[r] = tmp;
		}
	}
    public void generateCar(){
        Shuffle(Cars);
        for(int i = 0; i<carAmount; i++){
            var car = Instantiate(Cars[i]);
            if(car.tag == "Player"){
                car.GetComponent<CarControl>().carAI.Trait1Type = TraitManager.instance.PlayerSelectedTrait[0].Trait;
                car.GetComponent<CarControl>().carAI.Trait2Type = TraitManager.instance.PlayerSelectedTrait[1].Trait;
            }
            else{
                RandomCarTrait(car);
            }
            
            PlaceCarOnPosition(car, trackPoints[trackPoints.Count-10] + new Vector3((i-2)*2,0.2f,0), quaternion.identity);
        }
        GenerationComplete?.Invoke(trackPoints.ToArray());
        
    }
    private void PlaceCarOnPosition(GameObject car, Vector3 position, quaternion rotation){
        car.transform.position = position;
        car.transform.rotation = rotation;
    }

    private void RandomCarTrait(GameObject car){
        CarTraits traits1 = (CarTraits)UnityEngine.Random.Range(0, 9);
        CarTraits traits2 = (CarTraits)UnityEngine.Random.Range(0, 9);
        while(traits2 == traits1){
            traits2 = (CarTraits)UnityEngine.Random.Range(0, 9);
        }
        car.GetComponent<CarControl>().carAI.Trait1Type = traits1;
        car.GetComponent<CarControl>().carAI.Trait2Type = traits2;
    }
}
