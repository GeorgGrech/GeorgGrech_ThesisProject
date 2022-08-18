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
    protected int maxInventorySize;
    protected int inventoryAmountUsed;
    public List<Resource> inventory; //List of resource items to be deposited into base for points. May be changed to tuple with name and points in case of problems with system.

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Test");
    }

    // Update is called once per frame
    void Update()
    {

    }

    /*
     * Set methods to appropriate return type for inheritance later
     */

    // Player interacts with world (ie. human player presses interaction key). Will carry out appropriate action if within trigger of base or resource
    void Interact()
    {
        Debug.Log("Interacting...");
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

    private void ResetInventory()
    {
        inventoryAmountUsed = 0;
    }
}
