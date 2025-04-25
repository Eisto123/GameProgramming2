using UnityEngine;
using AlanZucconi.AI.BT;


[CreateAssetMenu(fileName = "BasicFollowCar",
                     menuName = "CarAI/BasicFollowCar")]
public class BasicFollowCar : CarAI
{
    private ITrait Trait1;
    private ITrait Trait2;
    public float SteerSpeed = 5f;

    [Range(0, 1)]
    public float PowerSpeed = 0.3f;

    public override void SetUp(CarControl car)
    {
        Trait1 = TraitManager.instance.GetTrait(Trait1Type);
        Trait2 = TraitManager.instance.GetTrait(Trait2Type);
        Trait1.ApplyTrait(car);
        Trait2.ApplyTrait(car);
    }
    public override Node CreateBehaviourTree(CarControl car)
    {
        return new Selector(
            new Filter(
                ()=>{return car.isSavageDriver;},
                new Selector(
                    new Filter(
                        ()=>{return car.isCarLeft();},
                        new Action
                        (
                            () => car.TurnLeft(PowerSpeed)
                        )   
                    ),
                    new Filter(
                        ()=>{return car.isCarRight();},
                        new Action
                        (
                            () => car.TurnRight(PowerSpeed)
                        )   
                    )
                ) 
            ),
            new Filter(
                ()=>{return car.isRabbitInFront()&&!car.isAnimalKiller;},
                new Action
                    (
                        () => car.HitBrake()
                    )   
            ),
            new Filter(
                ()=>{return car.isFenceInFront();},
                new Action
                    (
                        () => car.TurnRight(0f)
                    )   
            ),
            new Action(() => car.MoveTowardsCurrentTrackPoint(SteerSpeed, PowerSpeed))

        );
    }
}
