using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

public class GameController : MonoBehaviour
{
    public static GameController instance;
    public WalletManager walletManager;
    public SceneTransitionManager sceneTransitionManager;
    public RespawnManager respawnManager;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this.gameObject);
            Debug.Log("GameController Destroyed");
        }
    }

    void Start()
    {

    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
            Debug.Log("Quit Program");
        }
    }

    bool time;


    public void TimeScaleSlowDown(float startSlow, float holdTime, float timeToRecover)
    {
        if (startSlow < 0)
        {
            startSlow = 0;
        }

        if (timeCoroutine != null)
        {
            StopCoroutine(timeCoroutine);
        }
        timeCoroutine = StartCoroutine(timeSlow(startSlow, holdTime, timeToRecover));
    }
    Coroutine timeCoroutine;

    private IEnumerator timeSlow(float startSlow, float holdTime, float timeToRecover)
    {
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(holdTime);

        float interTime = 0;
        float step = 0;

        Time.timeScale = startSlow;
        Time.fixedDeltaTime = 0.02F * Time.timeScale;

        yield return wait;

        while (interTime <= timeToRecover)
        {
            step = interTime / timeToRecover;
            Time.timeScale = Mathf.Lerp(startSlow, 1f, step);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;

            interTime += Time.unscaledDeltaTime;

            yield return null;
        }

        Time.timeScale = 1;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    public void ReturnToRegularTime()
    {
        if (timeCoroutine != null)
        {
            StopCoroutine(timeCoroutine);
        }

        Time.timeScale = 1;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }



}
