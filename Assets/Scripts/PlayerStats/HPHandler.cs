using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using Unity.VisualScripting;
using TMPro;

public class HPHandler : NetworkBehaviour
{
    [Networked(OnChanged = nameof(OnHPChanged))]
    byte _HP { get; set; }

    [Networked(OnChanged = nameof(OnStateChanged))]
    public bool isDead { get; set; }

    bool _isInitialized = false;

    const byte _startingHP = 5;

    public Color uiOnHitColor;
    public Image uiOnHitImage;

    public MeshRenderer bodyMeshRenderer;
    Color defaultMeshBodyColor;

    public GameObject playerModel;
    public GameObject deathGameObjectPrefab;
    public TextMeshProUGUI healthText;

    //components
    HitboxRoot hitboxRoot;
    CharacterMovementHandler characterMovementHandler;
    WeaponHandler weaponHandler;

    private void Awake()
    {
        characterMovementHandler = GetComponent<CharacterMovementHandler>();
        hitboxRoot = GetComponentInChildren<HitboxRoot>();
        weaponHandler = GetComponent<WeaponHandler>();
    }
    // Start is called before the first frame update
    void Start()
    {
        _HP = _startingHP;
        isDead = false;

        defaultMeshBodyColor = bodyMeshRenderer.material.color;

        _isInitialized = true;
    }

    void Update()
    {
        //health state display 
        if(Object.HasInputAuthority)
        healthText.text = "Health: " + _HP + "/" + _startingHP;
    }

    IEnumerator OnHitCO()
    {
        bodyMeshRenderer.material.color = Color.white;

        if (Object.HasInputAuthority)
        {
            uiOnHitImage.color = uiOnHitColor;
        }
        yield return new WaitForSeconds(0.2f);

        bodyMeshRenderer.material.color = defaultMeshBodyColor;

        if(Object.HasInputAuthority && !isDead)
        {
            uiOnHitImage.color = new Color(0, 0, 0, 0);
        }
    }

    IEnumerator ServerReviveCO()
    {
        yield return new WaitForSeconds(2.0f);

        characterMovementHandler.RequestRespawn();
    }

    //function called on the server
    public void OnTakeDamage()
    {
        if (isDead)
        {
            return;
        }

        _HP -= (byte)weaponHandler.gunData.damage;

        if (_HP <= 0)
        {
            StartCoroutine(ServerReviveCO());

            isDead = true;
        }
    }

    static void OnHPChanged(Changed<HPHandler> changed)
    {
        byte newHP = changed.Behaviour._HP;

        changed.LoadOld();

        byte oldHP = changed.Behaviour._HP;

        if(newHP < oldHP)
        {
            changed.Behaviour.OnHPReduced();
        }
    }

    private void OnHPReduced()
    {
        if (!_isInitialized)
        {
            return;
        }
        StartCoroutine(OnHitCO());
    }

    static void OnStateChanged(Changed<HPHandler> changed)
    {
        bool isDeadCurrent = changed.Behaviour.isDead;

        changed.LoadOld();

        bool isDeadOld = changed.Behaviour.isDead;

        if (isDeadCurrent)
        {
            changed.Behaviour.OnDeath();
        }
        else if(!isDeadCurrent && isDeadOld)
        {
            changed.Behaviour.OnRevive();
        }
    }

    private void OnDeath()
    {
        playerModel.gameObject.SetActive(false);
        hitboxRoot.HitboxRootActive = false;
        characterMovementHandler.SetCharacterControllerEnabled(false);

        Instantiate(deathGameObjectPrefab, transform.position, Quaternion.identity);
    }

    private void OnRevive()
    {
        if (Object.HasInputAuthority)
        {
            uiOnHitImage.color = new Color(0, 0, 0, 0);
        }

        playerModel.gameObject.SetActive(true);
        hitboxRoot.HitboxRootActive = true;
        characterMovementHandler.SetCharacterControllerEnabled(true);
    }

    public void OnRespawned()
    {
        _HP = _startingHP;
        isDead = false;
    }
}
