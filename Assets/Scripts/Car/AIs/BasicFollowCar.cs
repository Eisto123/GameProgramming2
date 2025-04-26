using UnityEngine;
using AlanZucconi.AI.BT;


[CreateAssetMenu(fileName = "BasicFollowCar",
                     menuName = "CarAI/BasicFollowCar")]
public class BasicFollowCar : CarAI
{
    public string AIName;
    private ITrait Trait1;
    private ITrait Trait2;
    public ScriptableObject Trait1SO;
    public ScriptableObject Trait2SO;
    public float SteerSpeed = 5f;

    [Range(0, 1)]
    public float PowerSpeed = 0.3f;

    public override void SetUp(CarControl car)
    {
        
        Trait1SO = TraitManager.instance.GetTraitSO(Trait1Type);
        Trait2SO = TraitManager.instance.GetTraitSO(Trait2Type);
        Trait1 = Trait1SO as ITrait;
        Trait2 = Trait2SO as ITrait;

        Trait1.ApplyTrait(car);
        Trait2.ApplyTrait(car);
    }
    public override Node CreateBehaviourTree(CarControl car)
    {
        return new Selector(
            new Filter(
                ()=>{return car.finishedLine;},
                new Action
                    (
                        () => car.HitBrake()
                    )   
            ),
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
                ()=>{return car.isObsticleFront();},
                new Action
                    (
                        () => car.TurnRight(0.5f)
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
