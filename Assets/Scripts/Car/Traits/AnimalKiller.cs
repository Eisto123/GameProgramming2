using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="AnimalKiller",menuName ="Traits/AnimalKiller")]
public class AnimalKiller : ScriptableObject, ITrait
{
    public CarTraits Trait => CarTraits.AnimalKiller;
    public void ApplyTrait(CarControl car)
    {
    }
}
