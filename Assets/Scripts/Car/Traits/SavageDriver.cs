using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="SavageDriver",menuName ="Traits/SavageDriver")]
public class SavageDriver : ScriptableObject, ITrait
{
    public CarTraits Trait => CarTraits.SavageDriver;
    public void ApplyTrait(CarControl car)
    {
        car.isSavageDriver = true;
    }
}
