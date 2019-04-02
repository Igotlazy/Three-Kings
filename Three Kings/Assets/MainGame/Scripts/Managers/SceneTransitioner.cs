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

    public StateSetter transitionState;
    public StateSetter transitionStateUp;


    private void Awake()
    {
        if(controller == null)
        {
            controller = new FloatModifier(0, FloatModifier.FloatModType.Flat, this);
        }

        boxCollider = GetComponent<BoxCollider2D>();

        transitionState = new StateSetter(this, null, Player.instance.BaseActionUpdate, Player.instance.BaseActionFixedUpdate, StateSetter.SetStrength.Strong);
        transitionStateUp = new StateSetter(this, null, null, null, StateSetter.SetStrength.Strong);

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
            boxCollider.enabled = false;

            Player.instance.hurtBox.enabled = false;
            if (moveVector.Equals(Vector3.up))
            {
                Player.instance.SetLivingEntityState(transitionStateUp, false);
                Player.instance.InputAndPhysicsCleanUp();
                Player.instance.entityRB2D.bodyType = RigidbodyType2D.Kinematic;
                Player.instance.entityRB2D.velocity = new Vector2(0f, 8f);

            }
            else
            {
                Player.instance.SetLivingEntityState(transitionState, false);

                controller.ModifierValue = Player.instance.InputMultiplier * moveVector.x * 0.9f;
                Player.instance.outsideSourceSpeed.AddSingleModifier(controller);

                if (!Player.instance.isLookingRight && moveVector.Equals(Vector3.right))
                {
                    Player.instance.EntityFlip();
                }
                if (Player.instance.isLookingRight && moveVector.Equals(Vector3.left))
                {
                    Player.instance.EntityFlip();
                }

                Player.instance.InputAndPhysicsCleanUp();
            }

        }
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
        exit.position = new Vector3(exit.position.x, hit.point.y + Player.instance.entityBC2D.size.y / 2 + 0.1f, 0f);
    }

    public IEnumerator EnterPlayerToScene()
    {
        boxCollider.enabled = false;

        Player.instance.transform.position = exit.transform.position;
        Player.instance.InputAndPhysicsCleanUp();
        Player.instance.outsideSourceSpeed.RemoveAllModifiers();

        yield return new WaitForSeconds(1);

        if (!isUpOrDown)
        {
            Player.instance.SetLivingEntityState(transitionState, false);
            Debug.Log("Heelo");

            if (Player.instance.isLookingRight && moveVector.Equals(Vector3.right))
            {
                Player.instance.EntityFlip();
            }
            if (!Player.instance.isLookingRight && moveVector.Equals(Vector3.left))
            {
                Player.instance.EntityFlip();

            }

            controller.ModifierValue = Player.instance.InputMultiplier * -moveVector.x * 0.9f;
            Player.instance.outsideSourceSpeed.AddSingleModifier(controller);
            Debug.Log("Did it " + controller.ModifierValue);
        }
        else
        {
            if (moveVector.Equals(Vector3.up))
            {
                Player.instance.SetLivingEntityState(transitionState, false);
            }
        }

        float enterTimer = 0;
        while (enterTimer < 0.7f)
        {
            enterTimer += Time.deltaTime;
            yield return null;
        }


        Player.instance.outsideSourceSpeed.RemoveAllModifiers();
        Player.instance.hurtBox.enabled = true;

        Player.instance.entityRB2D.bodyType = RigidbodyType2D.Dynamic;
        Player.instance.OriginalStateSet();

        boxCollider.enabled = true;
    }

    private void OnDestroy()
    {
        GameController.instance.sceneTransitionManager.OnNewSceneEvent -= LoadToManager;
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
