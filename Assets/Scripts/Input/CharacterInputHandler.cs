using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInputHandler : MonoBehaviour
{
    Vector2 moveInputVector = Vector2.zero;
    Vector2 viewInputVector = Vector2.zero;
    bool isJumpButtonPressed = false;
    bool isCrouchButtonPressed = false;

    //Other components
    LocalCameraHandler localCameraHandler;
    private void Awake()
    {
        localCameraHandler = GetComponentInChildren<LocalCameraHandler>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        //View input
        viewInputVector.x = Input.GetAxis("Mouse X");
        viewInputVector.y = Input.GetAxis("Mouse Y") * -1; //Invert the mouse look

        //Move input
        moveInputVector.x = Input.GetAxis("Horizontal");
        moveInputVector.y = Input.GetAxis("Vertical");

        //jump input
        if(Input.GetButtonDown("Jump"))
            isJumpButtonPressed = true;

        if (Input.GetKeyDown(KeyCode.C) && isCrouchButtonPressed == false)
        {
            isCrouchButtonPressed = true;
        }

        //set view
        localCameraHandler.SetViewInputVector(viewInputVector);
        
    }

    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData();

        //Aim data
        networkInputData.aimForwardVector = localCameraHandler.transform.forward;

        //Move data
        networkInputData.movementInput = moveInputVector;

        //Jump data
        networkInputData.isJumpPressed = isJumpButtonPressed;

        networkInputData.isCrouchedPressed = isCrouchButtonPressed;

        //reset variables to read their states
        isJumpButtonPressed = false;
        isCrouchButtonPressed = false;

        return networkInputData;
    }
}
