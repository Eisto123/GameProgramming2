using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TraitManager : MonoBehaviour
{
    public static TraitManager instance;
    public ScriptableObject[] traitSOs;
    public List<ITrait> PlayerSelectedTrait = new List<ITrait>();
    public List<ITrait> traits = new List<ITrait>();
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        foreach (var traitSO in traitSOs)
        {
            ITrait trait = traitSO as ITrait;
            if (trait != null)
            {
                traits.Add(trait);
            }
        }
    }

    public ITrait GetTrait(CarTraits traitType)
    {
        foreach (var trait in traits)
        {
            if (trait.Trait == traitType)
            {
                return trait;
            }
        }
        return null;
    }
    public ScriptableObject GetTraitSO(CarTraits traitType)
    {
        foreach (var trait in traitSOs)
        {
            ITrait traitSO = trait as ITrait;
            if (traitSO != null && traitSO.Trait == traitType)
            {
                return trait;
            }
        }
        return null;
    }
}
