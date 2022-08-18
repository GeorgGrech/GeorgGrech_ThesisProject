using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Class for actual resource dropped by the Resource Object that will go into the player inventory then later deposited into the player base
/// </summary>
///
[System.Serializable]
//May be moved into ResourceObject class and saved into player inventory as a tuple in case this system does not work
public class Resource
{
    public string name; //name of the resource, i.e "Wood" or "Iron"
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

    // Start is called before the first frame update
    void Start()
    {

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
            //Obtain player script

        }
    }

    //Dropping the player resources in thier inventory
    public IEnumerator GiveResources()
    {
        for (int i = 0; i < dropAmount; i++)
        {
            yield return new WaitForSeconds(dropTime);
            //Deposit item
            Debug.Log("Item deposited");
        }
        Debug.Log("All items deposited. Destroying resource object.");
        Destroy(gameObject);
    }
}
