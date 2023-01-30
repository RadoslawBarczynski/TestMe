using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class WeaponHandler : NetworkBehaviour
{
    [Networked(OnChanged = nameof(OnFireChanged))]
    public bool isFiring { get; set; }

    public ParticleSystem muzzleFlashEffect;
    public Transform aimPoint;
    public LayerMask collisionLayers;
    public GameObject impactEffect;
    public Gun gunData;

    float lastTimeFired = 0;
    public float currentRecoilXPos;
    public float currentRecoilYPos;


    //components
    HPHandler hPHandler;
    CharacterInputHandler characterInputHandler;

    private void Awake()
    {
        hPHandler = GetComponent<HPHandler>();
        characterInputHandler = GetComponent<CharacterInputHandler>();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    public override void FixedUpdateNetwork()
    {
        if (hPHandler.isDead)
        {
            return;
        }

        //input from network
        if(GetInput(out NetworkInputData networkInputData))
        {
            if (networkInputData.isFireButtonPressed)
            {
                Fire(networkInputData.aimForwardVector);
            }
        }
    }

    void Fire(Vector3 aimForwardVector)
    {
        //limit fire rate
        if (Time.time - lastTimeFired > 1 / gunData.fireRate)
        {
            lastTimeFired = Time.time;

            StartCoroutine(FireEffectC0());
            RecoilMath();

            Runner.LagCompensation.Raycast(aimPoint.position, aimForwardVector, 100, Object.InputAuthority, out var hitInfo, collisionLayers, HitOptions.IncludePhysX);

            float hitDistance = 100;
            bool isHitOtherPlayer = false;

            if (hitInfo.Distance > 0)
            {
                hitDistance = hitInfo.Distance;
            }

            if (hitInfo.Hitbox != null)
            {
                Debug.Log($"{Time.time} {transform.name} hit hitbox {hitInfo.Hitbox.transform.root.name}");

                if (Object.HasStateAuthority)
                {
                    hitInfo.Hitbox.transform.root.GetComponent<HPHandler>().OnTakeDamage();
                }

                isHitOtherPlayer = true;
            }
            else if (hitInfo.Collider != null)
            {
                Debug.Log($"{Time.time} {transform.name} hit PhysX hitbox {hitInfo.Collider.transform.root.name}");
                Instantiate(impactEffect, hitInfo.Point, Quaternion.LookRotation(hitInfo.Normal));
            }

            if (isHitOtherPlayer)
            {
                Debug.DrawRay(aimPoint.position, aimForwardVector * hitDistance, Color.red, 1);
            }
            else
            {
                Debug.DrawRay(aimPoint.position, aimForwardVector * hitDistance, Color.green, 1);
            }

        }
    }

    //recoil calculations
    public void RecoilMath()
    {
        if (!characterInputHandler.isCrouchButtonPressed) 
        {
            currentRecoilXPos = ((Random.value - .5f) / gunData.recoilValue);
            currentRecoilYPos = ((Random.value - .5f) / gunData.recoilValue);
        }
        else if(characterInputHandler.isCrouchButtonPressed)
        {
            currentRecoilXPos = ((Random.value - .5f) / (gunData.recoilValue * 2));
            currentRecoilYPos = ((Random.value - .5f) / (gunData.recoilValue * 2));
        }
    }

    IEnumerator FireEffectC0()
    {
        isFiring = true;

        muzzleFlashEffect.Play();

        yield return new WaitForSeconds(0.09f);

        isFiring = false;
    }

    static void OnFireChanged(Changed<WeaponHandler> changed)
    {
        //Debug.Log($"{Time.time} OnFireChanged value {changed.Behaviour.isFiring}");

        bool isFiringCurrent = changed.Behaviour.isFiring;

        //load old value
        changed.LoadOld();

        bool isFiringOld = changed.Behaviour.isFiring;

        if(isFiringCurrent && !isFiringOld)
        {
            changed.Behaviour.OnFireRemote();
        }
    }

    void OnFireRemote()
    {
        if (!Object.HasInputAuthority)
        {
            muzzleFlashEffect.Play();
        }
    }

}
