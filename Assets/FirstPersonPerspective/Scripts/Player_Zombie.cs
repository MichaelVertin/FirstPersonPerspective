using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.Android.Gradle.Manifest;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem;
using TMPro;
using Unity.VisualScripting;
using NUnit.Framework.Constraints;
using Unity.Collections;
using NUnit.Framework;
using System.Collections;
using UnityEngine.AI;

public class Player_Zombie : MonoBehaviour
{
    private Rigidbody rb;
    private CharacterController charCtrl;
    [SerializeField] float speed = 5.0f;
    [SerializeField] int maxHealth = 100;
    [SerializeField] TMP_Text healthField;
    [SerializeField] private int damage = 30; // TODO: replace with weapon's damage
    [SerializeField] EndGameManager endGameManager;
    int __health;
    private Animator animationCtrl;

    public static Player_Zombie instance = null;

    [SerializeField] private int HealthGain = 1;
    [SerializeField] private float TimeBetweenHealthGain = 1f;
    [SerializeField] private float TimeBeforeHealthGain = 5f;
    [SerializeField] GameObject instructions;
    private float lastHitTime = 0f;

    public int Health
    {
        get
        {
            return __health;
        }
        set
        {
            if (value < __health) lastHitTime = Time.time;
            __health = value;
            healthField.text = "Health: " + __health.ToString() + " / " + maxHealth.ToString();
            if(__health <= 0)
            {
                endGameManager.OnGameEnd(won: false);
            }
        }
    }

    Dictionary<string, Vector3> keyToDirDictionary = new Dictionary<string, Vector3>
    {
        { "a", new Vector3(-1f, 0f, 0f) },
        { "d", new Vector3( 1f, 0f, 0f) },
        { "w", new Vector3( 0f, 0f, 1f) },
        { "s", new Vector3( 0f, 0f, -1f) },
        { "o", new Vector3( 0f, 1f, 0f) },
        { "p", new Vector3( 0f, -1f, 0f) }
    };

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        charCtrl = GetComponent<CharacterController>();
        Health = maxHealth;
        animationCtrl = GetComponentInChildren<Animator>();

        if(instance == null)
        {
            Player_Zombie.instance = this;
        }
        else
        {
            Debug.LogError("Instantiated Multiple Player Instances");
        }
    }
    
    private void Start()
    {
        StartCoroutine(HealthRegenerationCR());
    }

    private void Update()
    {
        ApplyGravity();
        // Attack when Space is pressed // TODO: define Space as attribute
        if (InputManager.WasKeyPressedThisFrame(Key.Space))
        {
            AttackZombie(null);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (this.Attacking) return; // disable input when attacking

        // get direction vector from keyboard input
        Vector3 direction = Vector3.zero;
        foreach(var key in keyToDirDictionary.Keys)
        {
            if(InputManager.IsKeyPressed(key))
            {
                direction += keyToDirDictionary[key];
                instructions.SetActive(false);
            }
        }
        
        // direction: x between -1 and 1, y between -1 and 1
        Vector3 localDirection = transform.TransformDirection(direction);

        // sum of zero speed -> not moving
        if(localDirection == Vector3.zero)
        {
            // set animation to idling
            animationCtrl.SetBool("isMoving", false);
        }
        // sum of non-zero speed -> moving
        else
        {
            // normalize to ensure constant speed when multiple keys are pressed
            Vector3 moveVector = localDirection.normalized * speed;

            // set animation to moving
            animationCtrl.SetBool("isMoving", true);
            animationCtrl.SetFloat("moveX", direction.x);
            animationCtrl.SetFloat("moveY", direction.z);

            // move character controller
            charCtrl.Move(moveVector);
        }

        // rotation
        float rotationSpeed = 100f;
        if(InputManager.IsKeyPressed("n"))
        {
            transform.Rotate(Vector3.up * -rotationSpeed * Time.deltaTime);
        }
        if(InputManager.IsKeyPressed("m"))
        {
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
    }

    #region Gravity
    Vector3 velocity = Vector3.zero;
    [SerializeField] float gravityStrength = -5f;
    [SerializeField] float jumpHeight = 1.5f;
    private void ApplyGravity()
    {
        bool isGrounded = charCtrl.isGrounded;
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (InputManager.WasKeyPressedThisFrame(Key.J) && isGrounded) // TODO: move jump key to attribute
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityStrength);
        }

        velocity.y += gravityStrength * Time.deltaTime;
        charCtrl.Move(velocity * Time.deltaTime);
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
    private Zombie GetZombieInRange(float rangeDistanceScaler = 1.0f)
    {
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
            Zombie zombie = hit.GetComponent<Zombie>();
            if (zombie != null)
            {
                return zombie;
            }
        }

        return null;
    }
    #endregion

    #region Attacking
    private bool Attacking = false;
    public void AttackZombie(Zombie targetZombie)
    {
        if(!this.Attacking)
        {
            // disable movement while attacking
            this.Attacking = true;
            animationCtrl.SetBool("isAttacking", true);
            animationCtrl.SetTrigger("attack");
        }
    }

    // called from attack animation
    public void OnAttackEnd()
    {
        animationCtrl.SetBool("isAttacking", false);
        this.Attacking = false;
    }

    // called from attack animation
    public void OnAttackEffect()
    {
        // ensure player is still in range (give 25% bonus range leeway)
        Zombie zombieToHit = GetZombieInRange(rangeDistanceScaler: 1.25f);
        if (zombieToHit != null)
        {
            // decrease zombie health
            zombieToHit.Health -= damage;
        }
    }
    #endregion

    #region HealthRegen
    // gain HealthGain health every TimeBetweenHealthGain seconds TimeBeforeHealthGain seconds after being hit
    public IEnumerator HealthRegenerationCR()
    {
        yield return new WaitForEndOfFrame();
        while (true)
        {
            if (Time.time >= lastHitTime + TimeBeforeHealthGain)
            {
                Health = Math.Min(Health + HealthGain, maxHealth);
            }
            yield return new WaitForSeconds(TimeBetweenHealthGain);
        }
    }
    #endregion
}
