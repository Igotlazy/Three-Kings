using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HiddenAreaCover : MonoBehaviour
{
    public Tilemap sprite;
    public bool revealed;
    public float revealSpeed = 0.5f;
    public AudioSource soundEffect;

    float step;
    bool finishedReveal;
    

    // Start is called before the first frame update
    void Start()
    {
        soundEffect = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!finishedReveal && revealed)
        {
            float lerpAlpha = Mathf.Lerp(1, 0, step / revealSpeed);
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, lerpAlpha);
            step += Time.deltaTime;

            if(step > revealSpeed)
            {
                sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 0);
                finishedReveal = true;
            }
        }
        /*
        if (Input.GetKeyDown(KeyCode.I))
        {
            revealed = false;
            finishedReveal = false;
            step = 0;
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 1);
        }
        */
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!revealed && collision.gameObject == Player.instance.gameObject)
        {
            revealed = true;
            soundEffect.Play();
        }
    }
}
