using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="AnimalKiller",menuName ="Traits/AnimalKiller")]
public class AnimalKiller : ScriptableObject, ITrait
{
    public CarTraits Trait => CarTraits.AnimalKiller;
    public string Description => "Ignore all the rabbits, smash on it!";
    public void ApplyTrait(CarControl car)
    {
        car.isAnimalKiller = true;
    }
}
