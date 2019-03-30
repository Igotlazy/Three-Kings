using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class RespawnManager : MonoBehaviour
{
    public class RespawnData
    {
        public string sceneName;
        public Vector2 respawnPosition;

        public static RespawnData Clone(RespawnData given)
        {
            return new RespawnData()
            {
                respawnPosition = given.respawnPosition,
                sceneName = given.sceneName
            };
        }
    }

    public RespawnData miniRespawnData;
    private RespawnData majorRespawnData;
    public RespawnData MajorRespawnData
    {
        get
        {
            return majorRespawnData;
        }
        set
        {
            majorRespawnData = value;
            miniRespawnData = RespawnData.Clone(value);
        }
    }
    public Action playerRespawn;

    public StateSetter respawnState;

    // Start is called before the first frame update
    void Awake()
    {
        respawnState = new StateSetter(this, null, null, null, StateSetter.SetStrength.Strong);
        RespawnData newMajor = new RespawnData()
        {
            sceneName = SceneManager.GetActiveScene().name,
            respawnPosition = Player.instance.transform.position
        };
        MajorRespawnData = newMajor;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitiateMiniRespawn()
    {
        StartCoroutine(Respawn(miniRespawnData));
    }

    public void InitiateMajorRespawn()
    {
        StartCoroutine(Respawn(majorRespawnData));
    }

    IEnumerator Respawn(RespawnData areaCode)
    {
        playerRespawn?.Invoke(); //So things know that the player is respawning.

        Player.instance.DisableHitboxAndVisuals();
        Player.instance.SetLivingEntityState(respawnState, false);
        Player.instance.InputAndPhysicsCleanUp();
        Player.instance.entityRB2D.bodyType = RigidbodyType2D.Kinematic;

        yield return new WaitForSeconds(1);
        UIManager.instance.ToggleFade();

        yield return new WaitForSeconds(UIManager.instance.fadeTime);

        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.name.Equals(areaCode.sceneName))
        {
            SceneManager.LoadScene(areaCode.sceneName);
            yield return new WaitUntil(() => activeScene.isLoaded);
        }

        Player.instance.transform.position = areaCode.respawnPosition;
        if(areaCode == miniRespawnData)
        {
            Player.instance.healthControl.ProcInvincibility();
        }
        if(areaCode == majorRespawnData)
        {
            miniRespawnData = RespawnData.Clone(areaCode);

            Player.instance.healthControl.CurrentHealth = Player.instance.healthControl.MaxHealth;
        }

        yield return new WaitForSeconds(1);
        Player.instance.EnableHitboxAndVisuals();

        UIManager.instance.ToggleFade();

        Player.instance.entityRB2D.bodyType = RigidbodyType2D.Dynamic;
        Player.instance.OriginalStateSet();
    }
}
