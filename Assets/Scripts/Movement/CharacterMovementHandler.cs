using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using ExitGames.Client.Photon.StructWrapping;

public class CharacterMovementHandler : NetworkBehaviour
{
    bool isRespawnRequested = false;
    float startYScale;
    float crouchYScale = 0.7f;

    //Other components
    NetworkCharacterControllerPrototypeCustom networkCharacterControllerPrototypeCustom;
    HPHandler hPHandler;
    CharacterController characterController;

    private void Awake()
    {
        networkCharacterControllerPrototypeCustom = GetComponent<NetworkCharacterControllerPrototypeCustom>();
        hPHandler = GetComponent<HPHandler>();
        characterController = GetComponent<CharacterController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        startYScale = transform.localScale.y;
    }


    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            if (isRespawnRequested)
            {
                Respawn();
                return;
            }

            //dont update the clients position when they are dead
            if (hPHandler.isDead)
            {
                return;
            }
        }

        //Get the input from the network
        if (GetInput(out NetworkInputData networkInputData))
        {
            //rotate the transform according to the client aim vector
            transform.forward = networkInputData.aimForwardVector;

            //disable player tilting
            Quaternion rotation = transform.rotation;
            rotation.eulerAngles = new Vector3(0, rotation.eulerAngles.y, rotation.eulerAngles.z);
            transform.rotation = rotation;

            //Move
            Vector3 moveDirection = transform.forward * networkInputData.movementInput.y + transform.right * networkInputData.movementInput.x;
            moveDirection.Normalize();

            networkCharacterControllerPrototypeCustom.Move(moveDirection);

            //Jump
            if (networkInputData.isJumpPressed)
            {
                networkCharacterControllerPrototypeCustom.Jump();
            }

            if (networkInputData.isCrouchedPressed)
            {
                networkCharacterControllerPrototypeCustom.Crouch();
                transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            }
            else if (!networkInputData.isCrouchedPressed)
            {
                networkCharacterControllerPrototypeCustom.UnCrouch();
                //networkCharacterControllerPrototypeCustom.Velocity = new Vector3(networkCharacterControllerPrototypeCustom.Velocity.x, 0, networkCharacterControllerPrototypeCustom.Velocity.z);
                transform.localScale = new Vector3(transform.localScale.x, 1, transform.localScale.z);
            }
            //Check if player fallen off the world
            CheckFallRespawn();
        }
    }

    void CheckFallRespawn()
    {
        if (transform.position.y < -12)
        {
            Respawn();
        }
    }

    public void RequestRespawn()
    {
        isRespawnRequested = true;
    }

    void Respawn()
    {
        networkCharacterControllerPrototypeCustom.TeleportToPosition(Utils.GetRandomSpawnPoint());

        hPHandler.OnRespawned();

        isRespawnRequested = false;
    }

    public void SetCharacterControllerEnabled(bool isEnabled)
    {
        networkCharacterControllerPrototypeCustom.Controller.enabled = isEnabled;
    }

}
