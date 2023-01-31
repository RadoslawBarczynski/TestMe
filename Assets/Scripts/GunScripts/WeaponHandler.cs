using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.UI;

public class WeaponHandler : NetworkBehaviour
{
    [Networked(OnChanged = nameof(OnFireChanged))]
    public bool isFiring { get; set; }

    [SerializeField]
    Image _hitMarkImage;

    public ParticleSystem muzzleFlashEffect;
    public Transform aimPoint;
    public LayerMask collisionLayers;
    public GameObject impactEffect;
    public Gun gunData;

    float _lastTimeFired = 0;
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
        if (Time.time - _lastTimeFired > 1 / gunData.fireRate)
        {
            _lastTimeFired = Time.time;

            StartCoroutine(FireEffectC0());
            RecoilMath();

            Runner.LagCompensation.Raycast(aimPoint.position, aimForwardVector, 100, Object.InputAuthority, out var hitInfo, collisionLayers, HitOptions.IncludePhysX | HitOptions.IgnoreInputAuthority);

            float hitDistance = 100;
            bool isHitOtherPlayer = false;

            if (hitInfo.Distance > 0)
            {
                hitDistance = hitInfo.Distance;
            }

            if (hitInfo.Hitbox != null)
            {
                if (Object.HasStateAuthority)
                {
                    hitInfo.Hitbox.transform.root.GetComponent<HPHandler>().OnTakeDamage();
                }

                isHitOtherPlayer = true;
            }
            else if (hitInfo.Collider != null)
            {
                Instantiate(impactEffect, hitInfo.Point, Quaternion.LookRotation(hitInfo.Normal));
            }
            //Check a shoot ray in editor
            if (isHitOtherPlayer)
            {
                Debug.DrawRay(aimPoint.position, aimForwardVector * hitDistance, Color.red, 1);
                if (Object.HasInputAuthority)
                {
                    StartCoroutine(HitMarkEffect());
                }
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

    IEnumerator HitMarkEffect()
    {
        _hitMarkImage.enabled = true;

        yield return new WaitForSeconds(0.2f);

        _hitMarkImage.enabled = false;
    }

    static void OnFireChanged(Changed<WeaponHandler> changed)
    {
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
