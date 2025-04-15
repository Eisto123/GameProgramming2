using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using AlanZucconi.AI.BT;


[CreateAssetMenu(fileName = "BasicFollowCar",
                     menuName = "CarAI/BasicFollowCar")]
public class BasicFollowCar : CarAI
{

    public float SteerSpeed = 5f;

    [Range(0, 1)]
    public float PowerSpeed = 0.3f;
    public override Node CreateBehaviourTree(CarControl car)
    {
        return new Action(() => car.MoveTowardsCurrentTrackPoint(SteerSpeed, PowerSpeed));
    }
}
