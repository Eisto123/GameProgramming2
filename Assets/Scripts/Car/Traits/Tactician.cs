using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Tactician",menuName ="Traits/Tactician")]
public class Tactician : ScriptableObject, ITrait
{
    public CarTraits Trait => CarTraits.Tactician;
    public string Description => "You will be able to foresee the track better!";
    public void ApplyTrait(CarControl car)
    {
        
        car.trackPointsOffset += 4;
    }
}
