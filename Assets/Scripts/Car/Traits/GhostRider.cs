using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="GhostRider",menuName ="Traits/GhostRider")]
public class GhostRider : ScriptableObject, ITrait
{
    public CarTraits Trait => CarTraits.GhostRider;
    public string Description => "You can pass through other cars!";
    public void ApplyTrait(CarControl car)
    {
        car.rb.excludeLayers += LayerMask.GetMask("Car");
    }
}
