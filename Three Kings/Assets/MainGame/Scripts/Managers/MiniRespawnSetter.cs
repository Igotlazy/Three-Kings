using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MiniRespawnSetter : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject == Player.instance.gameObject)
        {
            GameController.instance.respawnManager.miniRespawnData = new RespawnManager.RespawnData()
            {
                sceneName = SceneManager.GetActiveScene().name,
                respawnPosition = transform.position
            };

            Debug.Log("Set MiniRespawn");
        }
    }
}
