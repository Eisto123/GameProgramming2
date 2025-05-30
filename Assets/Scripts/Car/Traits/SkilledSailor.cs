using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="SkilledSailor",menuName ="Traits/SkilledSailor")]
public class SkilledSailor : ScriptableObject, ITrait
{
    public CarTraits Trait => CarTraits.SkilledSailor;
    public string Description => "You can drive ignoring water!";
    public void ApplyTrait(CarControl car)
    {
        car.rb.excludeLayers += LayerMask.GetMask("Water");
    }
}
