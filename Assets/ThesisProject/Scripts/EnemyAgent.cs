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
/*
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
    private Transform targetResource;

    private GameManager gameManager;

    public List<ResourceData> resourcesTrackingList;



    private Seeker seeker;
    private AIPath aiPath;
    private AIDestinationSetter destinationSetter;
    //private List<GameObject> resourceObjects;

    //public Transform target;
    // Start is called before the first frame update
    void Start()
    {
        destinationSetter = GetComponent<AIDestinationSetter>();
        aiPath = GetComponent<AIPath>();
        seeker = GetComponent<Seeker>();

        //playerScript = gameObject.GetComponent<ParentPlayer>();
        enemyPlayer = gameObject.AddComponent<EnemyPlayer>();
        enemyBase = GameObject.FindGameObjectWithTag("EnemyBase").transform;

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        //PopulateTrackingList();
    }

    // Update is called once per frame
    void Update()
    {
        //A* Test method. Be sure to remove.
        //AStarTest();
    }

    [ContextMenu("Update Distances")]
    public void GetTrackingList()
    {
        StartCoroutine(TrackingListCoroutine());
    }
    
    public IEnumerator TrackingListCoroutine()
    {
        gameManager.ClearNullValues(); //Clear null values from gameManager.ResourceOhjects

        resourcesTrackingList = new List<ResourceData>();
        foreach (GameObject resourceObject in gameManager.ResourceObjects)
        {
            //1. Get distance player to resource
            aiPath.destination = resourceObject.transform.position;
            int counter = 0;
            while (aiPath.pathPending) //Wait until path is processed
            {
                Debug.Log("Counter: " + counter + " aiPath.pathPending: " + aiPath.pathPending);
                counter++;
                yield return null;
            }
            float distanceFromPlayer = aiPath.remainingDistance;
            Debug.Log("Counter: " + counter + " aiPath.pathPending: " + aiPath.pathPending);

            //2. Get distance resource to base


            Debug.Log("Resource position: " + resourceObject.transform.position);
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
        foreach(ResourceData resourceData in resourcesTrackingList)
        {
            Debug.Log("ResourceData added- Type: " + resourceData.type +
                " DistFromPlayer: " + resourceData.distanceFromPlayer +
                " DistFromBase: " + resourceData.distanceFromBase);
        }
    }
    
    
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

 
    public Transform DestinationFinder()
    {
        //Find destination
        return null;
    }*/

   /*public override void CollectObservations(VectorSensor sensor)
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
    //}


    /// <summary>
    /// Test variable and method to check A* functionality. Remove or comment later.
    /// </summary>
    /*
 Transform player;
    void AStarTest()
    {
        if(!player)
            player = GameObject.Find("Player").transform;
        else
            StartCoroutine(enemyPlayer.GoToDestination(player));
    }
}*/

/// <summary>
/// EnemyPlayer class, inheriting ParentPlayer script, to handle basic actions such as Interaction as well as inventory management.
/// Set as separate class due to inability to inherit both Agent and ParentPlayer. Not set as ParentPlayer component for easier management.
/// </summary>
/*
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
        seeker = GetComponent<Seeker>();
        
        //Set correctly later so aiPath.maxSpeed updates with base.movementSpeed
        aiPath.maxSpeed = base.movementSpeed;

    }

    public override void Interact()
    {
        base.Interact();
    }

    public IEnumerator GoToDestination(Transform destination)
    {
        //destinationSetter.target = destination;
        aiPath.destination = destination.position;
        //ai.SearchPath();
        while (!aiPath.reachedDestination)
        {
            yield return null;
        }

        Interact();

        //Yield wait 
    }

    public float CalculateAStarDistance(Vector3 objectToCheck)
    {
        //1. Briefly set object to check as A* destination
        aiPath.destination = objectToCheck;
        //2. Return distance as float
        return aiPath.remainingDistance;
    }

}*/

