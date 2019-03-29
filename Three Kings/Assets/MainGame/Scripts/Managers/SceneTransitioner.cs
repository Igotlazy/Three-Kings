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
    public bool isUpOrDown;
    Vector3 moveVector;
    BoxCollider2D boxCollider;

    FloatModifier controller;


    private void Awake()
    {
        controller = new FloatModifier(0, FloatModifier.FloatModType.Flat, this);
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
            boxCollider.enabled = false;

            Player.instance.hurtBox.enabled = false;
            Player.instance.EntityControlTypeSet(LivingEntity.ControlType.CannotControl, true);

            controller.ModifierValue = Player.instance.InputMultiplier * moveVector.x;
            Player.instance.outsideSourceSpeed.AddSingleModifier(controller);

            if (!Player.instance.isLookingRight && moveVector.Equals(Vector3.right))
            {
                Player.instance.EntityFlip();
            }
            if (Player.instance.isLookingRight && moveVector.Equals(Vector3.left))
            {
                Player.instance.EntityFlip();
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
        Player.instance.transform.position = exit.transform.position;
        Player.instance.entityRB2D.velocity = Vector2.zero;
        boxCollider.enabled = false;

        if(Player.instance.isLookingRight && moveVector.Equals(Vector3.right))
        {
            Player.instance.EntityFlip();
        }
        if (!Player.instance.isLookingRight && moveVector.Equals(Vector3.left))
        {
            Player.instance.EntityFlip();
        }

        //Player.instance.outsideVelocitySource = 0;
        Player.instance.outsideSourceSpeed.RemoveAllModifiers();
        yield return new WaitForSeconds(1);

        Player.instance.LockSmoothing = true;
        //Player.instance.outsideVelocitySource = Player.instance.InputMultiplier * -moveVector.x;
        controller.ModifierValue = Player.instance.InputMultiplier * -moveVector.x;
        Player.instance.outsideSourceSpeed.AddSingleModifier(controller);

        float enterTimer = 0;
        while (enterTimer < 0.625f)
        {
            enterTimer += Time.deltaTime;
            yield return null;
        }

        //Player.instance.outsideVelocitySource = 0;
        Player.instance.outsideSourceSpeed.RemoveAllModifiers();
        Player.instance.hurtBox.enabled = true;
        Player.instance.LockSmoothing = false;
        Player.instance.EntityControlTypeSet(LivingEntity.ControlType.CanControl, true);

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
