using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

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

    private List<Ability> abilities = new List<Ability>();


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

        hurtBox.size = new Vector3(entityBC2D.size.x - 0.025f, hurtBox.size.y);

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

        GroundCheck();

        foreach (Ability abil in abilities)
        {
            abil.AbilityUpdate();
        }

        //Get Input Axis [Horizontal Movement]
        if (currControlType == ControlType.CanControl)
        {
            baseInputSpeed.BaseValue = InputSmoothing("Horizontal") * InputMultiplier;
        }
        else
        {
            baseInputSpeed.BaseValue = 0;
        }

        if (currControlType == ControlType.CanControl)
        {
            //Jumping
            if (Input.GetButtonDown("Jump"))
            {
                jumpAbility.CastAbility();
            }

            if (!Input.GetButton("Jump"))
            {
                jumpAbility.Cancel();
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
        }

        if (currControlType == ControlType.CanControl || dashAbility.isDashing)
        {
            //Wall Jump
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
                if (!vaultAbility.CanCast)
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
    }


    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        foreach (Ability abil in abilities)
        {
            abil.AbilityFixedUpdate();
        }
    }

    //----------------------------------------------------------------------------------

    // METHODS

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

        //Interactions
        jumpAbility.castRestriction += JumpRestric;

        dashAbility.onAbilityCast += OnDash;

        wallJumpAbility.onWallCling += onWallCling;
        wallJumpAbility.onAbilityCast += OnWallJump;
        wallJumpAbility.castRestriction += WallJumpRestric;

        vaultAbility.onVaultSuccess += ResetMobility;

        slashAbility.castRestriction += SlashRestrict;
        slashAbility.onSlashHit += OnSlashHit;
        slashAbility.onPogo += OnSlashPogo;
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

    }

    protected override void PhysicsCleanUpOnDeath(Attack attack)
    {
        jumpAbility.Cancel();
        dashAbility.Cancel();
        glideAbility.Cancel();
        wallJumpAbility.Cancel();
        vaultAbility.Cancel();
        vaultAbility.CanCast = false;
        base.PhysicsCleanUpOnDeath(attack);
    }

    protected override void ControlTypeInstaStopAux(ControlType givenType)
    {
        jumpAbility.Cancel();
        dashAbility.Cancel();
        glideAbility.Cancel();
        wallJumpAbility.Cancel();
        vaultAbility.Cancel();
        base.ControlTypeInstaStopAux(givenType);
    }

    public void ResetMobility()
    {
        jumpAbility.currentExtraJumps = jumpAbility.maxExtraJumps;
        dashAbility.currentDashCharge = dashAbility.maxDashCharge;
    }

    /// <summary>
    /// Below are Ability Interactions:
    /// 
    /// "On" methods are called when the ability does something.
    /// "Restric" methods are called when attempts to cast the ability are made and act as extra requirement.
    /// </summary>

    private bool JumpRestric()
    {
        return wallJumpAbility.CanCast ? false : true;
    }


    private void OnDash()
    {
        jumpAbility.isJumping = false;
    }


    private void onWallCling()
    {
        ResetMobility();
    }
    private void OnWallJump()
    {
        jumpAbility.isJumping = false;

        dashAbility.Cancel();
        dashAbility.ApplyLock();
    }
    private bool WallJumpRestric()
    {
        return IsGrounded ? false : true;
    }


    private bool SlashRestrict()
    {
        return wallJumpAbility.CanCast ? false : true;
    }
    private void OnSlashHit(HealthControl givenHealh)
    {
        Debug.Log("Slash Hit");
        dashAbility.Cancel();
        if(givenHealh != null)
        {
            swatheEnergy.CurrentEnergy += 1;
        }
    }
    private void OnSlashPogo()
    {
        jumpAbility.isJumping = false;

        dashAbility.Cancel();

        ResetMobility();
    }
    
}


