using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;

[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(PlayerEnergy))]
public class Player : LivingEntity
{
    public static Player instance;

    [Header("Swathe: Movement")]
    public float InputMultiplier = 6.5f;

    [Header("References:")]
    public BoxCollider2D hurtBox;
    public SpriteRenderer sprite;
    [HideInInspector] public PlayerHealth swatheHealth;
    [HideInInspector] public PlayerEnergy swatheEnergy;

    [Header("Mobility Abilities")]
    public Jump jumpAbility;
    public WallJump wallJumpAbility;
    public Dash dashAbility;
    public Glide glideAbility;
    public Vault vaultAbility;

    [Header("Combat Abilities")]
    public BasicSlash slashAbility;
    public BlastAttack blastAbility;
    public FocusHeal focusHealAbility;

    private List<Ability> abilities = new List<Ability>();

    public Action interactableMethod;

    public override Vector2 EntityVelocity
    {
        get
        {
            return base.EntityVelocity;
        }
        set
        {
            base.EntityVelocity = value;
        }
    }


    protected override void Awake()
    {
        base.Awake();

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }

        hurtBox.size = new Vector3(EntityBC2D.size.x - 0.03f, hurtBox.size.y);

        swatheHealth = (PlayerHealth)healthControl;
        swatheEnergy = GetComponent<PlayerEnergy>();

        SetUpAbilities();
    }


    protected override void Start()
    {
        base.Start();
    }


    protected override void Update()
    {
        base.Update();             
    }


    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    //----------------------------------------------------------------------------------

    // METHODS

    

    public override void BaseActionControl()
    {
        base.BaseActionControl();

        InputVector.X.BaseValue = InputSmoothing("Horizontal") * InputMultiplier;

        //Jumping
        if (Input.GetButtonDown("Jump"))
        {
            jumpAbility.CastAbility();
        }

        //Gliding
        if (Input.GetButton("Glide"))
        {
            glideAbility.CastAbility();
        }
        if (!Input.GetButton("Glide"))
        {
            glideAbility.Cancel();
        }

        if (Input.GetKey(KeyCode.B))
        {
            focusHealAbility.CastAbility();
        }


        //Blast Attack
        /*
        if (Input.GetMouseButtonDown(0))
        {
            blastAbility.CastAbility();
        }
        */


        //[DEBUG]
        if (Input.GetKeyDown(KeyCode.R))
        {
            RespawnSwathe();
        }

        if(Input.GetAxis("Vertical") > 0 && IsGrounded)
        {
            interactableMethod?.Invoke();
        }

        if (Input.GetButtonDown("Jump"))
        {
            wallJumpAbility.CastAbility();
        }

        //Dashing
        if (Input.GetButtonDown("Dash"))
        {
            dashAbility.CastAbility();
        }

        //Slash
        if (Input.GetButtonDown("Slash"))
        {
            if (!vaultAbility.CanActivate)
            {
                slashAbility.CastAbility();
            }
            else
            {
                //Vault
                vaultAbility.CastAbility();
            }
        }
        /*
        //Vault Toggle
        if (Input.GetButtonDown("Vault"))
        {
            vaultAbility.Toggle();
        }
        */
    }
    public override void BaseActionUpdate()
    {
        base.BaseActionUpdate();

        GroundCheck();

        foreach (Ability abil in abilities)
        {
            abil.AbilityUpdate();
        }
    }

    public override void BaseActionFixedUpdate()
    {
        base.BaseActionFixedUpdate();

        foreach (Ability abil in abilities)
        {
            abil.AbilityFixedUpdate();
        }
    }

    private void SetUpAbilities()
    {
        //Mobility
        abilities.Add(jumpAbility);
        abilities.Add(wallJumpAbility);
        abilities.Add(dashAbility);
        abilities.Add(glideAbility);
        abilities.Add(vaultAbility);

        //Combat
        abilities.Add(slashAbility);
        abilities.Add(blastAbility);
        abilities.Add(focusHealAbility);

        //Interactions
        jumpAbility.castRestriction += JumpRestric;
        jumpAbility.onRegularJump += OnRegularJump;

        dashAbility.onDashVectorCalculated += OnDashVectorCalculated;
        dashAbility.DashControl = DashControl;

        wallJumpAbility.onWallCling += OnWallCling;
        wallJumpAbility.onAbilityCast += OnWallJump;

        vaultAbility.onVaultSuccess += ResetMobility;

        slashAbility.castRestriction += SlashRestrict;
        slashAbility.onSlashHit += OnSlashHit;
        slashAbility.onPogo += OnSlashPogo;

        focusHealAbility.castRestriction += FocusHealRestric;
        focusHealAbility.FocusHealthControl = FocusHealControl;
        focusHealAbility.onPayment += OnFocusHealthPayment;
        focusHealAbility.onComplete += OnFocusHealComplete;
    }

    public void RespawnSwathe()
    {
        RespawnManager.RespawnData data = new RespawnManager.RespawnData()
        {
            sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
            respawnPosition = Vector3.zero
        };
        GameController.instance.respawnManager.MajorRespawnData = data;
        swatheHealth.Death();

        onManualRespawn?.Invoke();

    }
    public Action onManualRespawn;

    public void ResetMobility()
    {
        jumpAbility.currentExtraJumps = jumpAbility.maxExtraJumps;
        dashAbility.currentDashCharge = dashAbility.maxDashCharge;
    }

    public void EnableHitboxAndVisuals()
    {
        sprite.gameObject.SetActive(true);
        hurtBox.enabled = true;
    }

    public void DisableHitboxAndVisuals()
    {
        sprite.gameObject.SetActive(false);
        hurtBox.enabled = false;
    }

    /// <summary>
    /// Below are Ability Interactions:
    /// 
    /// "On" methods are called when the ability does something.
    /// "Restric" methods are called when attempts to cast the ability are made and act as extra requirement.
    /// </summary>

    private bool JumpRestric()
    {
        return wallJumpAbility.CanActivate ? false : true; //So just is not activated when you can Wall Jump.
    }
    private void OnRegularJump()
    {
        wallJumpAbility.lockOutTimer = 0.15f; //So you can't Wall Jump immediately after leaving the ground.
    }

    
    private Vector2 OnDashVectorCalculated(Vector2 dashVec) 
    {
        if (wallJumpAbility.CanActivate) //So if you're on a wall you always dash to the side.
        {
            if(isLookingRight && dashVec.x < 0)
            {
                if(dashVec.y > 0)
                {
                    return Vector2.up;
                }
                else
                {
                    return Vector2.right;
                }              
            }
            else if(!isLookingRight && dashVec.x > 0)
            {
                if (dashVec.y > 0)
                {
                    return Vector2.up;
                }
                else
                {
                    return Vector2.left;
                }
            }
            else
            {
                return dashVec;
            }
        }
        return dashVec;
    }
    private void DashControl()
    {
        wallJumpAbility.AbilityUpdate();

        if (Input.GetButtonDown("Jump"))
        {
            wallJumpAbility.CastAbility();
        }

        //Slash
        if (Input.GetButtonDown("Slash"))
        {
            if (!vaultAbility.CanActivate)
            {
                slashAbility.CastAbility();
            }
            else
            {
                //Vault
                vaultAbility.CastAbility();
            }
        }
        //Vault Toggle
        if (Input.GetButtonDown("Vault"))
        {
            vaultAbility.Toggle();
        }
    }



    private void OnWallCling()
    {
        if (!dashAbility.isDashing)
        {
            ResetMobility(); //So you reset mobility while on a wall.
        }
    }
    private void OnWallJump() 
    {
        dashAbility.ApplyLock(); //So you can't immediately Dash out of wall Jump.
    }


    private bool SlashRestrict() 
    {
        return wallJumpAbility.CanActivate ? false : true; //So you can't slash during a Wall Jump.
    }
    private void OnSlashHit(HealthControl givenHealh)
    {
        if (dashAbility.isDashing)
        {
            OriginalStateSet();
        }
        if (givenHealh != null && !givenHealh.gameObject.CompareTag("Hitable") && !givenHealh.gameObject.CompareTag("Hazard"))
        {
            swatheEnergy.CurrentEnergy += 1;
        }
    }
    private void OnSlashPogo()
    {     
        ResetMobility(); //So Pogo'ing resets mobility.
    }

    private bool FocusHealRestric()
    {
        if(IsGrounded && EntityVelocity.y <= 0 && swatheEnergy.CurrentEnergy >= focusHealAbility.cost)
        {
            return true;
        }
        return false;
    }
    private void FocusHealControl()
    {
        base.BaseActionControl();

        InputVector.X.BaseValue = InputSmoothing("Horizontal") * InputMultiplier;

        if (!Input.GetKey(KeyCode.B))
        {
            focusHealAbility.Cancel();
            OriginalStateSet();
        }

        if (Input.GetButtonDown("Dash"))
        {
            dashAbility.CastAbility();
        }

    }
    private void OnFocusHealthPayment()
    {
        swatheEnergy.PayEnergy(focusHealAbility.cost);
        Debug.Log("Pay");
    }
    private void OnFocusHealComplete()
    {
        swatheHealth.CurrentHealth += 1;
    }
    
}


