using UnityEngine;
using AlanZucconi.AI.BT;


public abstract class CarAI : ScriptableObject
{
    public virtual Node CreateBehaviourTree(CarControl car)
        {
            return null;
        }
}
