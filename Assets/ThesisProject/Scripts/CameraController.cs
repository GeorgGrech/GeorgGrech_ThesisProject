using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;

    [SerializeField]
    private Vector3 targetOffset;

    [SerializeField]
    private float movementSpeed;

    [SerializeField]
    private bool trackingEnemy = false;
    // Start is called before the first frame update
    void Start()
    {
        if (!trackingEnemy)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform; //Find player
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!target && trackingEnemy) //If target still isn't assigned and tracking enemy is true
        {
            target = GameObject.FindGameObjectWithTag("Enemy").transform; //Find enemy
        }

        MoveCamera();
    }

    void MoveCamera()
    {
        if (target)
        {
            transform.position = Vector3.Lerp(transform.position, target.position + targetOffset, movementSpeed * Time.deltaTime);
        }
    }
}
