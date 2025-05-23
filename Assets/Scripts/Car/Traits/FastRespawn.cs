using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="FastRespawn",menuName ="Traits/FastRespawn")]
public class FastRespawn : ScriptableObject, ITrait
{
    public CarTraits Trait => CarTraits.FastRespawn;
    public string Description => "You can respawn faster!";
    public void ApplyTrait(CarControl car)
    {
        car.respawnTreshold -=3;
    }
}
