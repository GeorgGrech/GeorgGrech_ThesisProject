using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

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
        Gold,
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
    public int totalDeposited;
    public int dropAmount;
    public float dropTime;
    public Resource resourceDropped;

    public bool playerInteracting; //To check if already being interacted with to avoid duplicate interaction or enemy and player interacting with same resource
    private GameObject playerUsing;

    //private GameManager gameManager;
    private ItemSpawner itemSpawner;
    //private GameObject playerObject; //To be inhabited by player/enemy when entering collider to access functionality/variables

    //A* Components
    //private AIPath aiPath;
    //private Seeker seeker;
    public NavMeshAgent navmeshAgent;

    //private Transform enemyBase;
    //private int validCounter;

    // Start is called before the first frame update
    void Start()
    {
        //aiPath = GetComponent<AIPath>();
        //seeker = GetComponent<Seeker>();
        //navmeshAgent = GetComponentInChildren<NavMeshAgent>();
        //navmeshAgent = Get<NavMeshAgent>();

        itemSpawner = GameObject.Find("ItemSpawner").GetComponent<ItemSpawner>();
        itemSpawner.ResourceObjects.Add(gameObject);

        //enemyBase = GameObject.FindGameObjectWithTag("EnemyBase").GetComponent<Transform>();

        //StartCoroutine(ValidNavCheck());
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

        if (other.CompareTag("Obstacle"))
        {
            itemSpawner = GameObject.Find("ItemSpawner").GetComponent<ItemSpawner>(); //Set here due to obstacle destruction occuring before itemSpawner is found
            Debug.Log("Collision with Obstacle. Destroying...");
            Destroy(gameObject);
        }
    }

    //Dropping the player resources in thier inventory
    public IEnumerator GiveResources(GameObject playerObject)
    {
        Transform resourceCanvas = transform.Find("ResourceCanvas");
        resourceCanvas.gameObject.SetActive(true);
        resourceCanvas.GetComponent<Canvas>().worldCamera = Camera.main; //Setting event camera just for safety

        Slider progressSlider = resourceCanvas.GetComponentInChildren<Slider>();
        

        Debug.Log("Error track - Start of GiveResources");
        playerInteracting = true;

        ParentPlayer playerScript = null;

        bool isPlayer;

        if (playerObject.CompareTag("Player"))
        {
            playerScript = playerObject.GetComponent<HumanPlayer>();
            isPlayer = true;
        }
        //else condition to get enemy script
        else
        {
            playerScript = playerObject.GetComponent<EnemyPlayer>();
            isPlayer = false;
        }

        playerScript.playerInteracting = playerInteracting;
        playerUsing = playerObject;

        int deposited; //Keep track of amount deposited to subtract from dropAmount in case of operation cancel
        //Add line for enemyPlayerScript
        for (deposited = totalDeposited; deposited < dropAmount; deposited++)
        {
            progressSlider.value = (float)deposited / dropAmount;
            if (playerScript.IsInventoryFull(resourceDropped.inventorySpaceTaken))
            {
                /*if (isEnemy && deposited == 0) //Penalize if inventory already full on first interaction
                {
                    playerScript.FullInventoryPenalize();
                }*/
                Debug.Log("Inventory full. Cancelling operation.");
                break;
            }
            playerScript.PauseMovement(); //Pause player movement for duration of resource gathering

            if (deposited == totalDeposited) //Only do when actually gathering but only once
            {
                playerScript.gameManager.LogResourceInteraction(isPlayer, resourceDropped.resourceType);
            }

            yield return new WaitForSeconds(dropTime);

            playerScript.AddToInventory(resourceDropped);
            //playerScript.inventory.Add(resourceDropped);
            //playerScript.inventoryAmountFree -= resourceDropped.inventorySpaceTaken;

            Debug.Log("Item deposited");
        }
        totalDeposited = deposited;

        playerScript.ResumeMovement(); //Resume player movement

        if (deposited == dropAmount)
        {
            Destroy(gameObject); //GameObject currently gets destroyed instantly upon full inventory due to skipping the for loop entirely
            Debug.Log("All items deposited. Destroying resource object.");
        }
        else
        {
            //dropAmount -= deposited;
            Debug.Log("Depositing interrupted. "+(dropAmount-totalDeposited)+" items left.");
        }

        playerInteracting = false;
        playerScript.playerInteracting = playerInteracting;
        Debug.Log("Error track - End of GiveResources");
    }

    void InteractAction(GameObject playerObject)
    {
        Debug.Log("Resource interaction");
        if (!playerInteracting)
        {
            StartCoroutine(GiveResources(playerObject));
        }
        else
        {
            if (playerObject.CompareTag("Enemy") && playerUsing!=playerObject) //Fix agent getting stuck when interacting with resoruce player is gathering
            {
                Debug.Log("Agent attempting invalid gather. Resetting.");
                StartCoroutine(playerObject.GetComponent<EnemyAgent>().GetTrackingList()); //Reset agent 
            }
        }
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

    /*
    private IEnumerator ValidNavCheck()
    {
        yield return new WaitForSecondsRealtime(.5f);
        StartCoroutine(ValidCounter());
        navmeshAgent.destination = enemyBase.position;
        while (GetPathRemainingDistance() == -1) //If generates an invalid path, destroy to not mess up EnemyAgent
        {
            yield return null;
            if(validCounter == 6)
            {
                Debug.Log("Resource in invalid spawn Destroyed");
                Destroy(gameObject);
            }
        }
    }

    private IEnumerator ValidCounter()
    {
        while(validCounter < 6)
        {
            yield return new WaitForSecondsRealtime(1);
            validCounter++;
        }
    }
    */

    private void OnDestroy()
    {
        itemSpawner.ClearNullValues();
    }

    private void EnableNavAgent(bool enable)
    {
        navmeshAgent.enabled = enable;
    }
}
