using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    
    public delegate void OnNewScene();
    public event OnNewScene OnNewSceneEvent;

    public List<SceneTransitioner> sceneTransitioners = new List<SceneTransitioner>();

    private void Awake()
    {
        
    }

    public void TransitionerSceneChange(SceneTransitioner.AreaCode areaCode)
    {
        StartCoroutine(TSceneChange(areaCode));
    }

    IEnumerator TSceneChange(SceneTransitioner.AreaCode areaCode)
    {
        UIManager.instance.ToggleFade();

        yield return new WaitForSeconds(UIManager.instance.fadeTime);


        SceneManager.LoadScene(areaCode.GiveSceneName());
        Scene activeScene = SceneManager.GetActiveScene();

        yield return new WaitUntil(() => activeScene.isLoaded);

        sceneTransitioners.Clear();
        OnNewSceneEvent?.Invoke();

        foreach (SceneTransitioner trans in sceneTransitioners)
        {
            if(trans.exitNumber == areaCode.exitNumber)
            {
                StartCoroutine(trans.EnterPlayerToScene());
                break;
            }
        }

        yield return new WaitForSeconds(1);

        UIManager.instance.ToggleFade();
    }

}
