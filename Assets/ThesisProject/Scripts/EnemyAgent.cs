using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

/// <summary>
/// EnemyAgent class, inheritng base ML Agent class, to handle all decisions and direct actions, including ones from ParentPlayer class
/// that may be grouped together as one action (i.e "Go to base" that includes setting target to base and interacting)
/// </summary>
public class EnemyAgent : Agent
{
    //ParentPlayer playerScript;
    //public Transform target;
    // Start is called before the first frame update
    void Start()
    {
        //playerScript = gameObject.GetComponent<ParentPlayer>(); 
    }

    // Update is called once per frame
    void Update()
    {
        //Interact();
    }

    void Interact()
    {
        //playerScript.Interact();
    }

    void MoveToTarget(Transform target)
    {

    }
}

/// <summary>
/// EnemyPlayer class, inheriting ParentPlayer script, to handle basic actions such as Interaction as well as inventory management.
/// Set as separate class due to inability to inherit both Agent and ParentPlayer. Not set as ParentPlayer component for easier management.
/// </summary>
public class EnemyPlayer : ParentPlayer
{
    protected override void Start()
    {
        base.Start();
    }

    public override void Interact()
    {
        base.Interact();
    }
}
