using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// The Parent Player class will contain functionality that is common to both human and AI opponent players,
/// i.e Interacting with resources, interacting with bases, inventory checking, etc.
/// Both human and AI player scripts may inherit this one.
/// </summary>
public class ParentPlayer : MonoBehaviour
{
    public int maxInventorySize = 20;
    public int inventoryAmountFree;
    public List<Resource> inventory; //List of resource items to be deposited into base for points. May be changed to tuple with name and points in case of problems with system.

    public int score = 0; //Total score

    public bool playerInteracting; //Curewntly interacting with an object;

    public GameObject interactableObject; //object to interact with (resource/base)

    [SerializeField] protected float movementSpeed; //To be used in charactercontroller for human player and A* scripts for AI player
    [SerializeField] protected float defaultMovementSpeed = 5; //To be used in charactercontroller for human player and A* scripts for AI player

    public GameManager gameManager;

    Coroutine walkingCoroutine;
    // Start is called before the first frame update
    protected virtual void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        inventory = new List<Resource>();
        ResetInventory(); //Set inventory free amount to max
        ResumeMovement();
    }

    // Update is called once per frame
    void Update()
    {

    }

    /*
     * Set methods to appropriate return type for inheritance later
     */

    // Player interacts with world (ie. human player presses interaction key). Will carry out appropriate action if within trigger of base or resource
    public virtual void Interact()
    {
        playerInteracting = true; //Set to true by default. This should fix missed interaction in GatherResource
        Debug.Log("Interact Action requested by "+gameObject.tag);
        if(interactableObject != null)
        {
            Debug.Log("Object found. Interacting...");
            interactableObject.SendMessageUpwards("InteractAction", gameObject);
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        //Debug.Log("OnTriggerEnter");
        interactableObject = other.gameObject;
     }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.gameObject == interactableObject) //Make sure trigger exit doesnt clear the wrong object
            interactableObject = null;
    }

    /*
    void ResourceInteract()
    {
        Debug.Log("Interacting with resource");
    }

    
    void BaseInteract()
    {
        Debug.Log("Interacting with base");
    }*/

    public bool IsInventoryFull(int resourceSize)
    {
        if ((inventoryAmountFree == 0) || (resourceSize > inventoryAmountFree))
            return true;
        else return false;
    }


    public virtual void AddScore(int index)
    {
        score += inventory[index].points;
        Debug.Log(inventory[index].name + " with " + inventory[index].points + " points deposited. " + tag + " score is now: " + score);

        gameManager.UpdateScoreText(tag, score);
    }

    public virtual void AddToInventory(Resource resourceDropped)
    {
        inventory.Add(resourceDropped);
        inventoryAmountFree -= resourceDropped.inventorySpaceTaken;
    }

    public virtual void ResetInventory()
    {
        inventoryAmountFree = maxInventorySize;
        inventory.Clear();
    }

    public virtual void PauseMovement()
    {
        movementSpeed = 0;
        if (walkingCoroutine != null)
        {
            StopCoroutine(walkingCoroutine);
            walkingCoroutine = null;
        }
    }

    public virtual void ResumeMovement()
    {
        movementSpeed = defaultMovementSpeed;
        if(walkingCoroutine == null) walkingCoroutine = StartCoroutine(WalkingLog());
    }

    public virtual void InventoryRemainderPenalize()
    {
        //Leave empty. To be set in EnemyPlayer, and links to EnemyAgent.
    }

    public virtual void FullInventoryPenalize()
    {
        //Leave empty. To be set in EnemyPlayer, and links to EnemyAgent.
    }


    private IEnumerator WalkingLog()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            if (gameObject.CompareTag("Player"))
                gameManager.LogMovement(true);
            else gameManager.LogMovement(false);
        }
    }
}
