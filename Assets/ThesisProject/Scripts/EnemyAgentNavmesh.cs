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


public class EnemyAgentNavmesh : Agent
{
    private EnemyPlayer enemyPlayer;
    private Transform enemyBase;
    private Transform targetResource;

    private GameManager gameManager;

    public List<ResourceData> resourcesTrackingList;
    private NavMeshAgent navmeshAgent;
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
        //PopulateTrackingList();
    }

    // Update is called once per frame
    void Update()
    {
        //A* Test method. Be sure to remove.
        AStarTest();
    }


    [ContextMenu("Update Distances")]
    public void TrackingListActivate()
    {
        StartCoroutine(GetTrackingList());
    }

    public IEnumerator GetTrackingList()
    {
        gameManager.ClearNullValues(); //Clear null values from gameManager.ResourceOhjects


        resourcesTrackingList = new List<ResourceData>();
        foreach (GameObject resourceObject in gameManager.ResourceObjects)
        {
            navmeshAgent.destination = resourceObject.transform.position;

            while(GetPathRemainingDistance() == -1)
            {
                yield return null;
            }

            float distanceFromPlayer = GetPathRemainingDistance();

            resourcesTrackingList.Add(new ResourceData()
            {
                resourceObject = resourceObject,
                //Save type
                type = resourceObject.GetComponent<ResourceObject>().resourceDropped.name,
                distanceFromPlayer = distanceFromPlayer,
                distanceFromBase = resourceObject.GetComponent<ResourceObject>().CalculateAStarDistance(enemyBase.position)

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

    /*
    public void UpdateTrackingDistances()
    {
        foreach (ResourceData resourceData in resourcesTrackingList)
        {
            Vector3 resourcePosition = resourceData.resourceObject.transform.position;

            
            // Vector3 distance method
            //resourceData.distanceFromPlayer = Vector3.Distance(resourcePosition, transform.position);
            //resourceData.distanceFromBase = Vector3.Distance(resourcePosition, enemyBase.position);
            
                       
            // A* Path distance method
            resourceData.distanceFromPlayer = enemyPlayer.CalculateAStarDistance(resourcePosition);
            resourceData.distanceFromBase = resourceData.resourceObject.GetComponent<ResourceObject>().CalculateAStarDistance(enemyBase.position);
        }
    }*/

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

    public void GatherResource()
    {
        StartCoroutine(enemyPlayer.GoToDestination(targetResource));

        // 1. Set target to resourceObject

        // 2. Detect Destination Reached

        // 3. Interact with item

        // 4. Wait until completion or interruption
    }

    public void ReturnToBase()
    {
        StartCoroutine(enemyPlayer.GoToDestination(enemyBase));

        // 1. Set target to base

        // 2. Detect Destination Reached

        // 3. Interact with item

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
        if (!player)
            player = GameObject.Find("Player").transform;
        else
            StartCoroutine(enemyPlayer.GoToDestination(player));
    }
}

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
    protected override void Start()
    {
        base.Start();
        navmeshAgent = GetComponent<NavMeshAgent>();


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
        yield return null;
    }


    /*public float CalculatePathDistance(Vector3 objectToCheck)
    {
        //1. Briefly set object to check as A* destination
        navmeshAgent.destination = objectToCheck;
        //2. Return distance as float
        return navmeshAgent.remainingDistance;
    }*/

}
