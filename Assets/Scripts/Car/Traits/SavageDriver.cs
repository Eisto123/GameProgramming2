using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="SavageDriver",menuName ="Traits/SavageDriver")]
public class SavageDriver : ScriptableObject, ITrait
{
    public CarTraits Trait => CarTraits.SavageDriver;
    public string Description => "You will tend to smash on other cars!";
    public void ApplyTrait(CarControl car)
    {
        car.isSavageDriver = true;
    }
}
