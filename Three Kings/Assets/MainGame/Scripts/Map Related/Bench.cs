using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bench : Interactable
{
    public Transform sitPosition;
    public bool isSitting;
    public float sitSpeed = 5f;

    public GameObject restLogo;
    public ParticleSystem sittingParticles;
    private Player swathe;

    protected override void Start()
    {
        base.Start();
        restLogo.SetActive(false);
        swathe = Player.instance;

    }

    protected override void Update()
    {
        base.Update();

        if (interacting && !isSitting)
        {
            swathe.entityRB2D.MovePosition(swathe.transform.position + ((sitPosition.position - swathe.transform.position) * Time.deltaTime * sitSpeed));
            if((swathe.transform.position - sitPosition.position).magnitude < 0.02f)
            {
                isSitting = true;
                HasSat();
            }
            
        }
        if(Input.GetAxisRaw("Vertical") < 0 && isSitting)
        {
            swathe.entityRB2D.bodyType = RigidbodyType2D.Dynamic;
            swathe.EntityControlTypeSet(LivingEntity.ControlType.CanControl, true);
            interacting = false;
            isSitting = false;
        }

        if (inRange && !interacting)
        {
            restLogo.SetActive(true);
        }
        else
        {
            restLogo.SetActive(false);
        }
    }

    protected override void Interact()
    {
        base.Interact();

        if(Input.GetAxisRaw("Vertical") > 0 && inRange && swathe.IsGrounded)
        {
            swathe.EntityControlTypeSet(LivingEntity.ControlType.OtherControl, true);
            swathe.entityRB2D.bodyType = RigidbodyType2D.Kinematic;
            interacting = true;

        }
    }

    private void HasSat()
    {
        swathe.healthControl.CurrentHealth = swathe.healthControl.MaxHealth;

        GameController.instance.respawnManager.MajorRespawnData = new RespawnManager.RespawnData()
        {
            respawnPosition = sitPosition.position,
            sceneName = SceneManager.GetActiveScene().name
        };

        Debug.Log("SIIIIT"); 

        sittingParticles.Play();

    }


}
