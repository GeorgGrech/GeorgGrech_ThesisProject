using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
//using Pathfinding;
using UnityEngine.AI;

/// <summary>
/// EnemyAgent class, inheritng base ML Agent class, to handle all decisions and direct actions, including ones from ParentPlayer class
/// that may be grouped together as one action (i.e "Go to base" that includes setting target to base and interacting)
/// </summary>

public class ResourceData
{
    public GameObject resourceObject;
    public string type;
    public float distanceFromPlayer;
    public float distanceFromBase;
}


public class EnemyAgent : Agent
{
    private EnemyPlayer enemyPlayer;
    private Transform enemyBase;
    //private Transform targetResource;

    private GameManager gameManager;

    public List<ResourceData> resourcesTrackingList;

    private NavMeshAgent navmeshAgent;
    public NavMeshSurface navmeshSurface; //Terrain navmesh for rebaking navmesh when necessary

    //private List<GameObject> resourceObjects;

    //public Transform target;
    // Start is called before the first frame update
    void Start()
    {
        //playerScript = gameObject.GetComponent<ParentPlayer>();
        enemyPlayer = gameObject.AddComponent<EnemyPlayer>();
        enemyBase = GameObject.FindGameObjectWithTag("EnemyBase").transform;

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        navmeshAgent = GetComponent<NavMeshAgent>();

        navmeshSurface = GameObject.Find("Terrain").GetComponent<NavMeshSurface>();
        //PopulateTrackingList();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    [ContextMenu("Update Distances")]
    public void TrackingListActivate()
    {
        StartCoroutine(GetTrackingList());
    }

    public IEnumerator GetTrackingList()
    {
        navmeshSurface.BuildNavMesh(); //Rebuild navmesh

        gameManager.ClearNullValues(); //Clear null values from gameManager.ResourceOhjects


        resourcesTrackingList = new List<ResourceData>();
        foreach (GameObject resourceObject in gameManager.ResourceObjects)
        {
            //1. Save distance from player to resource
            navmeshAgent.destination = resourceObject.transform.position; //Assign resource as agent target
            while(GetPathRemainingDistance() == -1) //Keep trying until value is valid
            {
                yield return null;
            }
            float distanceFromPlayer = GetPathRemainingDistance();

            //2. Save distance from resource to base
            ResourceObject objectScript = resourceObject.GetComponent<ResourceObject>();
            resourceObject.GetComponent<NavMeshAgent>().destination = enemyBase.position; //Redundant. Try setting it once in ResourceObject.cs
            while (objectScript.GetPathRemainingDistance() == -1) //Keep trying until value is valid
            {
                yield return null;
            }
            float distanceFromBase = objectScript.GetPathRemainingDistance();

            resourcesTrackingList.Add(new ResourceData()
            {
                resourceObject = resourceObject,
                //Save type
                type = resourceObject.GetComponent<ResourceObject>().resourceDropped.name,
                distanceFromPlayer = distanceFromPlayer,
                distanceFromBase = distanceFromBase

            });
        }

        //Redundant loop just for checking due to inability to see resourcesTrackingList in inspector
        foreach (ResourceData resourceData in resourcesTrackingList)
        {
            Debug.Log("ResourceData added- Type: " + resourceData.type +
                " DistFromPlayer: " + resourceData.distanceFromPlayer +
                " DistFromBase: " + resourceData.distanceFromBase);
        }
    }

    public float GetPathRemainingDistance()
    {
        if (navmeshAgent.pathPending ||
            navmeshAgent.pathStatus == NavMeshPathStatus.PathInvalid ||
            navmeshAgent.path.corners.Length == 0)
            return -1f;

        float distance = 0.0f;
        for (int i = 0; i < navmeshAgent.path.corners.Length - 1; ++i)
        {
            distance += Vector3.Distance(navmeshAgent.path.corners[i], navmeshAgent.path.corners[i + 1]);
        }

        return distance;
    }

    /// <summary>
    /// Action taken by agent to gather a resource. Goes to resource, interacts, and concludes once gathering is finished or interrupted. targetResource is a parameter that may be chosen via a method that handles selection. Penalization may occur (TBD).
    /// </summary>
    /// <param name="targetResource"></param>
    /// <returns></returns>
    public IEnumerator GatherResource(Transform targetResource)
    {
        // 1. Set target to resourceObject
        StartCoroutine(enemyPlayer.GoToDestination(targetResource));

        // 2. Detect Destination Reached
        while (!enemyPlayer.destinationReached)
        {
            yield return null;
        }

        // 3. Interact with item
        //Included in GoToDestination

        // 4. Wait until completion or interruption
        while (enemyPlayer.playerInteracting)
        {
            yield return null;
        }
        Debug.Log("Action completed. Interaction successful or interrupted.");
    }


    /// <summary>
    /// Action taken by agent to return to base and deposit resources. Goes to base, interacts, and concludes once all items deposited. Action will reward based on points recieved from resource. Penalization will occur based on amount of inventory still unoccupied when depositing. 
    /// </summary>
    public IEnumerator ReturnToBase()
    {
        // 1. Set target to base
        StartCoroutine(enemyPlayer.GoToDestination(enemyBase));

        // 2. Detect Destination Reached
        while (!enemyPlayer.destinationReached)
        {
            yield return null;
        }

        // 3. Interact with item
        //Included in GoToDestination

        // 4. Wait until completion
        while (enemyPlayer.playerInteracting)
        {
            yield return null;
        }
        Debug.Log("Action completed. Items deposited at base.");
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

        foreach(ResourceData resourceData in resourcesTrackingList)
        {
            sensor.AddObservation(resourceData.distanceFromBase);
            sensor.AddObservation(resourceData.distanceFromPlayer);
            //sensor.AddObservation(resourceData.type); 
        }
        sensor.AddObservation(this.transform.localPosition);

    }

#region testing methods
    /// <summary>
    /// Test variable and method to check seeking and interact functionality. Remove or comment later.
    /// </summary>

    Transform player;
    [ContextMenu("Find and Interact with Player")]
    void GoToPlayer()
    {
        if (!player)
            player = GameObject.Find("Player").transform;

        StartCoroutine(enemyPlayer.GoToDestination(player));
    }

    [ContextMenu("Gather Random Resource")]
    void GatherRandomResource()
    {
        Transform target = gameManager.ResourceObjects[Random.Range(0, gameManager.ResourceObjects.Count)].transform;
        StartCoroutine(GatherResource(target));
    }

    [ContextMenu("Return to Base and Deposit")]
    void ReturnToBaseMethod()
    {
        StartCoroutine(ReturnToBase());
    }
}
#endregion

/// <summary>
/// EnemyPlayer class, inheriting ParentPlayer script, to handle basic actions such as Interaction as well as inventory management.
/// Set as separate class due to inability to inherit both Agent and ParentPlayer. Not set as ParentPlayer component for easier management.
/// </summary>

public class EnemyPlayer : ParentPlayer
{
    /*
    //A* variables
    private Seeker seeker;
    private AIPath aiPath;
    private AIDestinationSetter destinationSetter;
    */

    private NavMeshAgent navmeshAgent;
    public bool destinationReached = false;
    protected override void Start()
    {
        base.Start();
        navmeshAgent = GetComponent<NavMeshAgent>();

        navmeshAgent.speed = movementSpeed;

    }

    public override void Interact()
    {
        base.Interact();
    }

    public IEnumerator GoToDestination(Transform destination)
    {
        /*
        //destinationSetter.target = destination;
        aiPath.destination = destination.position;
        //ai.SearchPath();
        while (!aiPath.reachedDestination)
        {
            yield return null;
        }

        Interact();
        */

        navmeshAgent.destination = destination.position;

        destinationReached = false;

        while (!destinationReached)
        {
            if (!navmeshAgent.pathPending)
            {
                if (navmeshAgent.remainingDistance <= navmeshAgent.stoppingDistance)
                {
                    if (!navmeshAgent.hasPath || navmeshAgent.velocity.sqrMagnitude == 0f)
                    {
                        destinationReached = true;
                    }
                    else yield return null;
                }
                else yield return null;
            }
            else yield return null;
        }

        Interact();
    }

}
