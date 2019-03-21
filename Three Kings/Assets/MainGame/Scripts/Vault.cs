using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;

public class Vault : Ability
{
    [Header("Vault:")]
    public float vaultSpeed = 21f;
    public float vaultTime = 0.25f;
    public float frozenTime = 0.5f;
    public bool isVaulting;

    [Header("References:")]
    public Dash dashRef;
    public BasicSlash basicSlashRef;
    [Space]
    public GameObject indicatorPrefab;
    public ParticleSystem particles;

    private GameObject spawnedIndicator;
    List<Collider2D> lastHits = new List<Collider2D>();
    public Action onVaultSuccess;

    public override bool AbilityActivated
    {
        get
        {
            return base.AbilityActivated;
        }
        set
        {
            base.AbilityActivated = value;
            if (AbilityActivated == false)
            {
                CanCast = false;
            }
        }
    }

    public override bool CanCast
    {
        get
        {
            return base.CanCast;
        }
        set
        {
            if (AbilityActivated)
            {
                base.CanCast = value;
            }
            else
            {
                base.CanCast = false;
            }

            if (CanCast)
            {
                particles.gameObject.SetActive(true);
            }
            else
            {
                particles.gameObject.SetActive(false);
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();
        CanCast = false;
    }

    protected override void Start()
    {
        base.Start();
    }


    public override void AbilityUpdate()
    {
        base.AbilityUpdate();
    }

    public void Toggle()
    {
        if (AbilityActivated)
        {
            CanCast = !CanCast;
        }
    }

    protected override void CastAbilityImpl()
    {
        PVault();
    }

    private void PVault()
    {
        if (basicSlashRef.CanCast)
        {
            CanCast = false;
            isVaulting = true;

            bool willLaunch = false;

            basicSlashRef.noFullRecoil = true;

            Collider2D[] hitColliders = basicSlashRef.PSlashAttack();
            foreach (Collider2D collider in hitColliders)
            {
                if (collider.gameObject.CompareTag("Enemy"))
                {                   
                    willLaunch = true;
                    break;
                }
            }

            if (willLaunch)
            {
                lastHits.Clear();
                foreach (Collider2D collider in hitColliders)
                {
                    if (collider.gameObject.CompareTag("Enemy"))
                    {
                        lastHits.Add(collider);
                        aEntity.healthControl.ignoreColliders.Add(collider);
                    }
                }

                if (vaultEnu != null)
                {
                    StopCoroutine(vaultEnu);
                }

                vaultEnu = StartCoroutine(VaultEnumerator());
            }
            else
            {
                isVaulting = false;
                basicSlashRef.noFullRecoil = false;
            }
        }
    }

    Coroutine vaultEnu;

    public override void Cancel()
    {
        base.Cancel();

        if (isVaulting)
        {
            if (vaultEnu != null)
            {
                StopCoroutine(vaultEnu);
            }

            if (spawnedIndicator != null)
            {
                basicSlashRef.noFullRecoil = false;
                Destroy(spawnedIndicator);
                GameController.instance.ReturnToRegularTime();
            }

            foreach (Collider2D col in lastHits)
            {
                if (aEntity.healthControl.ignoreColliders.Contains(col))
                {
                    aEntity.healthControl.ignoreColliders.Remove(col);
                }
            }
            lastHits.Clear();
        }

        isVaulting = false;
    }

    private IEnumerator VaultEnumerator()
    {

        spawnedIndicator = Instantiate(indicatorPrefab, transform.position, Quaternion.identity);
        GameController.instance.TimeScaleSlowDown(0.1f, frozenTime, 0.2f);

        CinemachineFramingTransposer vir = CameraManager.instance.currentCam.GetCinemachineComponent<CinemachineFramingTransposer>();
        Camera main = Camera.main;
        Vector2 direction = Vector2.right;

        float timer = 0;
        while (timer < frozenTime)
        {
            Vector3 worldMousePosition = main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, vir.m_CameraDistance));
            direction = new Vector2(worldMousePosition.x - transform.position.x, worldMousePosition.y - transform.position.y).normalized;

            if(spawnedIndicator != null)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                spawnedIndicator.transform.position = transform.position;
                spawnedIndicator.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
            }

            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        if(spawnedIndicator != null)
        {
            Destroy(spawnedIndicator);
        }

        Debug.DrawLine(transform.position, new Vector3(direction.x, direction.y, 0) + transform.position, Color.blue, 1f);


        basicSlashRef.noFullRecoil = false;
        onVaultSuccess?.Invoke();
        isVaulting = false; //Vault needs to end by this point or else it will be cancelled when Dash activates.


        dashRef.DashInitiate2(direction, vaultSpeed, vaultTime);

        yield return new WaitForSeconds(vaultTime + 0.25f);

        foreach (Collider2D col in lastHits)
        {
            if (aEntity.healthControl.ignoreColliders.Contains(col))
            {
                aEntity.healthControl.ignoreColliders.Remove(col);
            }
        }
        lastHits.Clear();
    }
}
