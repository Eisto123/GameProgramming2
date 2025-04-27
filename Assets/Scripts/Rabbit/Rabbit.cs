using UnityEngine;
using AlanZucconi.AI.BT;
using UnityEngine.AI;
using System.Linq;
using System;

public enum RabbitStatus
{
    Move,
    Drink,
    Eat,
    Heal
}

public class Rabbit : MonoBehaviour
{
    public RabbitStatus status;
    public float searchRadius;
    public float reachRadius;
    public TerrainData genTerrain;
    public NavMeshAgent agent;
    public bool hasTarget = true;

    [Header("Threshold")]
    public int maxValue = 20;
    public int threshold = 10;

    [Header("Drink")]
    public float valueThirsty;
    [Range(0f,2f)]
    public float dropSpeedThirsty;
    [Range(1f,5f)]
    public float drinkSpeed;
    public int drinkTime;
    private float drinkTimer;

    [Header("Eat")]
    public float valueHungry;
    [Range(0f,2f)]
    public float dropSpeedHungry;
    [Range(1f,5f)]
    public float eatSpeed;
    public int eatTime;
    private float eatTimer;

    [Header("Heal")]
    [Range(5f,10f)]
    public float healSpeed;
    public int healTime;
    private float healTimer;

    private string targetLayer;
    private Vector3 target;
    private RabbitAI rabbitAI;
    private BehaviourTree BTree;
    private Animator rabbit;
    
    // Start is called before the first frame update
    void Start()
    {
        rabbitAI = new RabbitAI();
        BTree = new BehaviourTree(rabbitAI.CreateBehaviourTree(this));

        valueHungry = UnityEngine.Random.Range(threshold+1, maxValue);
        valueThirsty = UnityEngine.Random.Range(threshold+1, maxValue);

        agent = GetComponent<NavMeshAgent>();
        agent.autoBraking = false;
        rabbit = GetComponent<Animator>();

        drinkTimer = drinkTime;
        eatTimer = eatTime;
        healTimer = healTime;
    }

    // Update is called once per frame
    void Update()
    {
        BTree.Update();
    }

    public Vector3 GetTarget()
    {
        return target;
    }

    public void SetStatus(RabbitStatus status)
    {
        this.status = status;
    }
    
    public void SetTargetLayer(string target)
    {
        this.targetLayer = target;
    }

    public void SetTargetPosition(Vector3 target, bool hastarget)
    {
        this.target = target;
        this.hasTarget = hastarget;
    }

    public void Die()
    {
        Destroy(this.gameObject);
    }

    public void Drink()
    {
        drinkTimer -= Time.deltaTime;
        valueThirsty += UnityEngine.Random.Range(0.5f,1.5f)*Time.deltaTime*drinkSpeed;
        valueThirsty = Mathf.Clamp(valueThirsty, 0, maxValue);
        if (valueThirsty >= maxValue || drinkTimer <= 0)
        {
            status = RabbitStatus.Move;
            drinkTimer = drinkTime;
            rabbit.SetBool("bow", false);
            return;
        }
    }

    public void Eat()
    {
        eatTimer -= Time.deltaTime;
        valueHungry += UnityEngine.Random.Range(0.5f,1.5f)*Time.deltaTime*eatSpeed;
        valueHungry = Mathf.Clamp(valueHungry, 0, maxValue);
        if (valueHungry >= maxValue || eatTimer <= 0)
        {
            status = RabbitStatus.Move;
            eatTimer = eatTime;
            rabbit.SetBool("bow", false);
            return;
        }
    }

    public void Heal()
    {
        healTimer -= Time.deltaTime;
        // valueHungry += UnityEngine.Random.Range(0.5f,1.5f)*Time.deltaTime*eatSpeed;
        // valueThirsty += UnityEngine.Random.Range(0.5f,1.5f)*Time.deltaTime*drinkSpeed;
        // valueHungry = Mathf.Clamp(valueHungry, 0, maxValue);
        // valueThirsty = Mathf.Clamp(valueThirsty, 0, maxValue);
        valueHungry = maxValue;
        valueThirsty = maxValue;
        // if ((valueThirsty >= maxValue && valueHungry >= maxValue) || healTimer <= 0)
        if (healTimer <= 0)
        {
            status = RabbitStatus.Move;
            healTimer = healTime;
            rabbit.SetBool("bow", false);
            return;
        }
    }

    public Collider[] CheckTarget(LayerMask checkLayer)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position+new Vector3(0,0.2f,0), searchRadius, checkLayer);
        return hits;
    }

    // public void SearchTarget()
    // {
    //     Debug.Log("target: "+targetLayer);
    //     Collider[] hits = Physics.OverlapSphere(transform.position+new Vector3(0,0.2f,0), searchRadius, LayerMask.GetMask(targetLayer));
    //     if (hits.Length != 0)
    //     {
    //         Debug.Log("yes hit");
    //         Vector3 targetPos = hits.MinBy(obj => Vector3.Distance(transform.position, obj.transform.position))
    //             .ClosestPoint(transform.position);
    //         targetPos = new Vector3(targetPos.x, genTerrain.GetInterpolatedHeight(targetPos.x/genTerrain.size.x, targetPos.z/genTerrain.size.z), targetPos.z);

    //         if (targetLayer == "Water" && (targetPos.y > 8 || targetPos.y < 7.5f))
    //             Debug.Log("No water point < 8 &&  > 7.5");
    //         else
    //         {
    //             target = targetPos;
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
    //             return;

    //         //else reached, change a random pos target
    //     }

    //     // if has a target(food/water) last update
    //     // has lost the target, generate a new one
    //     float x = transform.position.x + (UnityEngine.Random.Range(0,2)*2-1)*10;
    //     float z = transform.position.z + (UnityEngine.Random.Range(0,2)*2-1)*10;
    //     float y = genTerrain.GetInterpolatedHeight(x/genTerrain.size.x, z/genTerrain.size.z);

    //     int maxTries = 100;
    //     int tries = 0;
        
    //     while (tries < maxTries)
    //     {
    //         tries++;
    //         x = transform.position.x + (UnityEngine.Random.Range(0,2)*2-1)*10;
    //         z = transform.position.z + (UnityEngine.Random.Range(0,2)*2-1)*10;
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

    //     target = new Vector3(x, y, z);
        
    //     hasTarget = false;
    // }

    public void MoveToTarget()
    {
        //transform.position = Vector3.MoveTowards(transform.position, target,Time.deltaTime);
        //transform.rotation = Quaternion.LookRotation(new Vector3(target.x, transform.position.y, target.z));
        agent.destination = target;

        valueThirsty -= UnityEngine.Random.Range(0.5f,1.5f) * dropSpeedThirsty * Time.deltaTime;
        valueHungry -= UnityEngine.Random.Range(0.5f,1.5f) * dropSpeedHungry * Time.deltaTime;
    }

    public void CheckTargetReach()
    {
        if (!hasTarget) return;
        if (agent.pathPending)
        {
            Debug.Log("path pending");
        }

        // if (agent.remainingDistance < 0.5f && targetLayer == "ColorFood")
        // {
        //     this.status = RabbitStatus.Heal;
        // }
        // else if (agent.remainingDistance < 0.5f)
        // {
        //     this.status = targetLayer == "Water"? RabbitStatus.Drink:RabbitStatus.Eat;
        // }

        if (agent.remainingDistance < 0.5f)
        {
            switch(targetLayer)
            {
                case "ColorFood":
                    this.status = RabbitStatus.Heal;
                    rabbit.SetBool("bow", true);
                    break;
                case "Water":
                    this.status = RabbitStatus.Drink;
                    rabbit.SetBool("bow", true);
                    break;
                case "Food":
                    this.status = RabbitStatus.Eat;
                    rabbit.SetBool("bow", true);
                    break;
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position+new Vector3(0,0.2f,0), searchRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, reachRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(target, 0.5f);
    }

    private void OnCollisionEnter(Collision other) 
    {
        if (other.gameObject.CompareTag("Car")||other.gameObject.CompareTag("Player"))
        {
            Die();
        }
    }
}
