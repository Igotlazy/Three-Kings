using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;
    public CinemachineVirtualCamera currentCam;
    CinemachineBasicMultiChannelPerlin perlin;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            perlin = currentCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
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

    public void AddShake(Shake givenShake)
    {
        if(!shakes.Contains(givenShake))
        {
            shakes.Add(givenShake);
        }
        givenShake.timer = givenShake.duration;
    }


    private void Update()
    {
        float totalAmp = 0;
        float totalFreq = 0;

        if(shakes.Count > 0)
        {
            for (int i = shakes.Count - 1; i >= 0; i--)
            {
                totalAmp += shakes[i].amplitude;
                totalFreq += shakes[i].frequency;

                if (shakes[i].istimed)
                {
                    shakes[i].timer -= Time.deltaTime;
                    if (shakes[i].timer < 0)
                    {
                        shakes.Remove(shakes[i]);
                    }
                }
            }

            perlin.m_AmplitudeGain = Mathf.Clamp(totalAmp, 0f, 5f);
            perlin.m_FrequencyGain = Mathf.Clamp(totalFreq, 0f, 5f);
        }
        else
        {
            perlin.m_AmplitudeGain = 0;
            perlin.m_FrequencyGain = 0;
        }
    }

    private List<Shake> shakes = new List<Shake>();

    public bool RemoveShake(Shake shakeToRemove)
    {
        if (shakes.Remove(shakeToRemove))
        {
            return true;
        }
        return false;
    }

    public class Shake
    {
        public float amplitude;
        public float frequency;
        public float duration;
        public float timer;

        public bool istimed;

        public Shake(float _amplitude, float _frequency, float _duration)
        {
            amplitude = _amplitude;
            frequency = _frequency;
            istimed = true;
            duration = _duration;
            timer = duration;
        }

        public Shake(float _amplitude, float _frequency)
        {
            amplitude = _amplitude;
            frequency = _frequency;
        }

    }
}
