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

    private StateSetter benchState;

    protected override void Start()
    {
        base.Start();
        benchState = new StateSetter(this, null, BenchUpdate, null, BenchCancel, StateSetter.SetStrength.Medium);
        restLogo.SetActive(false);
        swathe = Player.instance;

    }

    protected override void Update()
    {
        base.Update();

        if (inRange && !interacting)
        {
            restLogo.SetActive(true);
        }
        else
        {
            restLogo.SetActive(false);
        }
    }

    private void BenchUpdate()
    {
        if (interacting && !isSitting)
        {
            swathe.entityRB2D.MovePosition(swathe.transform.position + ((sitPosition.position - swathe.transform.position) * Time.deltaTime * sitSpeed));
            if ((swathe.transform.position - sitPosition.position).magnitude < 0.02f)
            {
                isSitting = true;
                HasSat();
            }
        }

        if (Input.GetAxisRaw("Vertical") < 0 && isSitting)
        {
            swathe.OriginalStateSet();
        }
    }

    private void BenchCancel()
    {
        swathe.entityRB2D.bodyType = RigidbodyType2D.Dynamic;
        interacting = false;
        isSitting = false;
    }

    protected override void Interact()
    {
        swathe.SetLivingEntityState(benchState, false);
        swathe.InputAndPhysicsCleanUp();
        swathe.entityRB2D.bodyType = RigidbodyType2D.Kinematic;
        interacting = true;
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
