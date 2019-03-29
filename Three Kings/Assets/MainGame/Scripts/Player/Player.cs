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

        hurtBox.size = new Vector3(entityBC2D.size.x - 0.03f, hurtBox.size.y);

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
        jumpAbility.onRegularJump += OnRegularJump;

        dashAbility.onAbilityCast += OnDash;
        dashAbility.onDashVectorCalculated += OnDashVectorCalculated;

        wallJumpAbility.onWallCling += OnWallCling;
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
        vaultAbility.CanActivate = false;
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
        return wallJumpAbility.CanActivate ? false : true; //So just is not activated when you can Wall Jump.
    }
    private void OnRegularJump()
    {
        wallJumpAbility.lockOutTimer = 0.15f; //So you can't Wall Jump immediately after leaving the ground.
    }


    private void OnDash()
    {
        jumpAbility.isJumping = false; //So Jump Cancel can't set Y velocity to Zero for a frame.
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



    private void OnWallCling()
    {
        ResetMobility(); //So you reset mobility while on a wall.
    }
    private void OnWallJump() 
    {
        jumpAbility.isJumping = false; //So Jump Cancel cannot stop Y velocity from Wall Jump.

        dashAbility.Cancel(); //So you can Wall Jump out of Dash
        dashAbility.ApplyLock(); //So you can't immediately Dash out of wall Jump.
    }
    private bool WallJumpRestric()
    {
        return IsGrounded ? false : true; //So it won't try to wall jump if you're grounded.
    }


    private bool SlashRestrict() 
    {
        return wallJumpAbility.CanActivate ? false : true; //So you can't slash during a Wall Jump.
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
        jumpAbility.isJumping = false; //So Jump Cancel cannot stop Y velocity

        dashAbility.Cancel(); //So you can Cancel Dash from a Pogo

        ResetMobility(); //So Pogo'ing resets mobility.
    }
    
}


