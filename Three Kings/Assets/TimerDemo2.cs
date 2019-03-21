using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerDemo2 : MonoBehaviour
{
    public TimerDemo timerDemo;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == Player.instance.hurtBox && timerDemo.isActive)
        {
            timerDemo.StopCounting();
        }
    }
}
