using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base : MonoBehaviour
{

    private bool playerInteracting; //To check if already being interacted with to avoid duplicate interaction
    [SerializeField] private float depositTime = 3;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Check own tag/variable to check whether or not to allow player interaction
            Debug.Log("Player entered within range of " + gameObject.name);
        }

        //else statement for enemy
    }*/

    private IEnumerator ResourceDepositing(GameObject playerObject)
    {
        playerInteracting = true;
        ParentPlayer playerScript = null;

        if (playerObject.CompareTag("Player"))
        {
            playerScript = playerObject.GetComponent<HumanPlayer>();
        }
        //else condition to get enemy script

        playerScript.PauseMovement();

        for (int i = 0; i < playerScript.inventory.Count; i++)
        {
            yield return new WaitForSeconds(depositTime);
            playerScript.AddScore(i); //Add points of object in player score
        }
        
        playerScript.ResetInventory();
        playerScript.ResumeMovement();
        playerInteracting = false;
    }

    void InteractAction(GameObject playerObject)
    {
        Debug.Log("Base interaction");
        //GiveResources();
           
        if (tag.Contains(playerObject.tag) && !playerInteracting)  //Check if appropriate base (i.e "Player" with "PlayerBase" and "Enemy" and "EnemyBase"
        {
            StartCoroutine(ResourceDepositing(playerObject));
        }
    }
}
