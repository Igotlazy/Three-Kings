using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;
    public CinemachineVirtualCamera currentCam;

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
        if(currentCam.m_Follow != Player.instance.transform)
        {
            currentCam.m_Follow = Player.instance.transform;
        }
    }

    public void CameraShake(Shake givenShake)
    {
        shakes.Add(givenShake);
        if (!shaking)
        {
            StartCoroutine(CameraShakeControl());
        }

        Debug.Log("SHAKE CMAERA");
    }

    bool shaking;


    IEnumerator CameraShakeControl()
    {
        shaking = true;

        CinemachineBasicMultiChannelPerlin perlin = currentCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        while (shakes.Count > 0)
        {
            float totalAmp = 0;
            float totalFreq = 0;

            for(int i = shakes.Count - 1; i >= 0; i--)
            {
                totalAmp += shakes[i].amplitude;
                totalFreq += shakes[i].frequency;
                shakes[i].duration -= Time.deltaTime;
                if(shakes[i].duration < 0)
                {
                    shakes.RemoveAt(i);
                }
            }

            perlin.m_AmplitudeGain = Mathf.Clamp(totalAmp, 0f, 5f);
            perlin.m_FrequencyGain = Mathf.Clamp(totalFreq, 0f, 5f);

            yield return null;
        }

        perlin.m_AmplitudeGain = 0;
        perlin.m_FrequencyGain = 0;

        shaking = false;
    }

    private List<Shake> shakes = new List<Shake>();

    public class Shake
    {
        public float amplitude;
        public float frequency;
        public float duration;

        public Shake(float _amplitude, float _frequency, float _duration)
        {
            amplitude = _amplitude;
            frequency = _frequency;
            duration = _duration;
        }

    }
}
