using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SawController : MonoBehaviour
{
    public float speed = 4;
    public float interPauseTime;
    public AnimationCurve animCurve;
    public List<Transform> movePositions = new List<Transform>();

    [Header("References:")]
    public GameObject positionParent;
    public GameObject saw;

    Transform oldPos;
    Transform nextPos;
    int tracker;
    float lerper;
    float interCounter;

    private void Awake()
    {
        Transform[] pos = positionParent.GetComponentsInChildren<Transform>();
        foreach(Transform trans in pos)
        {
            if(trans != positionParent.transform)
            {
                movePositions.Add(trans);
            }
        }

        if(movePositions.Count < 2)
        {
            Debug.LogError("WARNING: SAW DOES NOT HAVE ENOUGH POSITIONS");
        }
        else
        {
            saw.transform.position = movePositions[tracker].position;
            oldPos = movePositions[tracker];
            nextPos = movePositions[tracker + 1];
        }

    }


    void Update()
    {
        if(interCounter > 0)
        {
            interCounter -= Time.deltaTime;
        }
        else
        {
            PositionMover();
        }
    }

    void PositionMover()
    {
        float curve = animCurve.Evaluate(lerper / (Vector3.Distance(oldPos.position, nextPos.position)));
        saw.transform.position = Vector3.Lerp(oldPos.position, nextPos.position, (lerper * speed * curve));
        lerper += Time.deltaTime;

        if(saw.transform.position == nextPos.position)
        {
            tracker++;
            if (tracker > movePositions.Count - 1)
            {
                tracker = 0;
            }
            oldPos = movePositions[tracker];

            if (tracker + 1 > movePositions.Count - 1)
            {
                nextPos = movePositions[0];
            }
            else
            {
                nextPos = movePositions[tracker + 1];
            }

            lerper = 0;
            interCounter = interPauseTime;
            
        }
    }

}
