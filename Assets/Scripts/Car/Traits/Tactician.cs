using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Tactician",menuName ="Traits/Tactician")]
public class Tactician : ScriptableObject, ITrait
{
    public CarTraits Trait => CarTraits.Tactician;
    public void ApplyTrait(CarControl car)
    {
        car.detectionRadius += 20f;
        car.trackPointsOffset += 2;
    }
}
