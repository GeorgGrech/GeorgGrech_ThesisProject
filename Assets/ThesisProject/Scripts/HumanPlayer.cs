using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanPlayer : ParentPlayer
{
    private CharacterController controller;
    [SerializeField] private float movementSpeed = 5f;

    float gravity = 9.8f;
    float verticalSpeed = 0;

    protected override void Start()
    {
        base.Start();
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        MovementInput();
        RotationInput();
        KeyboardInput();
    }

    void MovementInput()
    {

        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if (controller.isGrounded)
        {
            verticalSpeed = 0; // grounded character has vSpeed = 0...
        }
        verticalSpeed -= gravity * Time.deltaTime;
        move.y = verticalSpeed;

        controller.Move(move * Time.deltaTime * movementSpeed);
    }

    void RotationInput()
    {
        RaycastHit _hit;
        Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(_ray, out _hit))
        {
            transform.LookAt(new Vector3(_hit.point.x, transform.position.y, _hit.point.z));
        }
    }

    void KeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            base.Interact();
        }
    }
}
