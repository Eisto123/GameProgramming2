using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="TitanRacer",menuName ="Traits/TitanRacer")]
public class TitanRacer : ScriptableObject, ITrait
{
    public CarTraits Trait => CarTraits.TitanRacer;
    public string Description => "You are a titan! You are bigger than the others!";
    public void ApplyTrait(CarControl car)
    {
        car.gameObject.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
        car.maxPower+=500;
    }
}
