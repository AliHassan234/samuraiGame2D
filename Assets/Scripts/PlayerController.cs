
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum Controls { mobile,pc}

public class PlayerController : MonoBehaviour
{


    public float moveSpeed = 5f;
    public float runMultiplier = 1.6f;
    public float jumpForce = 10f;
    public LayerMask groundLayer;
    public Transform groundCheck;

    private Rigidbody2D rb;
    private bool isGroundedBool = false;
    private bool leftPressedMobile = false;
    private bool rightPressedMobile = false;

    public Animator playeranim;

    public Controls controlmode;
   

    private float moveX;
    public bool isPaused = false;

    public ParticleSystem footsteps;
    private ParticleSystem.EmissionModule footEmissions;

    public ParticleSystem ImpactEffect;
    private bool wasonGround;


   // public GameObject projectile;
   // public Transform firePoint;

    public float fireRate = 0.5f; // Time between each shot
    private float nextFireTime = 0f; // Time of the next allowed shot
    private bool runPressedMobile = false;

    public Button Left;
    public Button Right;
    public Button JumpBTN;
    public Button Sprint;
    public Button AttackBTN;

    




    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        footEmissions = footsteps.emission;
        BindMobileButtons();

        if (controlmode == Controls.mobile)
        {
            UIManager.instance.EnableMobileControls();
        }


    }

    private void Update()
    {
        isGroundedBool = IsGrounded();
        SetAnimations();

        if (moveX != 0)
        {
            FlipSprite(moveX);
        }

        //impactEffect

        if(!wasonGround && isGroundedBool)
        {
            ImpactEffect.gameObject.SetActive(true);
            ImpactEffect.Stop();
            ImpactEffect.transform.position = new Vector2(footsteps.transform.position.x,footsteps.transform.position.y-0.2f);
            ImpactEffect.Play();
        }

        wasonGround = isGroundedBool;

        
    }
   
    public void SetAnimations()
    {
        bool isRunning = IsRunning();

        if (moveX != 0 && isGroundedBool)
        {
            playeranim.SetBool("walk", !isRunning);
            playeranim.SetBool("run", isRunning);

            footEmissions.rateOverTime = isRunning ? 50f : 35f;
        }
        else
        {
            playeranim.SetBool("walk", false);
            playeranim.SetBool("run", false);

            footEmissions.rateOverTime = 0f;
        }

        playeranim.SetBool("isGrounded", isGroundedBool);
    }

    private void FlipSprite(float direction)
    {
        if (direction > 0)
        {
            // Moving right, flip sprite to the right
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (direction < 0)
        {
            // Moving left, flip sprite to the left
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
    private void FixedUpdate()
    {
        // Player movement using mobile buttons only.
        if (leftPressedMobile && !rightPressedMobile)
        {
            moveX = -1f;
        }
        else if (rightPressedMobile && !leftPressedMobile)
        {
            moveX = 1f;
        }
        else
        {
            moveX = 0f;
        }


        float currentSpeed = IsRunning() ? moveSpeed * runMultiplier : moveSpeed;
        rb.velocity = new Vector2(moveX * currentSpeed, rb.velocity.y);
    }

    private void Jump(float jumpForce)
    {
        rb.velocity = new Vector2(rb.velocity.x, 0); // Zero out vertical velocity
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        playeranim.SetTrigger("jump");
    }

    private bool IsGrounded()
    {
        float rayLength = 0.25f;
        Vector2 rayOrigin = new Vector2(groundCheck.transform.position.x, groundCheck.transform.position.y - 0.1f);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayLength, groundLayer);
        return hit.collider != null;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "killzone")
        {
            GameManager.instance.Death();
        }
    }

    //mobile;
    public void MobileMove(float value)
    {
        moveX = value;
    }
    public void MobileJump()
    {
        if (isGroundedBool)
        {
            Jump(jumpForce);
        }
    }

    private bool IsRunning()
    {
        return runPressedMobile;
    }

    public void Attack()
    {
        if (Time.time < nextFireTime)
        {
            return;
        }

        nextFireTime = Time.time + 1f / fireRate; // Set the next allowed fire time
        playeranim.SetTrigger("attack");
        Debug.LogError("Attack Anim Played");
        // Keep old projectile logic hook.
        Shoot();
    }

    public void Shoot()
    {
        //GameObject fireBall = Instantiate(projectile, firePoint.position, Quaternion.identity);
        //fireBall.GetComponent<Rigidbody2D>().AddForce(firePoint.right * 500f);
    }

    public void MobileShoot()
    {
        Attack();
    }

    public void MobileAttack()
    {
        Attack();
    }

    public void MobileRunDown()
    {
        runPressedMobile = true;
    }

    public void MobileRunUp()
    {
        runPressedMobile = false;
    }

    public void OnLeftDown()
    {
        leftPressedMobile = true;
    }

    public void OnLeftUp()
    {
        leftPressedMobile = false;
    }

    public void OnRightDown()
    {
        rightPressedMobile = true;
    }

    public void OnRightUp()
    {
        rightPressedMobile = false;
    }

    private void BindMobileButtons()
    {
        if (JumpBTN != null)
        {
            JumpBTN.onClick.AddListener(MobileJump);
        }

        if (AttackBTN != null)
        {
            AttackBTN.onClick.AddListener(MobileAttack);
        }

        AddPointerEvents(Left, OnLeftDown, OnLeftUp);
        AddPointerEvents(Right, OnRightDown, OnRightUp);
        AddPointerEvents(Sprint, MobileRunDown, MobileRunUp);
    }

    private void AddPointerEvents(Button button, UnityEngine.Events.UnityAction onDown, UnityEngine.Events.UnityAction onUp)
    {
        if (button == null)
        {
            return;
        }

        EventTrigger trigger = button.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }

        AddTriggerEntry(trigger, EventTriggerType.PointerDown, onDown);
        AddTriggerEntry(trigger, EventTriggerType.PointerUp, onUp);
        AddTriggerEntry(trigger, EventTriggerType.PointerExit, onUp);
    }

    private void AddTriggerEntry(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction callback)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback.AddListener((_) => callback());
        trigger.triggers.Add(entry);
    }

}