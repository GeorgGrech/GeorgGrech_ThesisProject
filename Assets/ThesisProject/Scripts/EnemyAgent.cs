using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
//using Pathfinding;
using UnityEngine.AI;
using UnityEditor;
using Unity.Barracuda;

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

    public float takenAmount;
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
    //public int EndEpisodeScore = 2000;
    public bool endTrainingWithMaxEpisodes;
    public int MaxEpisodes = 0;
    public float resourceGatherRewardPriority = 0.5f;

    public float defaultRewardWeight = .1f; //All rewards to be multiplied by this value to remain mostly in optimal [-1,1] range

    //Brains
    [SerializeField] private NNModel[] brains;

    //private List<GameObject> resourceObjects;

    //public Transform target;

    private int validCounter; //Counter used when checking if action hasn't gotten stuck
    private bool isTracking; //used to check if agent is atteempting tracking. If taking too long and still in this state, break

    // Start is called before the first frame update
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

        StartCoroutine(DelayedStart()); //Delayed GetTrackingList
        //StartCoroutine(GetTrackingList());
        //PopulateTrackingList();

        firstTimeStart = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Cumulative Reward: "+GetCumulativeReward());
    }


    [ContextMenu("Update Distances")]
    public void TrackingListActivate()
    {
        StartCoroutine(GetTrackingList());
    }

    private IEnumerator DelayedStart()
    {
        yield return new WaitForSecondsRealtime(.5f); //Slightly delayed GetTrackingList to let all vital start methods complete
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

            isTracking = true;
            Coroutine trackTimer = StartCoroutine(TrackingListForceStopTimer(8));
            //Debug.Log("GetTrackingList Trace: 3");
            foreach (Collider collider in hits)
            {
                bool validNav = true;

                //1. Save distance from player to resource
                float distanceFromPlayer;

                if (collider != null) //Don't run if collider turns out to be null. (Rare error)
                {
                    navmeshAgent.destination = collider.transform.position; //Assign resource as agent target
                    while (GetPathRemainingDistance() == -1 && collider!=null) //Keep trying until value is valid
                    {
                        yield return null;
                    }
                }

                //Debug.Log("GetTrackingList Trace: 4");

                //2. Save distance from resource to base
                if (collider != null) //Check that collider still exists. Fixes error causewd when changing level for agent
                {

                    GameObject resourceObject = collider.transform.parent.gameObject;
                    ResourceObject objectScript = resourceObject.GetComponent<ResourceObject>();
                    objectScript.navmeshAgent.destination = enemyBase.position; //Redundant. Try setting it once in ResourceObject.cs
                    //Debug.Log("GetTrackingList Trace - Resource name:" + collider.name + " Resource position:" + collider.transform.position);
                    Coroutine validTimer = StartCoroutine(StartValidTimer(1));
                    while (objectScript.GetPathRemainingDistance() == -1) //Keep trying until value is valid
                    {
                        yield return null;
                        if (validCounter == 1) //If still invalid after some time, break
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
                        //Debug.Log("Valid nav, using nav distance");
                    }
                    else
                    {
                        distanceFromPlayer = Vector3.Distance(resourceObject.transform.position, enemyBase.position); //Invalid nav, using basic Vector3 distance
                        Debug.Log("Invalid nav, using Vector3.distance");
                    }
                    //Debug.Log("GetTrackingList Trace: 5");
                    ResourceObject resourceObjectScript = resourceObject.GetComponent<ResourceObject>();
                    resourcesTrackingList.Add(new ResourceData()
                    {
                        resourceObject = resourceObject,
                        //Save type
                        type = resourceObjectScript.resourceDropped.resourceType,
                        distanceFromPlayer = distanceFromPlayer,
                        distanceFromBase = distanceFromBase,

                        takenAmount = resourceObjectScript.totalDeposited / (float)resourceObjectScript.dropAmount

                    });
                    resourceCounter++;
                    //Debug.Log("Scanned " + resourceCounter + " / " + resourceAmount);
                    //Debug.Log("GetTrackingList Trace: 6");
                }
                else
                {
                    Debug.Log("Null collider. Breaking...");
                    break; //break for loop and avoid checking other colliders
                }


            }
            enemyPlayer.ResumeMovement(); //Resume movement again
            StopCoroutine(trackTimer);
            isTracking = false;
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

    private IEnumerator StartValidTimer(float time)
    {
        Debug.Log("ValidTimer - Started");
        validCounter = 0;
        while (validCounter < time)
        {
            yield return new WaitForSecondsRealtime(1);
            validCounter++;
        }
    }

    private IEnumerator TrackingListForceStopTimer(float time)
    {
        Debug.Log("TrackingListForceStopTimer - Started");
        int counter = 0;
        while (counter < time)
        {
            yield return new WaitForSecondsRealtime(1);
            counter++;
        }

        if (isTracking)
        {
            Debug.Log("TrackingListForceStopTimer - Breaking");
            isTracking = false;
            RequestDecision(); //If enough time has passed and GetTracking hasn't concluded, just jump to decision
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
        //Debug.Log("GatherResources Error Track 1");
        //Penalize based on distances
        AddReward(-(distToPlayer * distancePenalisePriority * defaultRewardWeight));
        AddReward(-(distToBase * distancePenalisePriority * defaultRewardWeight));

        Debug.Log("Distance Penalty: " + (-(distToPlayer * distancePenalisePriority * defaultRewardWeight)));
        //Debug.Log("GatherResources Error Track 2");
        // 1. Set target to resourceObject
        StartCoroutine(enemyPlayer.GoToDestination(targetResource));

        //Debug.Log("GatherResources Error Track 3");
        // 2. Detect Destination Reached
        bool validPos = true;
        Coroutine validTimer = null;


        while (!enemyPlayer.destinationReached)
        {
            if (targetResource != null) //Circumvents rare error where player destroys resource wanted by enemy
            {
                if((Vector3.Distance(transform.position,targetResource.position) < 2.5)&& validTimer == null) //If in vicinity, start coroutine
                {
                    validTimer = StartCoroutine(StartValidTimer(2));
                }
                yield return null;
                if (validCounter == 2) //If still invalid after some time, break. 
                {
                    validPos = false;
                    break;
                }
            }
            else
            {
                validPos = false; //Set valid pos as false to request decision again
                break;
            }
        }
        if(validTimer!=null) StopCoroutine(validTimer);

        if (!validPos)
        {
            Debug.Log("(ValidTimer break) Destination not reached successfully. Interrupting action and and rescanning.");
            //RequestDecision();
            StartCoroutine(GetTrackingList()); //If not reached succeessfully rescan to try another nearby resource
        }

        else //If destination reached successfully
        {
            // 3. Interact with item
            //Included in GoToDestination
            //yield return new WaitForSecondsRealtime(.1f); //Buffer to ensure that interact has been activated

            // 4. Wait until completion or interruption
            Debug.Log("GatherResources Error Track 4");
            validTimer = null;
            //yield return new WaitForSecondsRealtime(.2f);
            if(validTimer == null)
            {
                validTimer = StartCoroutine(StartValidTimer(2));
            }

            if (enemyPlayer.interactableObject) //If interactableObject is null, for whatever reason, break operation and rescan
            {
                ResourceObject resourceObject = enemyPlayer.interactableObject.GetComponent<ResourceObject>(); //Get ResourcObject to chech if interaction successful
                while (enemyPlayer.playerInteracting)
                {
                    yield return null;
                    if(resourceObject != null)
                    {
                        if (validCounter == 2 && !resourceObject.playerInteracting)
                        {
                            Debug.Log("(ValidTimer break) Interaction not successful. Breaking and rescanning.");
                            break; //If agent gets stuck on this stage, skip immediately to rescan
                        }
                    }
                }
            }

            else
            {
                Debug.Log("interactableObject is null. Rescanning list.");
            }
            //if (validTimer != null) StopCoroutine(validTimer);
            Debug.Log("Action completed. Interaction successful or interrupted.");
            Debug.Log("GatherResources Error Track 5");

            StartCoroutine(GetTrackingList()); //Rescan list to allow next decision
        }
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
        Debug.Log("Training currently on episode: "+(CompletedEpisodes+1)+" / "+MaxEpisodes);
        if (!firstTimeStart) //Double check so that this function doesn't run on start
        {
            ResetLevelAndAgent();
            
            StopAllCoroutines(); //Stop any actions
            StartCoroutine(DelayedStart()); //Start actions again with delay
        }

        if (endTrainingWithMaxEpisodes)
        {
            if (MaxEpisodes != 0 && CompletedEpisodes >= MaxEpisodes) //For every x amount of completed episodes, quit. User will then manually interrupt and save NN file before restarting for next session.
            {
                EditorApplication.ExitPlaymode();
            }
        }
    }

    public void ResetLevelAndAgent()
    {
        itemSpawner.ResetLevel(this.gameObject,false);
        enemyPlayer.ResetInventory();
        //enemyPlayer.ResumeMovement();
        enemyPlayer.score = 0;
    }

    public void Penalty(float penalty)
    {
        AddReward(-penalty*defaultRewardWeight);
        Debug.Log("Penalty weight: " + -penalty * defaultRewardWeight);
    }


    #region Agent methods
    public override void CollectObservations(VectorSensor sensor)
    {
        try
        {
            if (resourcesTrackingList != null && resourcesTrackingList.Count > 0)
            {
                foreach (ResourceData resourceData in resourcesTrackingList)
                {
                    sensor.AddObservation(resourceData.distanceFromBase);
                    sensor.AddObservation(resourceData.distanceFromPlayer);
                    sensor.AddOneHotObservation((int)resourceData.type, resourceData.numOfTypes);
                    sensor.AddObservation(resourceData.takenAmount);
                }
                //sensor.AddObservation(this.transform.localPosition); //No need to observe current position since distances are already measured
            }

            sensor.AddObservation(enemyPlayer.inventoryAmountFree); //Keep track of inventory
        }
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

        catch
        {
            Debug.Log("Exception caught in observations");
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Debug.Log("Cumulative Reward: "+GetCumulativeReward());

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

    [ContextMenu("Switch to a random brain")]
    void SwitchRandomBrain()
    {
        NNModel brain = brains[Random.Range(0, brains.Length)];
        SetModel("ResourceAgent",brain);
        Debug.Log("Switched brain to: " + brain.name);
    }


    public int GetScore()
    {
        return enemyPlayer.score;
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

    StatsRecorder statsRecorder;

    protected override void Start()
    {
        base.Start();
        statsRecorder = Academy.Instance.StatsRecorder;

        navmeshAgent = GetComponent<NavMeshAgent>();

        navmeshAgent.speed = movementSpeed;

        enemyAgent = gameObject.GetComponent<EnemyAgent>(); //Access enemyAgent for point rewards


    }

    [ContextMenu("Interact")]
    public override void Interact()
    {
        Debug.Log("Error track - Interact");
        base.Interact();
    }

    public override void AddScore(int index)
    {
        base.AddScore(index);
        enemyAgent.AddReward(inventory[index].points*enemyAgent.defaultRewardWeight); //Add points of deposited items as reward
        statsRecorder.Add("Score", enemyAgent.GetScore()); //Display score on TensorBoard
        Debug.Log("Base deposit reward: " + inventory[index].points * enemyAgent.defaultRewardWeight);

        /*
        if(score >= enemyAgent.EndEpisodeScore && enemyAgent.itemSpawner.agentTrainingLevel)
        {
            enemyAgent.EndEpisode();
        }
        */
    }


    public override void PauseMovement()
    {
        base.PauseMovement();
        if(navmeshAgent) navmeshAgent.speed = movementSpeed;
    }

    public override void ResumeMovement()
    {
        base.ResumeMovement();
        if (navmeshAgent) navmeshAgent.speed = movementSpeed;
    }

    public override void InventoryRemainderPenalize()
    {
        enemyAgent.Penalty(inventoryAmountFree);
    }

    public override void FullInventoryPenalize()
    {
        enemyAgent.Penalty(maxInventorySize-inventoryAmountFree);
    }

    public override void AddToInventory(Resource resourceDropped)
    {
        base.AddToInventory(resourceDropped);

        //This line of code rewards the agent for simply gathering a resource, though the award is less
        enemyAgent.AddReward(resourceDropped.points * enemyAgent.resourceGatherRewardPriority * enemyAgent.defaultRewardWeight);
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
