using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTransitioner : MonoBehaviour
{
    [Header("Personal Data:")]
    public int exitNumber;

    [Header("Transition To:")]
    public AreaCode areaCode;

    [Header("References:")]
    public Transform exit;
    private bool isUpOrDown = false;
    Vector3 moveVector;
    BoxCollider2D boxCollider;

    static FloatModifier controller;

    public StateSetter transitionStartState;
    public StateSetter transitionStartStateUp;


    private void Awake()
    {
        if(controller == null)
        {
            controller = new FloatModifier(0, FloatModifier.FloatModType.Flat, this);
        }

        transitionStartState = new StateSetter(this, TransitionStartSetUp, null, Player.instance.BaseActionUpdate, Player.instance.BaseActionFixedUpdate, TransitionCancel, StateSetter.SetStrength.Strong);
        transitionStartStateUp = new StateSetter(this, TransitionStartSetUpUpward, null, null, null, TransitionCancel, StateSetter.SetStrength.Strong);

        boxCollider = GetComponent<BoxCollider2D>();

        
        PositionExit();
        OrientMoveVector();
    }

    private void Start()
    {
        GameController.instance.sceneTransitionManager.OnNewSceneEvent += LoadToManager;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == Player.instance.gameObject)
        {
            GameController.instance.sceneTransitionManager.TransitionerSceneChange(areaCode);

            if (moveVector.Equals(Vector3.up))
            {
                Player.instance.SetLivingEntityState(transitionStartStateUp, false);

            }
            else
            {
                Player.instance.SetLivingEntityState(transitionStartState, false);
            }
        }
    }

    private void TransitionStartSetUp()
    {
        Player.instance.InputAndPhysicsCleanUp();
        boxCollider.enabled = false;

        Player.instance.hurtBox.enabled = false;

        controller.ModifierValue = Player.instance.InputMultiplier * moveVector.x * 0.9f;
        Player.instance.InputVector.X.AddSingleModifier(controller);

        Player.instance.InputAndPhysicsCleanUp();
    }

    private void TransitionStartSetUpUpward()
    {
        boxCollider.enabled = false;

        Player.instance.hurtBox.enabled = false;

        Player.instance.InputAndPhysicsCleanUp();
        Player.instance.EntityRB2D.bodyType = RigidbodyType2D.Kinematic;
        Player.instance.EntityRB2D.velocity = new Vector2(0f, 8f); //<-- errrrrrrrrrrr
    }

    private void TransitionCancel()
    {
        //Player.instance.hurtBox.enabled = true;
        Player.instance.OutsideVelVector.X.RemoveSingleModifier(controller, false);
    }

    public void LoadToManager()
    {
        GameController.instance.sceneTransitionManager.sceneTransitioners.Add(this);
    }

    private void OrientMoveVector()
    {
        if (!isUpOrDown)
        {
            if (exit.position.x >= transform.position.x)
            {
                moveVector = Vector3.right;
            }
            else
            {
                moveVector = Vector3.left;
            }
        }
        else
        {
            if (exit.position.y >= transform.position.y)
            {
                moveVector = Vector3.up;
            }
            else
            {
                moveVector = Vector3.down;
            }
        }
    }
    
    private void PositionExit()
    {
        RaycastHit2D hit = Physics2D.Raycast(exit.position, Vector2.down);
        exit.position = new Vector3(exit.position.x, hit.point.y + Player.instance.EntityBC2D.size.y / 2 + 0.1f, 0f);
    }



    public IEnumerator EnterPlayerToScene()
    {
        boxCollider.enabled = false;

        Player.instance.transform.position = exit.transform.position;
        Player.instance.InputAndPhysicsCleanUp();
        Player.instance.transform.position = exit.transform.position;
        controller.ModifierValue = 0;

        yield return new WaitForSeconds(1);

        if (!isUpOrDown)
        {
            controller.ModifierValue = Player.instance.InputMultiplier * -moveVector.x * 0.9f;
        }
        else
        {
            if (moveVector.Equals(Vector3.up))
            {
                Player.instance.SetLivingEntityState(transitionStartStateUp, false);
                Player.instance.InputVector.X.RemoveSingleModifier(controller, false);
            }
        }

        float enterTimer = 0;
        while (enterTimer < 0.7f)
        {
            enterTimer += Time.deltaTime;
            yield return null;
        }

        Player.instance.InputVector.X.RemoveSingleModifier(controller, false);
        Player.instance.hurtBox.enabled = true;

        Player.instance.EntityRB2D.bodyType = RigidbodyType2D.Dynamic;
        Player.instance.OriginalStateSet();

        boxCollider.enabled = true;
    }

    private void OnDestroy()
    {
        GameController.instance.sceneTransitionManager.OnNewSceneEvent -= LoadToManager;
        if (!boxCollider.enabled)
        {
            boxCollider.enabled = true;
        }
    }

    [System.Serializable]
    public struct AreaCode
    {
        public string areaName;
        public int sceneNumber;
        public int exitNumber;

        public string GiveSceneName()
        {
            return areaName + " " + sceneNumber;
        }
    }


}
