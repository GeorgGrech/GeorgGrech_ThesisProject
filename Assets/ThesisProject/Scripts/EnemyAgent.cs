using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Pathfinding;

/// <summary>
/// EnemyAgent class, inheritng base ML Agent class, to handle all decisions and direct actions, including ones from ParentPlayer class
/// that may be grouped together as one action (i.e "Go to base" that includes setting target to base and interacting)
/// </summary>
public class EnemyAgent : Agent
{
    public EnemyPlayer enemyPlayer;
    public Transform enemyBase;
    public Transform targetResource;
    //public Transform target;
    // Start is called before the first frame update
    void Start()
    {
        //playerScript = gameObject.GetComponent<ParentPlayer>();
        enemyPlayer = gameObject.AddComponent<EnemyPlayer>();
        enemyBase = GameObject.FindGameObjectWithTag("EnemyBase").transform;
    }

    // Update is called once per frame
    void Update()
    {
        //A* Test method. Be sure to remove.
        AStarTest();
    }

    public void GatherResource()
    {
        // 1. Set target to resourceObject
        enemyPlayer.GoToDestination(targetResource);
        // 2. Detect Destination Reached

        // 3. Interact with item
        enemyPlayer.Interact();

        // 4. Wait until completion or interruption
    }

    public void ReturnToBase()
    {
        // 1. Set target to base
        enemyPlayer.GoToDestination(enemyBase);
        // 2. Detect Destination Reached

        // 3. Interact with item
        enemyPlayer.Interact();

        // 4. Wait until completion
    }

    /*
    public Transform DestinationFinder()
    {
        //Find destination
        return null;
    }*/

    public override void CollectObservations(VectorSensor sensor)
    {
        /* Observations to collect
         * -(All resources) Distance from enemy
         * -(All resources) Distance from base
         * -(ALl resources) Type of resource
         */

        /*
        sensor.AddObservation(targetResource.localPosition); // Position of target resource
        sensor.AddObservation(enemyBase.transform.localPosition); // Position of enemy base
        sensor.AddObservation(this.transform.localPosition); // Position of enemy
        */
    }


    /// <summary>
    /// Test variable and method to check A* functionality. Remove or comment later.
    /// </summary>

    Transform player;
    void AStarTest()
    {
        if(!player)
            player = GameObject.Find("Player").transform;
        else
            enemyPlayer.GoToDestination(player);
    }
}

/// <summary>
/// EnemyPlayer class, inheriting ParentPlayer script, to handle basic actions such as Interaction as well as inventory management.
/// Set as separate class due to inability to inherit both Agent and ParentPlayer. Not set as ParentPlayer component for easier management.
/// </summary>

public class EnemyPlayer : ParentPlayer
{
    //A* variables
    private Seeker seeker;
    private AIPath aiPath;
    private AIDestinationSetter destinationSetter;

    protected override void Start()
    {
        base.Start();
        
        destinationSetter = GetComponent<AIDestinationSetter>();
        aiPath = GetComponent<AIPath>();

        //Set correctly later so aiPath.maxSpeed updates with base.movementSpeed
        aiPath.maxSpeed = base.movementSpeed;

    }

    public override void Interact()
    {
        base.Interact();
    }

    public void GoToDestination(Transform destination)
    {
        destinationSetter.target = destination;
    }
}
