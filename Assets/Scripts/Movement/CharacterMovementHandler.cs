using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using ExitGames.Client.Photon.StructWrapping;

public class CharacterMovementHandler : NetworkBehaviour
{
    bool _isRespawnRequested = false;
    float _crouchYScale = 0.7f;

    //components
    NetworkCharacterControllerPrototypeCustom networkCharacterControllerPrototypeCustom;
    HPHandler hPHandler;

    private void Awake()
    {
        networkCharacterControllerPrototypeCustom = GetComponent<NetworkCharacterControllerPrototypeCustom>();
        hPHandler = GetComponent<HPHandler>();
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            if (_isRespawnRequested)
            {
                Respawn();
                return;
            }

            //Dont update the clients position when they are dead
            if (hPHandler.isDead)
            {
                return;
            }
        }

        //Get the input from the network
        if (GetInput(out NetworkInputData networkInputData))
        {
            //Rotate the transform according to the client aim vector
            transform.forward = networkInputData.aimForwardVector;

            //Disable player tilting
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
                transform.localScale = new Vector3(transform.localScale.x, _crouchYScale, transform.localScale.z);
            }
            else if (!networkInputData.isCrouchedPressed)
            {
                if (transform.localScale.y < 1)
                {
                    transform.localScale = new Vector3(transform.localScale.x, 1, transform.localScale.z);
                    transform.position = new Vector3(transform.position.x, 1.58f, transform.position.z);
                    networkCharacterControllerPrototypeCustom.UnCrouch();
                }
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
        _isRespawnRequested = true;
    }

    void Respawn()
    {
        networkCharacterControllerPrototypeCustom.TeleportToPosition(Utils.GetRandomSpawnPoint());

        hPHandler.OnRespawned();

        _isRespawnRequested = false;
    }

    public void SetCharacterControllerEnabled(bool isEnabled)
    {
        networkCharacterControllerPrototypeCustom.Controller.enabled = isEnabled;
    }

}
