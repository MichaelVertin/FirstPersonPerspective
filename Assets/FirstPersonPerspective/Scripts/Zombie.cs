using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Editor;

public class Zombie : MonoBehaviour
{
    private Animator animationCtrl;
    public NavMeshAgent agent;
    [SerializeField] private int damage = 10;
    [SerializeField] private int __initialHealth = 50;
    private int __health;
    [SerializeField] private float DISTANCE_TO_TRACK_PLAYER = 10f;
    [SerializeField] private float DISTANCE_TO_STOP_TRACKING_PLAYER = 30f;
    private HealthBar healthBar;

    private bool FollowingPlayer = false;

    public int Health
    {
        get { return __health; }
        set
        {
            __health = value;
            healthBar.HealthRatio = (float)__health / (float)__initialHealth; 
            if (__health <= 0)
            {
                Destroy(this.gameObject);
            }
        }
    }

    private void Awake()
    {
        animationCtrl = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        healthBar = GetComponentInChildren<HealthBar>();
        Health = __initialHealth;
    }

    public void SetPosition(Transform targetTransform)
    {
        // try to move onto navmesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetTransform.position, out hit, 5.0f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
    }

    void FixedUpdate()
    {
        // when attacking, give animation full control
        if (this.Attacking) return;

        // set path to player
        SetPathToPlayer();

        // attack any player in range
        //  - wait for player to be comfortably in range(75%) before initializing attack
        Player_Zombie playerInRange = GetPlayerInRange(rangeDistanceScaler: .75f);
        if(playerInRange != null)
        {
            AttackPlayer(playerInRange);
        }
        else
        {
            float dist = agent.remainingDistance;
            if (agent.remainingDistance == 0)
            {
                animationCtrl.SetBool("isMoving", false);
            }
            else
            {
                animationCtrl.SetBool("isMoving", true);
            }
        }
    }

    #region Pathing
    public void SetTarget(Vector3 targetPos)
    {
        agent.SetDestination(targetPos);
    }

    private void SetPathToPlayer()
    {
        Player_Zombie player = Player_Zombie.instance;
        float distToPlayer = (player.transform.position - this.transform.position).magnitude;
        if(distToPlayer <= DISTANCE_TO_TRACK_PLAYER)
        {
            agent.SetDestination(player.transform.position);
            FollowingPlayer = true;
        }
        else if(distToPlayer <= DISTANCE_TO_STOP_TRACKING_PLAYER && FollowingPlayer)
        {
            agent.SetDestination(player.transform.position);
        }
        else
        {
            if(FollowingPlayer)
            {
                SetTarget(this.transform.position);
                FollowingPlayer = false;
            }
        }
    }
    #endregion

    #region Range
    [SerializeField] private float rangeHeight = 2f;
    [SerializeField] private float rangeWidth = 1f;
    [SerializeField] private float rangeDistance = 2f;
    [SerializeField] private LayerMask characterMask;
    // returns a player in the range of the zombie, otherwise null
    // rangeDistanceScaler: multiplies rangeDistance by rangeDistanceScaler
    //   to shorten/expand the volume to check
    private Player_Zombie GetPlayerInRange(float rangeDistanceScaler = 1.0f)
    {
        // prevent attacking before reached player (ie attacking through walls, ...)
        if (agent.remainingDistance > rangeDistance * rangeDistanceScaler) return null;

        // calculate center or range box
        Vector3 center = transform.position + transform.forward * (rangeDistance * rangeDistanceScaler * 0.5f);
        center.y += rangeHeight * 0.5f;

        // halfExtents: half the range's magnitude
        Vector3 halfExtents = new Vector3(rangeWidth * 0.5f, rangeHeight * 0.5f, rangeDistance * rangeDistanceScaler * 0.5f);

        // orient range in the direction of the Zombie
        Quaternion orientation = transform.rotation;

        // find all hits in the playerLayerMask
        Collider[] hits = Physics.OverlapBox(center, halfExtents, orientation, characterMask);

        foreach (var hit in hits)
        {
            Player_Zombie playerToHit = hit.GetComponent<Player_Zombie>();
            if (playerToHit != null)
            {
                return playerToHit;
            }
        }

        return null;
    }
    #endregion

    #region Attacking
    private bool Attacking = false;
    public void AttackPlayer(Player_Zombie targetPlayer)
    {
        // disable movement while attacking
        this.Attacking = true;
        agent.isStopped = true;
        animationCtrl.SetBool("isAttacking", true);
        animationCtrl.SetTrigger("attack");
    }

    // called from attack animation
    public void OnAttackEnd()
    {
        // reenable movement if player left range
        if (GetPlayerInRange() == null)
        {
            animationCtrl.SetBool("isAttacking", false);
            agent.isStopped = false;
            this.Attacking = false;
        }
    }

    // called from attack animation
    public void OnAttackEffect()
    {
        // ensure player is still in range (give 25% bonus range leeway)
        Player_Zombie playerToHit = GetPlayerInRange(rangeDistanceScaler: 1.25f);
        if (playerToHit != null)
        {
            // decrease player health
            playerToHit.Health -= damage;
        }
    }
    #endregion
}
