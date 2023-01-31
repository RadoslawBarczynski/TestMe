using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInputHandler : MonoBehaviour
{
    public Vector2 moveInputVector = Vector2.zero;
    public Vector2 viewInputVector = Vector2.zero;
    bool _isJumpButtonPressed = false;
    public bool isCrouchButtonPressed = false;
    bool _isFireButtonPressed = false;
    public float currentRecoilXPos;
    public float currentRecoilYPos;

    //components
    LocalCameraHandler localCameraHandler;
    CharacterMovementHandler characterMovementHandler;
    WeaponHandler weaponHandler;
    private void Awake()
    {
        localCameraHandler = GetComponentInChildren<LocalCameraHandler>();
        characterMovementHandler = GetComponent<CharacterMovementHandler>();
        weaponHandler = GetComponent<WeaponHandler>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!characterMovementHandler.Object.HasInputAuthority)
            return;

        //View input
        viewInputVector.x = Input.GetAxis("Mouse X") - weaponHandler.currentRecoilXPos; 
        viewInputVector.y = Input.GetAxis("Mouse Y") * -1 - Mathf.Abs(weaponHandler.currentRecoilYPos); //Invert the mouse look

        //Move input
        moveInputVector.x = Input.GetAxis("Horizontal");
        moveInputVector.y = Input.GetAxis("Vertical");

        //Jump input
        if (Input.GetButtonDown("Jump"))
        {
            _isJumpButtonPressed = true;
        }            

        if(Input.GetButtonDown("Crouch") && isCrouchButtonPressed == false)
        {
            isCrouchButtonPressed = true;
        }
        else if(Input.GetButtonDown("Crouch") && isCrouchButtonPressed == true)
        {
            isCrouchButtonPressed = false;
        }

        //Fire
        if (Input.GetButtonDown("Fire1"))
        {
            _isFireButtonPressed = true;
        }
        if (Input.GetButtonUp("Fire1"))
        {
            _isFireButtonPressed = false;
            weaponHandler.currentRecoilXPos = 0;
            weaponHandler.currentRecoilYPos = 0;
        }

        //Set view
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
        networkInputData.isJumpPressed = _isJumpButtonPressed;

        //Crouch data
        networkInputData.isCrouchedPressed = isCrouchButtonPressed;

        //Shoot data
        networkInputData.isFireButtonPressed = _isFireButtonPressed;

        //Reset variables to read their states
        _isJumpButtonPressed = false;

        return networkInputData;
    }
}
