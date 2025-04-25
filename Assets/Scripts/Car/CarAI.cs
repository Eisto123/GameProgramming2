using UnityEngine;
using AlanZucconi.AI.BT;

public abstract class CarAI : ScriptableObject
{
    public CarTraits Trait1Type;
    public CarTraits Trait2Type;
    public virtual void SetUp(CarControl car)
    {
        // Setup the AI for the car
    }
    public virtual Node CreateBehaviourTree(CarControl car)
        {
            return null;
        }
}
