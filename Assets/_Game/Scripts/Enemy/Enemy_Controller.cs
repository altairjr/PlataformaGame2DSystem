using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Controller : MonoBehaviour
{
    // Parametros de status base
    [Header("Set velocity for enemy"), Range(0.1f, 10f), SerializeField] private float speed_ = 4.5f;
    [Header("Set 'Life' and 'Damege' for enemy"), Range(1f, 100f), SerializeField] private float life_ = 50f;

    // Parametros para identificar o player
    [Header("Set layer obstacles and player for fiew"), SerializeField] private LayerMask obstacles_;
    [Space] [SerializeField] private LayerMask playerMask_;
    [Header("Set range view"), Range(0.1f, 10f), SerializeField] private float rangeView_ = 2f; 

    // Parametros e components para configurar os waypoints
    [Header("Config waypoints walk"), SerializeField] private Vector2[] waypoints;
    [Header("Set object for check waypoint"), SerializeField] private GameObject object_;
    [Header("List waypoints check"), SerializeField] private Transform list_;

    // Parametros do sprite
    [Header("Set component sprite 'SpriteRenderer' "), SerializeField] private SpriteRenderer spriteRenderer_;

    // Parametros de animação
    [Header("Set component 'Animator' "), SerializeField] private Animator animator_;
    [Header("Set parameters for change animator controller")]
    [SerializeField] private string animator_WALK;
    [SerializeField] private string animator_DEAD;
    [SerializeField] private string animator_AfterDEAD;
    [SerializeField] private string animator_HIT;
    [SerializeField] private string animator_ATTACK;

    //State Enemy
    private enum state { patroll, attack}
    private state state_ = state.patroll;

    // Parametros internos de movimentação
    private float realSpeed_ = 0f;
    private float speedAfterWait_ = 0f;
    private bool wait_ = false;
    private float timeWaiting_ = 0f;
    private Vector2 movement_ = Vector2.zero;

    // Life
    private float realLife_ = 0f;
    private bool isDeath_ = false;
    private float timeAfterDeath_ = 0f;

    // view player
    private GameObject playerTarget_ = null;

    //Attack
    private bool attackPlayer_ = false;

    // Definindo componentes
    private Rigidbody2D rigidbody2D_;
    private BoxCollider2D boxCollider2D_;

    private void Awake()
    {
        realLife_ = life_;
        speedAfterWait_ = speed_;
        timeWaiting_ = Random.Range(2f, 5f);
        rigidbody2D_ = GetComponent<Rigidbody2D>();
        boxCollider2D_ = GetComponent<BoxCollider2D>();

        InstatiateWaypoints();
    }

    private void Update()
    {
        AfterDeath();

        if (isDeath_)
            return;

        if (state_ == state.patroll)
            CheckMovePatroll();
        else
        {
            CheckMoveAttack();
            MoveAttack();
        }

        CheckViewPlayer();
        AnimationController();
    }

    private void FixedUpdate()
    {
        if (isDeath_)
            return;

        if (state_ == state.patroll)
            MovePatroll();
    }

    #region Damage Controll

    public void TakeDamage(float damage)
    {
        if (isDeath_)
            return;

        realLife_ -= damage;
        realSpeed_ = 0f;
        animator_.SetTrigger(animator_HIT);
        if (realLife_ <= 0)
        {
            realLife_ = 0;
            Death();
        }
    }

    private void Death()
    {
        isDeath_ = true;
        ChangeAnimationBool(animator_DEAD, true);
        rigidbody2D_.isKinematic = true;
        rigidbody2D_.velocity = Vector2.zero;
        boxCollider2D_.enabled = false;
    }

    private void AfterDeath()
    {
        if (isDeath_)
        {
            timeAfterDeath_ += Time.deltaTime;
            if(timeAfterDeath_ >= 1f)
            {
                timeAfterDeath_ = 0;
                ChangeAnimationTrigger(animator_AfterDEAD);
                this.enabled = false;
            }
        }
    }

    #endregion

    #region Enemy Move

    private void CheckMoveAttack()
    {
        realSpeed_ = 0f;
        movement_ = Vector2.zero;
    }

    private void CheckMovePatroll()
    {
        if (wait_)
        {
            timeWaiting_ -= Time.deltaTime;
            realSpeed_ = 0f;
            if(timeWaiting_ <= 0)
            {
                timeWaiting_ = Random.Range(1.5f, 3f);
                wait_ = false;
            }
        }
        else
        {
            realSpeed_ = speedAfterWait_;
            if (movement_.x > 0)
                transform.rotation = Quaternion.Euler(0, 180f, 0);
            else
                transform.rotation = Quaternion.Euler(0, 0f, 0);
        }
    }

    private void MovePatroll()
    {
        movement_ = new Vector2(realSpeed_, rigidbody2D_.velocity.y);
        rigidbody2D_.velocity = movement_;
    }

    private void MoveAttack()
    {
        transform.position = Vector2.MoveTowards(transform.position, playerTarget_.transform.position, 1f * Time.deltaTime);
    }

    #endregion

    #region Trigger Controller

    private void ManagerTrigger(GameObject obj)
    {
        if(state_ == state.patroll)
        {
            if (obj.CompareTag("waypoint_enemy"))
            {
                wait_ = true;
                if (realSpeed_ > 0)
                {
                    realSpeed_ = (-1) * realSpeed_;
                    speedAfterWait_ = realSpeed_;
                }
                else
                {
                    realSpeed_ = (-1) * realSpeed_;
                    speedAfterWait_ = realSpeed_;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ManagerTrigger(collision.gameObject);
    }

    #endregion

    #region Animator Controller

    private void AnimationController()
    {
        if(animator_WALK != string.Empty)
        {
            if (movement_.x > 0 || movement_.x < 0)
                ChangeAnimationBool(animator_WALK, true);
            else
                ChangeAnimationBool(animator_WALK, false);
        }
    }

    private void ChangeAnimationBool(string parameters, bool state)
    {
        animator_.SetBool(parameters, state);
    }

    private void ChangeAnimationTrigger(string parameters)
    {
        animator_.SetTrigger(parameters);
    }

    #endregion

    #region Waypoints Controller

    private void InstatiateWaypoints()
    {
        if (waypoints.Length > 0)
        {
            for (int i = 0; i < waypoints.Length; i++)
            {
                Object.Instantiate(object_, waypoints[i], Quaternion.identity, list_);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (waypoints.Length > 0 && object_ != null)
        {
            for (int i = 0; i < waypoints.Length; i++)
            {
                Gizmos.DrawWireCube(new Vector3(waypoints[i].x, waypoints[i].y, 0f), new Vector3(object_.transform.localScale.x, object_.transform.localScale.y, 0));
            }
        }
    }

    #endregion

    #region System check visible player

    private void CheckViewPlayer()
    {
        Collider2D hit2d = Physics2D.OverlapCircle(transform.position, rangeView_, playerMask_);
        if (hit2d == null)
        {
            state_ = state.patroll;
            playerTarget_ = null;
            return;
        }

        playerTarget_ = hit2d.gameObject;

        if (CheckObstacles())
            return;

        state_ = state.attack;
    }

    private bool CheckObstacles()
    {
        RaycastHit2D hit2D = Physics2D.Linecast(transform.position, playerTarget_.transform.position, obstacles_);
        return hit2D.collider != null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, rangeView_);

        if (playerTarget_ == null)
            return;

        if (CheckObstacles())
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, playerTarget_.transform.position);
        }
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, playerTarget_.transform.position);
        }

    }

    #endregion
}
