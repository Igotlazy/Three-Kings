using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MiniRespawnSetter : MonoBehaviour
{
    public LayerMask hitMask;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject == Player.instance.gameObject)
        {
            RaycastHit2D hitRay = Physics2D.Raycast(transform.position, Vector2.down, 20f, hitMask);
            if(hitRay)
            {
                GameController.instance.respawnManager.miniRespawnData = new RespawnManager.RespawnData()
                {
                    sceneName = SceneManager.GetActiveScene().name,
                    respawnPosition = new Vector2(hitRay.point.x, hitRay.point.y + Player.instance.EntityBC2D.size.y / 2 + 0.1f)
                };

                Debug.Log("Set MiniRespawn");
            }
            else
            {
                Debug.LogError("Warning: MiniRespawnSetter couldnt find the ground");
            }


        }
    }
}
