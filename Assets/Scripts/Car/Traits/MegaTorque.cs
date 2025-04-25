using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="MegaTorque",menuName ="Traits/MegaTorque")]
public class MegaTorque : ScriptableObject, ITrait
{
    public CarTraits Trait => CarTraits.MegaTorque;
    public string Description => "Your maximum power is increased by 300!";
    public void ApplyTrait(CarControl car)
    {
        car.maxPower += 300f;
    }
}
