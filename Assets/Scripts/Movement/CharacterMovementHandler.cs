using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using ExitGames.Client.Photon.StructWrapping;

public class CharacterMovementHandler : NetworkBehaviour
{

    //Other components
    NetworkCharacterControllerPrototypeCustom networkCharacterControllerPrototypeCustom;
    Camera localCamera;

    private void Awake()
    {
        networkCharacterControllerPrototypeCustom = GetComponent<NetworkCharacterControllerPrototypeCustom>();
        localCamera = GetComponentInChildren<Camera>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }


    public override void FixedUpdateNetwork()
    {
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
            if(networkInputData.isJumpPressed)
                networkCharacterControllerPrototypeCustom.Jump();

            //Check if player fallen off the world
            CheckFallRespawn();
        }
    }

    void CheckFallRespawn()
    {
        if(transform.position.y < -12) 
            transform.position = Utils.GetRandomSpawnPoint();
    }

}
