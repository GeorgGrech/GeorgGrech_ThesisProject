using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base : MonoBehaviour
{
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
            //Check own tag/variable to check whether or not to allow player interaction
            Debug.Log("Player entered within range of " + gameObject.name);
        }

        //else statement for enemy
    }
}
