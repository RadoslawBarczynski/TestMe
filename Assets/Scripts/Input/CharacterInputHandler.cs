using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInputHandler : MonoBehaviour
{
    Vector2 moveInputVector = Vector2.zero;
    Vector2 viewInputVector = Vector2.zero;
    bool isJumpButtonPressed = false;
    bool isCrouchButtonPressed = false;
    int crouchState = 0;
    bool isFireButtonPressed = false;

    //Other components
    LocalCameraHandler localCameraHandler;
    CharacterMovementHandler characterMovementHandler;
    private void Awake()
    {
        localCameraHandler = GetComponentInChildren<LocalCameraHandler>();
        characterMovementHandler = GetComponent<CharacterMovementHandler>();
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
        if (!characterMovementHandler.Object.HasInputAuthority)
            return;

        //View input
        viewInputVector.x = Input.GetAxis("Mouse X");
        viewInputVector.y = Input.GetAxis("Mouse Y") * -1; //Invert the mouse look

        //Move input
        moveInputVector.x = Input.GetAxis("Horizontal");
        moveInputVector.y = Input.GetAxis("Vertical");

        //jump input
        if (Input.GetButtonDown("Jump"))
        {
            isJumpButtonPressed = true;
        }            

        if(Input.GetButtonDown("Crouch") && isCrouchButtonPressed == false)
        {
            isCrouchButtonPressed = true;
        }
        else if(Input.GetButtonDown("Crouch") && isCrouchButtonPressed == true)
        {
            isCrouchButtonPressed = false;
        }

        //fire
        if (Input.GetButtonDown("Fire1"))
        {
            isFireButtonPressed = true;
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

        networkInputData.isFireButtonPressed = isFireButtonPressed;

        //reset variables to read their states
        isJumpButtonPressed = false;
        isFireButtonPressed = false;

        return networkInputData;
    }
}
