using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HiddenArea : MonoBehaviour
{
    [Header("Properties:")]
    public float revealTime = 0.5f;
    public bool revealOnce;
    public Color startColor;
    public bool playRevealAudio = true;


    [Header("Feedback:")]
    [SerializeField] private bool disable;
    public bool Disable
    {
        get
        {
            return disable;
        }
        set
        {
            if (value && !disable)
            {
                RevealArea();
            }
            disable = value;
        }
    }

    [Header("References:")]
    public Tilemap sprite;
    private AudioSource soundEffect;

    float alphaTarget = 1;
    bool lerping;

    bool revealed;


    void Start()
    {
        SetUpHooks();

        soundEffect = GetComponent<AudioSource>();
        sprite.color = startColor;
    }

    void Update()
    {
        if (lerping)
        {
            if(sprite.color.a != alphaTarget)
            {
                float lerpAlpha = Mathf.MoveTowards(sprite.color.a, alphaTarget, Time.deltaTime / revealTime);

                sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, lerpAlpha);
            }
            else
            {
                sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, alphaTarget);
                lerping = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject == Player.instance.gameObject)
        {
            RevealArea();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == Player.instance.gameObject && !revealOnce)
        {
            HideArea();
        }
    }

    public void RevealArea()
    {
        if (!disable)
        {
            lerping = true;
            alphaTarget = 0;

            if (playRevealAudio)
            {
                soundEffect.Play();
            }

            if (revealOnce)
            {
                disable = true;
            }
        }
    }

    public void HideArea()
    {
        if (!disable)
        {
            lerping = true;
            alphaTarget = 1;
        }
    }

    protected virtual void SetUpHooks()
    {

    }
}
