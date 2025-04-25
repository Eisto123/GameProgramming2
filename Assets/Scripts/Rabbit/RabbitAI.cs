using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlanZucconi.AI.BT;
using UnityEngine.AI;

public class RabbitAI
{
    public Node CreateBehaviourTree(Rabbit rabbit)
    {
        //return null;
        return new Selector(
            new Sequence(
                new Condition(() => CheckDie(rabbit)),
                new Action(rabbit.Die)),
            
            // Check rabbit state
            new Selector(
                // Heal (color carrot)
                new Sequence(
                    new Condition(() => {if (rabbit.status == RabbitStatus.Heal) 
                                            return true; 
                                        else return false;}),
                    new Action(rabbit.Heal)
                ),

                // Drink
                new Sequence(
                    new Condition(() => {if (rabbit.status == RabbitStatus.Drink) 
                                            return true; 
                                        else return false;}),
                    new Action(rabbit.Drink)
                ),

                // Eat
                new Sequence(
                    new Condition(() => {if (rabbit.status == RabbitStatus.Eat) 
                                            return true; 
                                        else return false;}),
                    new Action(rabbit.Eat)
                )
            ),

            // Move
            new Action(() => MoveRabbit(rabbit))
        );
    }

    private bool CheckDie(Rabbit rabbit)
    {
        if (rabbit.valueHungry <= 0 || rabbit.valueThirsty <= 0)
        {
            return true;
        }
        else
            return false;
    }

    private bool CheckColorFood(Rabbit rabbit)
    {
        Collider[] hits = rabbit.CheckTarget(LayerMask.GetMask("ColorFood"));
        if (hits.Length == 0) return false;
        else 
            return true;
    }

    // private void MoveRabbit(Rabbit rabbit)
    // {
    //     string target = "ColorFood";
    //     if (rabbit.valueThirsty < rabbit.threshold || rabbit.valueHungry < rabbit.threshold)
    //     {
    //         // at least one of the values reach threshold, deal with the smaller one;
    //         target = rabbit.valueHungry < rabbit.valueThirsty? "Food":"Water";
    //         if (CheckColorFood(rabbit))
    //         {
    //             target = "ColorFood";
    //         }
    //     }
    //     Debug.Log("target layer: " + target);
        
    //     rabbit.SetTargetLayer(target);
        
    //     //rabbit.SearchTarget();
    //     SearchTarget(rabbit, target, out Vector3 targetPos, out bool findTarget);
    //     rabbit.SetTargetPosition(targetPos, findTarget);

    //     rabbit.MoveToTarget();
    //     rabbit.CheckTargetReach();

    // }

    private void MoveRabbit(Rabbit rabbit)
    {
        string target = "ColorFood";
        if (rabbit.valueThirsty < rabbit.threshold || rabbit.valueHungry < rabbit.threshold)
        {
            // at least one of the values reach threshold, deal with the smaller one;
            target = rabbit.valueHungry < rabbit.valueThirsty? "Food":"Water";
            if (CheckColorFood(rabbit))
            {
                target = "ColorFood";
            }
            
            if( SearchTarget(rabbit, target, out Vector3 targetPos, out bool findTarget) )
                rabbit.SetTargetPosition(targetPos, findTarget);
            else
            {
                ChangeTarget(rabbit, target, out targetPos, out findTarget);
                rabbit.SetTargetPosition(targetPos, findTarget);
            }
        }
        else
        {
            // idle
            ChangeTarget(rabbit, target, out  Vector3 targetPos, out bool findTarget);
            rabbit.SetTargetPosition(targetPos, findTarget);
        }

        Debug.Log("target layer: " + target);
        rabbit.SetTargetLayer(target);
        
        //rabbit.SearchTarget();
        //SearchTarget(rabbit, target, out Vector3 targetPos, out bool findTarget);
        //rabbit.SetTargetPosition(targetPos, findTarget);

        rabbit.MoveToTarget();
        rabbit.CheckTargetReach();

    }

    // public void SearchTarget(Rabbit rabbit, string targetLayer, out Vector3 targetPos, out bool hasTarget)
    // {
    //     Transform rabbitTransform = rabbit.transform;
    //     TerrainData genTerrain = rabbit.genTerrain;
    //     Vector3 target = rabbit.GetTarget();
    //     hasTarget = rabbit.hasTarget;
    //     NavMeshAgent agent = rabbit.agent;

    //     Collider[] hits = Physics.OverlapSphere(rabbitTransform.position+new Vector3(0,0.2f,0), rabbit.searchRadius, LayerMask.GetMask(targetLayer));
    //     if (hits.Length != 0)
    //     {
    //         Debug.Log("yes hit");
    //         Vector3 pos = hits.MinBy(obj => Vector3.Distance(rabbitTransform.position, obj.transform.position))
    //             .ClosestPoint(rabbit.transform.position);
    //         pos = new Vector3(pos.x, genTerrain.GetInterpolatedHeight(pos.x/genTerrain.size.x, pos.z/genTerrain.size.z), pos.z);

    //         if (targetLayer == "Water" && (pos.y > 8 || pos.y < 7.5f))
    //             Debug.Log("No water point < 8 &&  > 7.5");
    //         else
    //         {
    //             targetPos = pos;
    //             hasTarget = true;
    //             return;
    //         }
    //     }
    //     Debug.Log("hastarget? "+hasTarget);
    //     Debug.Log("no hit");
    //     //target = transform.position + new Vector3(Random.Range(0,1f),Random.Range(0,1f),Random.Range(0,1f));

    //     // if not has a target(food/water) before, last time is a random target pos
    //     // check whether has reached the last pos
    //     if (!hasTarget)
    //     {
    //         //not reached, keep approaching
    //         if (agent.remainingDistance > 0.5f)
    //         {
    //             targetPos = target;
    //             return;
    //         }

    //         //else reached, change a random pos target
    //     }

    //     // if has a target(food/water) last update
    //     // has lost the target, generate a new one
    //     float x = rabbitTransform.position.x + (UnityEngine.Random.Range(0,2)*2-1)*10;
    //     float z = rabbitTransform.position.z + (UnityEngine.Random.Range(0,2)*2-1)*10;
    //     float y = genTerrain.GetInterpolatedHeight(x/genTerrain.size.x, z/genTerrain.size.z);

    //     int maxTries = 100;
    //     int tries = 0;
        
    //     while (tries < maxTries)
    //     {
    //         tries++;
    //         x = rabbitTransform.position.x + (UnityEngine.Random.Range(0,2)*2-1)*10;
    //         z = rabbitTransform.position.z + (UnityEngine.Random.Range(0,2)*2-1)*10;
    //         y = genTerrain.GetInterpolatedHeight(x/genTerrain.size.x, z/genTerrain.size.z);
    //         if (y < 7.5)
    //             continue;
            
    //         // if is finding water and hasn't reach a water point
    //         // try to go lower
    //         else if (targetLayer == "Water" && target.y <= 7.5f)
    //             break;
    //         else if (targetLayer == "Water" && target.y > 7.5f && y > target.y)
    //             continue;

    //         else 
    //             break;
    //     }

    //     if (tries == maxTries)
    //     {
    //         Debug.LogError("target: " + target + " fail: " + new Vector3(x, y, z));
    //     }

    //     targetPos = new Vector3(x, y, z);
        
    //     hasTarget = false;
    // }

    public bool SearchTarget(Rabbit rabbit, string targetLayer, out Vector3 targetPos, out bool hasTarget)
    {
        Transform rabbitTransform = rabbit.transform;
        TerrainData genTerrain = rabbit.genTerrain;
        Vector3 target = rabbit.GetTarget();
        targetPos = target;
        hasTarget = rabbit.hasTarget;

        //Collider[] hits = Physics.OverlapSphere(rabbitTransform.position+new Vector3(0,0.2f,0), rabbit.searchRadius, LayerMask.GetMask(targetLayer));
        Collider[] hits = rabbit.CheckTarget(LayerMask.GetMask(targetLayer));
        if (hits.Length != 0)
        {
            Debug.Log("yes hit");
            Vector3 pos = hits.MinBy(obj => Vector3.Distance(rabbitTransform.position, obj.transform.position))
                .ClosestPoint(rabbit.transform.position);
            pos = new Vector3(pos.x, genTerrain.GetInterpolatedHeight(pos.x/genTerrain.size.x, pos.z/genTerrain.size.z), pos.z);

            if (targetLayer == "Water" && (pos.y > 8 || pos.y < 7.5f))
                Debug.Log("No water point < 8 &&  > 7.5");
            else
            {
                targetPos = pos;
                hasTarget = true;
                return true;
            }
        }
        Debug.Log("hastarget? "+hasTarget);
        Debug.Log("no hit");
        //target = transform.position + new Vector3(Random.Range(0,1f),Random.Range(0,1f),Random.Range(0,1f));

        
        return false;
    }

    public void ChangeTarget(Rabbit rabbit, string targetLayer, out Vector3 targetPos, out bool hasTarget)
    {
        Transform rabbitTransform = rabbit.transform;
        TerrainData genTerrain = rabbit.genTerrain;
        Vector3 target = rabbit.GetTarget();
        
        // if not has a target(food/water) before, last time is a random target pos
        // check whether has reached the last pos
        if (!rabbit.hasTarget)
        {
            //not reached, keep approaching, no need to change target
            if (rabbit.agent.remainingDistance > 0.5f)
            {
                targetPos = target;
                hasTarget = false;
                return;
            }

            //else reached, change a random pos target
        }

        // if has a target(food/water) last update
        // has lost the target, generate a new one
        float x = rabbitTransform.position.x + (UnityEngine.Random.Range(0,2)*2-1)*10;
        float z = rabbitTransform.position.z + (UnityEngine.Random.Range(0,2)*2-1)*10;
        float y = genTerrain.GetInterpolatedHeight(x/genTerrain.size.x, z/genTerrain.size.z);

        int maxTries = 100;
        int tries = 0;
        
        while (tries < maxTries)
        {
            tries++;
            x = rabbitTransform.position.x + (UnityEngine.Random.Range(0,2)*2-1)*10;
            z = rabbitTransform.position.z + (UnityEngine.Random.Range(0,2)*2-1)*10;
            y = genTerrain.GetInterpolatedHeight(x/genTerrain.size.x, z/genTerrain.size.z);
            if (y < 7.5)
                continue;
            
            // if is finding water and hasn't reach a water point
            // try to go lower
            else if (targetLayer == "Water" && target.y <= 7.5f)
                break;
            else if (targetLayer == "Water" && target.y > 7.5f && y > target.y)
                continue;

            else 
                break;
        }

        if (tries == maxTries)
        {
            Debug.LogError("target: " + target + " fail: " + new Vector3(x, y, z));
        }

        targetPos = new Vector3(x, y, z);
        hasTarget = false;
    }
}
