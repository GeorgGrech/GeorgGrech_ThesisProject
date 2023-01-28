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
    //public string type; //type needs to be set to some type of number for compatibility with ML
    public Resource.Type type; //Enum type, Wood or Iron
    public int numOfTypes = (int)Resource.Type.LastItem;
    public float distanceFromPlayer;
    public float distanceFromBase;
}


public class EnemyAgent : Agent
{
    private bool firstTimeStart = true; //Used so that OnEpisodeBegin isn't called on start

    private EnemyPlayer enemyPlayer;
    private Transform enemyBase;
    //private Transform targetResource;

    //private GameManager gameManager;
    public ItemSpawner itemSpawner;

    public List<ResourceData> resourcesTrackingList; //Scanned list including all distances to enemy and base
    [SerializeField] private int initialScanRange; 

    private NavMeshAgent navmeshAgent;
    public NavMeshSurface navmeshSurface; //Terrain navmesh for rebaking navmesh when necessary

    [Tooltip("Max size of Discrete Branch. Make sure it is equal to size of Branch 0 in Behaviour Parameters. Will be used to mask out unused actions when making a decision.")]
    [SerializeField] private int maxBranchSize;

    [SerializeField] private float distancePenalisePriority = 0.5f;
    public int EndEpisodeScore = 2000;
    public float resourceGatherRewardPriority = 0.5f;

    //private List<GameObject> resourceObjects;

    //public Transform target;
    // Start is called before the first frame update

    private int validCounter;

    void Start()
    {
        //playerScript = gameObject.GetComponent<ParentPlayer>();
        enemyPlayer = gameObject.AddComponent<EnemyPlayer>();
        enemyBase = GameObject.FindGameObjectWithTag("EnemyBase").transform;

        //gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        itemSpawner = GameObject.Find("ItemSpawner").GetComponent<ItemSpawner>();

        navmeshAgent = GetComponent<NavMeshAgent>();

        navmeshSurface = GameObject.Find("Terrain").GetComponent<NavMeshSurface>();

        resourcesTrackingList = new List<ResourceData>();

        StartCoroutine(GetTrackingList());
        //PopulateTrackingList();

        firstTimeStart = false;
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

        itemSpawner.ClearNullValues(); //Clear null values from gameManager.ResourceOhjects

        enemyPlayer.PauseMovement(); //Pause movement to not start moving to resources to scan

        while(itemSpawner.ResourceObjects == null || itemSpawner.ResourceObjects.Count == 0) //Wait until resources are loaded
        {
            yield return null;
        }


        Collider[] hits = new Collider[0];
        int scanRange = initialScanRange;

        while (hits.Length == 0 && scanRange < 100) //Safety precaution: Stop scanning if nothing found up to range = 100
        {
            hits = Physics.OverlapSphere(transform.position, scanRange, 1<<6); //Get resources within range, and only in "Resource" layer
            scanRange += 10; //if no objects detected in range, increase range
        }

        if (hits.Length > 0) //Safety precaution, if nothing found after scanning, don't do anything
        {
            int resourceCounter = 0; //Used to for debugging by tracking progress
            //int resourceAmount = hits.Length;

            resourcesTrackingList = new List<ResourceData>();

            Debug.Log("GetTrackingList Trace: 3");
            foreach (Collider collider in hits)
            {
                bool validNav = true;
                //1. Save distance from player to resource
                navmeshAgent.destination = collider.transform.position; //Assign resource as agent target
                while (GetPathRemainingDistance() == -1) //Keep trying until value is valid
                {
                    yield return null;
                }
                float distanceFromPlayer; 

                Debug.Log("GetTrackingList Trace: 4");

                //2. Save distance from resource to base
                GameObject resourceObject = collider.transform.parent.gameObject;
                ResourceObject objectScript = resourceObject.GetComponent<ResourceObject>();
                objectScript.navmeshAgent.destination = enemyBase.position; //Redundant. Try setting it once in ResourceObject.cs
                Debug.Log("GetTrackingList Trace - Resource name:"+collider.name +" Resource position:"+collider.transform.position);
                Coroutine validTimer = StartCoroutine(StartValidTimer());
                while (objectScript.GetPathRemainingDistance() == -1) //Keep trying until value is valid
                {
                    yield return null;
                    if (validCounter == 2) //If still invalid after some time, break
                    {
                        validNav = false;
                        break;
                    }
                }
                StopCoroutine(validTimer);
                float distanceFromBase = 0;
                if (validNav)
                {
                    distanceFromPlayer = objectScript.GetPathRemainingDistance(); //If  valid nav, use nav distance
                    Debug.Log("Valid nav, using nav distance");
                }
                else
                {
                    distanceFromPlayer = Vector3.Distance(resourceObject.transform.position, enemyBase.position); //Invalid nav, using basic Vector3 distance
                    Debug.Log("Invalid nav, using Vector3.distance");
                }
                Debug.Log("GetTrackingList Trace: 5");

                resourcesTrackingList.Add(new ResourceData()
                {
                    resourceObject = resourceObject,
                    //Save type
                    type = resourceObject.GetComponent<ResourceObject>().resourceDropped.resourceType,
                    distanceFromPlayer = distanceFromPlayer,
                    distanceFromBase = distanceFromBase

                });
                resourceCounter++;
                //Debug.Log("Scanned " + resourceCounter + " / " + resourceAmount);
                Debug.Log("GetTrackingList Trace: 6");
            }
            enemyPlayer.ResumeMovement(); //Resume movement again


            RequestDecision(); //After getting list, request decision

            /*
            //Redundant loop just for checking due to inability to see resourcesTrackingList in inspector
            foreach (ResourceData resourceData in resourcesTrackingList)
            {
                Debug.Log("ResourceData added- Type: " + resourceData.type +
                    " DistFromPlayer: " + resourceData.distanceFromPlayer +
                    " DistFromBase: " + resourceData.distanceFromBase);
            }*/
        }   
    }

    private IEnumerator StartValidTimer()
    {
        validCounter = 0;
        while (validCounter < 2)
        {
            yield return new WaitForSecondsRealtime(1);
            validCounter++;
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
    public IEnumerator GatherResource(Transform targetResource, float distToPlayer, float distToBase)
    {
        Debug.Log("Error track - Start of GatherResource");
        //Penalize based on distances
        AddReward(-(distToPlayer * distancePenalisePriority));
        AddReward(-(distToBase * distancePenalisePriority));

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
        Debug.Log("Error track - End of GatherResource");

        StartCoroutine(GetTrackingList()); //Rescan list to allow next decision
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

        StartCoroutine(GetTrackingList()); //Rescan list to allow next decision
    }


    public override void OnEpisodeBegin()
    {
        if (!firstTimeStart) //Double check so that this function doesn't run on start
        {
            itemSpawner.ResetLevel(this.gameObject);
            enemyPlayer.ResetInventory();
            enemyPlayer.score = 0;
        }
    }

    public void InventoryRemainderPenalize(float penalty)
    {
        AddReward(-penalty);
    }

    #region Agent methods
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

        if (resourcesTrackingList != null && resourcesTrackingList.Count > 0)
        {
            foreach (ResourceData resourceData in resourcesTrackingList)
            {
                sensor.AddObservation(resourceData.distanceFromBase);
                sensor.AddObservation(resourceData.distanceFromPlayer);
                sensor.AddOneHotObservation((int)resourceData.type, resourceData.numOfTypes);
            }
            //sensor.AddObservation(this.transform.localPosition); //No need to observe current position since distances are already measured
        }
        sensor.AddObservation(enemyPlayer.inventoryAmountFree); //Keep track of inventory
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        
        int actionIndex = actionBuffers.DiscreteActions[0];

        Debug.Log("Action Index: " + actionIndex);

        if (actionIndex == 0) //0 means returns to base 
        {
            Debug.Log("Return to base");
            StartCoroutine(ReturnToBase());
        }


        else //1 or greater, choose to gather a resource
        {
            Debug.Log("Going to gather resource " + (actionIndex - 1) + ": " + resourcesTrackingList[actionIndex - 1].type); 
            StartCoroutine(GatherResource(resourcesTrackingList[actionIndex - 1].resourceObject.transform,
                resourcesTrackingList[actionIndex - 1].distanceFromPlayer,
                resourcesTrackingList[actionIndex - 1].distanceFromBase));
        }
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        //int used = 0;
        for (int i = 0; i < maxBranchSize; i++)
        {
            if(i >= resourcesTrackingList.Count+1) //Disable out of range unused branches
            {
                actionMask.SetActionEnabled(0, i, false);
            }
            //else used = i;
        }
        //Debug.Log("Branches used: " + (used + 1 )+" Total actions:" + (resourcesTrackingList.Count + 1));
    }

    #endregion

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
        Transform target = itemSpawner.ResourceObjects[Random.Range(0, itemSpawner.ResourceObjects.Count)].transform;
        Debug.Log("Test target: " + target.name+" at position: "+target.position);
        StartCoroutine(GatherResource(target,0,0));
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
    private EnemyAgent enemyAgent;

    protected override void Start()
    {
        base.Start();
        navmeshAgent = GetComponent<NavMeshAgent>();

        navmeshAgent.speed = movementSpeed;

        enemyAgent = gameObject.GetComponent<EnemyAgent>(); //Access enemyAgent for point rewards
    }

    public override void Interact()
    {
        Debug.Log("Error track - Interact");
        base.Interact();
    }

    public override void AddScore(int index)
    {
        base.AddScore(index);
        enemyAgent.AddReward(inventory[index].points); //Add points of deposited items as reward

        if(score >= enemyAgent.EndEpisodeScore && enemyAgent.itemSpawner.agentTrainingLevel)
        {
            enemyAgent.EndEpisode();
        }
    }

    public override void InventoryRemainderPenalize()
    {
        enemyAgent.InventoryRemainderPenalize(maxInventorySize-inventoryAmountFree);
    }

    public override void AddToInventory(Resource resourceDropped)
    {
        base.AddToInventory(resourceDropped);

        //This line of code rewards the agent for simply gathering a resource, though the award is less
        //enemyAgent.AddReward(resourceDropped.points * enemyAgent.resourceGatherRewardPriority);
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
        Debug.Log("Error track - End of GoToDestination");
        Interact();
    }

}
