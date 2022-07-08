using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warrior_controller : MonoBehaviour
{
    [Header("Set component warrior")]
    [SerializeField] private Animator animator_;
    [Space]
    [SerializeField] private SpriteRenderer spriteRenderer_;

    [Header("Set layer mask for checking ground")] [SerializeField] private LayerMask layerMask_;

    [Header("Set warrior life and damage")] [Space] [Range(1, 3)] [SerializeField] private int life_ = 3;
    [Space] [Range(1, 50)] [SerializeField] private int warriorDamageNormal_ = 10;

    [Header("Set speed warrior and Force Jump"), Range(0.1f, 10f)] [SerializeField] private float speed_ = 2f;

    [Space] [Range(0.1f, 20f)] [SerializeField] private float forceJump_ = 2f, ghostJump_;

    [Header("Set attack parameters"), Range(0.1f, 10f)] [SerializeField] private float coldDownAttack_ = 1f;
    [Space] [Range(0.1f, 10f)] [SerializeField] private float attackRange_ = 0.5f;
    [Space] [SerializeField] private Transform attackPoint_;
    [Space] [SerializeField] private LayerMask enemiesLayer_;

    //Move
    private float realSpeed_ = 0f;

    //Jump
    private bool canJump_ = false;
    private bool isJumping_ = false;
    private float ghostJumpTime_ = 0;
    private float returnJump_ = 0;

    //Fall
    private bool isFall_ = false;

    //Death

    //Blocking

    //Attack
    private bool canAttack_ = false;
    private bool returnMove_ = true;
    private bool isAttack_ = false;
    private bool attacked_ = false;
    private float timeForAttackAgain_ = 0f;
    private float timeReturnMove_ = 0.5f;

    //Parameters animator
    private string animatorState_RUNING = "IsRunning";
    private string animatorState_JUMPING = "IsJumping";
    private string animatorState_FALL = "IsFall";
    private string animatorState_ATTACK = "IsAttack";

    private Rigidbody2D rigidbody2D_;
    private Warrior_input warrior_Input_;
    private BoxCollider2D BoxCollider2D_;

    private void Awake()
    {
        rigidbody2D_ = GetComponent<Rigidbody2D>();
        warrior_Input_ = GetComponent<Warrior_input>();
        BoxCollider2D_ = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        realSpeed_ = speed_;
    }
    private void Update()
    {
        AnimatorController();
        CheckGroundedForJump();
        Attacking();
    }

    private void FixedUpdate()
    {
        MoveWarrior();
    }

    #region Controller Warrior

    private void MoveWarrior()
    {
        Vector2 _movement = new Vector2(warrior_Input_.GetMovement_.x * realSpeed_, rigidbody2D_.velocity.y);
        rigidbody2D_.velocity = _movement;

        if (warrior_Input_.getPressJump_ && canJump_ && !warrior_Input_.getPressAttack_ && returnMove_)
            Jump();
    }

    private void Jump()
    {
        canJump_ = false;
        isFall_ = false;
        isJumping_ = true;
        Vector2 _forceJump = new Vector2(rigidbody2D_.velocity.x, forceJump_);
        rigidbody2D_.AddForce((_forceJump),ForceMode2D.Impulse);
    }

    private void Attacking()
    {
        if (!isFall_)
            canAttack_ = true;
        else
            canAttack_ = false;

        if (canAttack_ && warrior_Input_.getPressAttack_ && !attacked_)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint_.position, attackRange_, enemiesLayer_);

            foreach (Collider2D enemy in hitEnemies)
            {
                enemy.GetComponent<Enemy_Controller>().TakeDamage(warriorDamageNormal_);
            }

            isAttack_ = true;
            attacked_ = true;
        }
        else
            isAttack_ = false;

        if (attacked_)
        {
            realSpeed_ = 0;
            returnMove_ = false;
            timeReturnMove_ -= Time.deltaTime;
            if(timeReturnMove_ <= 0)
            {
                timeReturnMove_ = 0f;
                realSpeed_ = speed_;
                returnMove_ = true;
            }

            timeForAttackAgain_ += Time.deltaTime;
            if(timeForAttackAgain_>= coldDownAttack_)
            {
                timeForAttackAgain_ = 0;
                timeReturnMove_ = 0.5f;
                attacked_ = false;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint_ == null)
            return;

        Gizmos.DrawWireSphere(attackPoint_.position, attackRange_);
    }

    #endregion

    #region Animator Controller

    private void AnimatorController()
    {
        // Flip character
        if (warrior_Input_.GetMovement_.x < 0 && realSpeed_ > 0)
            gameObject.transform.rotation = Quaternion.Euler(0, 180, 0);

        if (warrior_Input_.GetMovement_.x > 0 && realSpeed_ > 0)
            gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);

        // Set condition for animator controller
        if (realSpeed_ > 0 && warrior_Input_.stateWarriorGrounded_ == Warrior_input.stateWarriorGrounded.IsRunning && !isJumping_)
            ChangeAnimatorState(animatorState_RUNING, true);

        if (realSpeed_ > 0 && warrior_Input_.stateWarriorGrounded_ == Warrior_input.stateWarriorGrounded.IsIdle && !isJumping_)
            ChangeAnimatorState(animatorState_RUNING, false);

        if(isJumping_)
            ChangeAnimatorState(animatorState_JUMPING, true);
        else
            ChangeAnimatorState(animatorState_JUMPING, false);

        if (isFall_)
            ChangeAnimatorState(animatorState_FALL, true);
        else
            ChangeAnimatorState(animatorState_FALL, false);

        if (isAttack_)
        {
            ChangeAnimatorState(animatorState_ATTACK, true);
            ChangeAnimatorState(animatorState_RUNING, false);
        }
        else
            ChangeAnimatorState(animatorState_ATTACK, false);
    }

    private void ChangeAnimatorState(string condition, bool state)
    {
        animator_.SetBool(condition, state);
    }

    #endregion

    #region Controll Collider

    private void CheckGroundedForJump()
    {
        if (IsGround())
        {
            returnJump_ -= Time.deltaTime;
            if(returnJump_ <= 0)
            {
                returnJump_ = 0;
                canJump_ = true;
            }
            isJumping_ = false;
            isFall_ = false;
            ghostJumpTime_ = ghostJump_;
        }
        else
        {
            returnJump_ = ghostJump_;
            ghostJumpTime_ -= Time.deltaTime;
            if(ghostJumpTime_<= 0)
            {
                ghostJumpTime_ = 0;
                canJump_ = false;
                isFall_ = true;
            }
        }
    }

    private bool IsGround()
    {
        RaycastHit2D hit2D = Physics2D.BoxCast(BoxCollider2D_.bounds.center, BoxCollider2D_.bounds.size, 0, Vector2.down, 0.1f, layerMask_);
        return hit2D.collider != null;
    }

    private void ManagerCollider(GameObject obj)
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ManagerCollider(collision.gameObject);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        ManagerCollider(collision.gameObject);
    }

    #endregion
}
