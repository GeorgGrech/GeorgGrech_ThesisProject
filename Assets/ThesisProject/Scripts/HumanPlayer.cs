using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HumanPlayer : ParentPlayer
{
    private CharacterController controller;
    //[SerializeField] private float movementSpeed = 5f;

    float gravity = 9.8f;
    float verticalSpeed = 0;

    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private GameObject inventorySlot;

    private GameObject[] inventorySlots;

    protected override void Start()
    {
        Debug.Log("Player Start");
        controller = GetComponent<CharacterController>();
        InitialiseInventory();
        base.Start();
    }

    private void InitialiseInventory()
    {
        inventorySlots = new GameObject[maxInventorySize];
        for (int i = 0; i < maxInventorySize; i++)
        {
            inventorySlots[i] = Instantiate(inventorySlot,inventoryUI.transform);
        }
    }

    void Update()
    {
        if (!gameManager.isGameFinished)
        {
            MovementInput();
            RotationInput();
            KeyboardInput();
        }
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

    public override void AddToInventory(Resource resourceDropped)
    {
        UpdateInventory(resourceDropped);
        base.AddToInventory(resourceDropped);
    }

    public void UpdateInventory(Resource resourceDropped)
    {
        for (int i = 0; i < resourceDropped.inventorySpaceTaken; i++)
        {
            GameObject slotToUpdate = inventorySlots[maxInventorySize - inventoryAmountFree + i];

            RawImage image = slotToUpdate.GetComponent<RawImage>();
            if (resourceDropped.resourceType == Resource.Type.Wood)
                image.color = new Color(75f / 255f, 17f / 255f, 12f / 255f);
            else if (resourceDropped.resourceType == Resource.Type.Iron)
                image.color = new Color(63f / 255f, 83f / 255f, 87f / 255f);
            else if (resourceDropped.resourceType == Resource.Type.Gold)
                image.color = new Color(173 / 255f, 83f / 167, 0f / 255f);

            TextMeshProUGUI text = slotToUpdate.GetComponentInChildren<TextMeshProUGUI>();
            text.text = resourceDropped.resourceType.ToString();
            text.enabled = true;
        }
    }

    public override void ResetInventory()
    {
        base.ResetInventory();
        ClearInventoryUI();
    }

    public void ClearInventoryUI()
    {
        for (int i = 0; i < maxInventorySize; i++)
        {
            GameObject slotToUpdate = inventorySlots[i];
            RawImage image = slotToUpdate.GetComponent<RawImage>();
            image.color = Color.white;

            TextMeshProUGUI text = slotToUpdate.GetComponentInChildren<TextMeshProUGUI>();
            text.enabled = false;
        }
    }
}
