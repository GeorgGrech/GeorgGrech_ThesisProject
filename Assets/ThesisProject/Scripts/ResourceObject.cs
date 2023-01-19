using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


/// <summary>
/// Class for actual resource dropped by the Resource Object that will go into the player inventory then later deposited into the player base
/// </summary>
///
[System.Serializable]
//May be moved into ResourceObject class and saved into player inventory as a tuple in case this system does not work
public class Resource
{
    public string name; //name of the resource, i.e "Wood" or "Iron"

    public enum Type
    {
        Wood,
        Iron,
        LastItem //Used solely as marker to determine enum range. Used in EnemyAgent.
    }

    public Type resourceType;

    public int points; //points given to player when resource deposited in base
    public int inventorySpaceTaken; //space taken up by resource in player inventory
}


/// <summary>
/// Class for resource objects within world, i.e trees and iron mines
/// </summary>
///
public class ResourceObject : MonoBehaviour
{
    public int dropAmount;
    public float dropTime;
    public Resource resourceDropped;

    private bool playerInteracting; //To check if already being interacted with to avoid duplicate interaction or enemy and player interacting with same resource

    //private GameManager gameManager;
    private ItemSpawner itemSpawner;
    //private GameObject playerObject; //To be inhabited by player/enemy when entering collider to access functionality/variables

    //A* Components
    //private AIPath aiPath;
    //private Seeker seeker;
    public NavMeshAgent navmeshAgent;

    // Start is called before the first frame update
    void Start()
    {
        //aiPath = GetComponent<AIPath>();
        //seeker = GetComponent<Seeker>();
        navmeshAgent = GetComponentInChildren<NavMeshAgent>();

        itemSpawner = GameObject.Find("ItemSpawner").GetComponent<ItemSpawner>();
        itemSpawner.ResourceObjects.Add(gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered within range of " + gameObject.name);
            //playerObject = other.gameObject;
            //Obtain player script

        }
    }

    //Dropping the player resources in thier inventory
    public IEnumerator GiveResources(GameObject playerObject)
    {
        playerInteracting = true;

        ParentPlayer playerScript = null;

        if (playerObject.CompareTag("Player"))
        {
            playerScript = playerObject.GetComponent<HumanPlayer>();
        }
        //else condition to get enemy script
        else
        {
            playerScript = playerObject.GetComponent<EnemyPlayer>();
        }

        playerScript.playerInteracting = playerInteracting;

        int deposited; //Keep track of amount deposited to subtract from dropAmount in case of operation cancel
        //Add line for enemyPlayerScript
        for (deposited = 0; deposited < dropAmount; deposited++)
        {
            if (playerScript.IsInventoryFull(resourceDropped.inventorySpaceTaken))
            {
                Debug.Log("Inventory full. Cancelling operation.");
                break;
            }
            playerScript.PauseMovement(); //Pause player movement for duration of resource gathering

            yield return new WaitForSeconds(dropTime);

            playerScript.AddToInventory(resourceDropped);
            //playerScript.inventory.Add(resourceDropped);
            //playerScript.inventoryAmountFree -= resourceDropped.inventorySpaceTaken;

            Debug.Log("Item deposited");
        }

        playerScript.ResumeMovement(); //Resume player movement

        if (deposited == dropAmount)
        {
            Destroy(gameObject); //GameObject currently gets destroyed instantly upon full inventory due to skipping the for loop entirely
            Debug.Log("All items deposited. Destroying resource object.");
        }
        else
        {
            dropAmount -= deposited;
            Debug.Log("Depositing interrupted. "+dropAmount+" items left.");
        }

        playerInteracting = false;
        playerScript.playerInteracting = playerInteracting;
    }

    void InteractAction(GameObject playerObject)
    {
        Debug.Log("Resource interaction");
        if (!playerInteracting)
            StartCoroutine(GiveResources(playerObject));
    }

    //nav methods methods to be used by Agent determining distance between resource and base

    //copy of GetPathRemainingDistance in EnemyAgent
    public float GetPathRemainingDistance()
    {
        try
        {
            //EnableNavAgent(true);
            if (navmeshAgent.pathPending ||
                navmeshAgent.pathStatus == NavMeshPathStatus.PathInvalid ||
                navmeshAgent.path.corners.Length == 0)
                return -1f;

            float distance = 0.0f;
            for (int i = 0; i < navmeshAgent.path.corners.Length - 1; ++i)
            {
                distance += Vector3.Distance(navmeshAgent.path.corners[i], navmeshAgent.path.corners[i + 1]);
            }
            //EnableNavAgent(false);
            return distance;
        }
        catch //Error thrown due to destroyed navmesh on level reset
        {
            return -1;
        }
    }

    private void EnableNavAgent(bool enable)
    {
        navmeshAgent.enabled = enable;
    }
}
