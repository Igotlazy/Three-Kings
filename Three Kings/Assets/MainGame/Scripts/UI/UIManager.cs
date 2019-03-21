using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public HealthUI healthAndEnergyUI;
    public TextBoxUI textBoxUI;

    [Header("Fading")]
    public Image Fader;
    bool faded;
    bool fading;
    public float fadeTime = 1f;
    Color32 FadedColor = new Color32(0, 0, 0, 255);
    Color32 NonFadedColor = new Color32(0, 0, 0, 0);

    [Header("Wallet")]
    public TextMeshProUGUI moneyValue;


    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        GameController.instance.walletManager.MoneySetEvent += SetMoneyValue;
    }

    private void Update()
    {

    }

    public void ToggleFade()
    {
        if (!fading)
        {
            if (currentFade != null)
            {
                StopCoroutine(currentFade);
            }
            if (!faded)
            {
                currentFade = FadeToBlack();
            }
            else
            {
                currentFade = FadeToScreen();
            }
            StartCoroutine(currentFade);
        }

    }
    IEnumerator currentFade;

    private IEnumerator FadeToBlack()
    {
        Debug.Log("Fade");
        fading = true;
        float inter = 0;

        while(inter < 1)
        {
            inter += Time.deltaTime / fadeTime;
            Fader.color = Color.Lerp(NonFadedColor, FadedColor, inter);
            yield return null;
        }

        Fader.color = FadedColor;
        faded = true;
        fading = false;
    }

    private IEnumerator FadeToScreen()
    {
        Debug.Log("UnFade");
        fading = true;
        float inter = 0;

        while (inter < 1)
        {
            inter += Time.deltaTime / fadeTime;
            Fader.color = Color.Lerp(FadedColor, NonFadedColor, inter);
            yield return null;
        }

        Fader.color = NonFadedColor;
        faded = false;
        fading = false;
    }

    private void SetMoneyValue(int newValue)
    {
        moneyValue.text = newValue.ToString();
    }

}
