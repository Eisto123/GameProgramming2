using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="SteeringMaster",menuName ="Traits/SteeringMaster")]
public class SteeringMaster : ScriptableObject, ITrait
{
    public CarTraits Trait => CarTraits.SteeringMaster;
    public string Description => "Your steering will be better!";
    public void ApplyTrait(CarControl car)
    {
        car.radius -= 0.4f;
    }
}
