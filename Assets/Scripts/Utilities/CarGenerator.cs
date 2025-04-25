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
    public void generateCar(Vector3[] knotArray){
        
        for(int i = 0; i<carAmount; i++){
            var car = Instantiate(Cars[i]);
            if(car.tag == "Player"){
                car.GetComponent<CarControl>().carAI.Trait1Type = TraitManager.instance.PlayerSelectedTrait[0].Trait;
                car.GetComponent<CarControl>().carAI.Trait2Type = TraitManager.instance.PlayerSelectedTrait[1].Trait;
            }
            else{
                RandomCarTrait(car);
            }
            
            PlaceCarOnPosition(car, knotArray[knotArray.Length-1] + new Vector3((i-2)*2,0.2f,0), quaternion.identity);
        }
        GenerationComplete?.Invoke(knotArray);
        
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
