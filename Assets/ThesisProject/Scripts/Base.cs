using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Base : MonoBehaviour
{

    private bool playerInteracting; //To check if already being interacted with to avoid duplicate interaction
    [SerializeField] private float depositTime = 3;

    private GameObject canvasObject;
    [SerializeField] private GameObject depositText;


    // Start is called before the first frame update
    void Start()
    {
        canvasObject = transform.Find("Canvas").gameObject;
        canvasObject.GetComponent<Canvas>().worldCamera = Camera.main;
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
        else
        {
            playerScript = playerObject.GetComponent<EnemyPlayer>();
            
            playerScript.InventoryRemainderPenalize();
        }

        playerScript.playerInteracting = playerInteracting;

        playerScript.PauseMovement();

        for (int i = 0; i < playerScript.inventory.Count; i++)
        {
            yield return new WaitForSeconds(depositTime);
            playerScript.AddScore(i); //Add points of object in player score
            StartCoroutine(DisplayDepositText(playerScript.inventory[i]));
        }
        
        playerScript.ResetInventory();
        playerScript.ResumeMovement();
        playerInteracting = false;
        playerScript.playerInteracting = playerInteracting;
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

    private IEnumerator DisplayDepositText(Resource resource)
    {
        GameObject textObject = Instantiate(depositText, canvasObject.transform);
        textObject.GetComponent<TextMeshProUGUI>().text = resource.resourceType.ToString() + " +" + resource.points + " points";
        yield return new WaitForSeconds(depositTime);
        Destroy(textObject);
    }
}
