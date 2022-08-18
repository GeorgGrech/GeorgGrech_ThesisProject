using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// (Currently unused)
/// The Parent Player class will contain functionality that is common to both human and AI opponent players,
/// i.e Interacting with resources, interacting with bases, inventory checking, etc.
/// Both human and AI player scripts may inherit this one.
/// </summary>
public class ParentPlayer : MonoBehaviour
{
    public int maxInventorySize = 20;
    public int inventoryAmountFree;
    public List<Resource> inventory; //List of resource items to be deposited into base for points. May be changed to tuple with name and points in case of problems with system.

    private GameObject interactableObject; //object to interact with (resource/base)

    // Start is called before the first frame update
    protected virtual void Start()
    {
        ResetInventory(); //Set inventory free amount to max
    }

    // Update is called once per frame
    void Update()
    {

    }

    /*
     * Set methods to appropriate return type for inheritance later
     */

    // Player interacts with world (ie. human player presses interaction key). Will carry out appropriate action if within trigger of base or resource
    protected virtual void Interact()
    {
        Debug.Log("Interact Action requested");
        if(interactableObject != null)
        {
            Debug.Log("Object found. Interacting...");
            interactableObject.SendMessageUpwards("InteractAction", gameObject);
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {

        Debug.Log("OnTriggerEnter");
        interactableObject = other.gameObject;
     }

    protected virtual void OnTriggerExit(Collider other)
    {
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


    private void ResetInventory()
    {
        inventoryAmountFree = maxInventorySize;
    }
}
