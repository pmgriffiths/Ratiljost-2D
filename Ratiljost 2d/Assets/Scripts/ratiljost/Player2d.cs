using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.SceneManagement;

public enum MoveState
{
    Idle,
    Walk,
    Jump_up,
    Falling
}

public class Player2d : Trackable
{

    public Lamp lamp;

    public float playerSpeed = 2f;

    public float deadSpace = 0.02f;

    private Vector3 flippedScale = new Vector3(-1, 1, 1);

    /// <summary>
    /// Is the player dead ? 
    /// </summary>
    public bool dead;

    public BoxedFollowCam2d boxedFollowCam;

    /// <summary>
    /// How much upwards force to apply
    /// </summary>
    public float jumpFactor = 1f;

    private Quaternion forwardsQuot = Quaternion.Euler(0, 0, 0);
    private Quaternion backwardsQuot = Quaternion.Euler(0, 180, 0);

    public Rigidbody2D rigidbody2d;

    /// Player state variables
    public MoveState moveState;
    public bool groundedPlayer;

    public float speedIncrease;
    public float maxSpeed;


    public bool jumpRemaining = false;

    public Direction nextDirection = Direction.Forwards;

    public bool moving = false;
    public bool slowing = false;
    public float slowTime;
    public float minVelocity;

    public LayerMask groundedMask;
    public Vector2 groundCheckBox;

    public float cameraSize;

    private Rigidbody2D cafonoRigidBody;

    public Animator animator;

    private void Start()
    {
        // Assume player faces forwards
        
        facingDirection = Direction.Forwards;

        Debug.Assert(boxedFollowCam != null);


        cafonoRigidBody = GetComponent<Rigidbody2D>();
        Debug.Assert(rigidbody2d != null, "Need 2d rigidbody");

#if ENABLE_LEGACY_INPUT_MANAGER
        Debug.Assert(false, "Need new input manager enabled");
#endif

    }

    private void UpdateGrounded()
    {
        Collider2D possibleGround = Physics2D.OverlapBox(transform.position, groundCheckBox, 0f, groundedMask);
        groundedPlayer = possibleGround != null;

    }


    // Update is called once per frame
    void Update()
    {
        UpdateGrounded();

        float lampRotation = 0f;

        if (moving)
        {
            if (facingDirection != nextDirection)
            {
                SlowMovement(true);
                ChangeDirection(nextDirection);
            }

            KeepMoving(facingDirection == Direction.Forwards ? 1 : -1);
        } else if (slowing)
        {
            SlowMovement(false);
        }

        if (Mathf.Abs(lampRotation) > deadSpace && false)
        {
            bool lampLimit = lamp.DeflectLamp(-lampRotation, Time.deltaTime);

            if (lampLimit)
            {
                // noise
                Debug.Log("Lamp Limit");
            }
        }

        if (jumpRemaining)
        {
            jumpRemaining = false;
            if (groundedPlayer)
            {
                rigidbody2d.AddForce(new Vector2(0, jumpFactor), ForceMode2D.Impulse);
            }
        }

        UpdateAnimationState();

        /* if (Input.GetKeyDown(KeyCode.Q))
        {
            // return to menu
            SceneManager.LoadScene(0);

        } */

    }

    private void UpdateAnimationState()
    {
        animator.SetBool("grounded", groundedPlayer);
        animator.SetBool("walk", moving);
        animator.SetBool("airborne_up", rigidbody2d.velocity.y > 0);
    }


    private void KeepMoving(float lateralInput)
    {
        Vector2 velocity = rigidbody2d.velocity;

        float speedIncreasedBy = lateralInput * speedIncrease * Time.deltaTime;
        velocity.x += lateralInput * speedIncrease * Time.deltaTime;

        velocity.x = Mathf.Clamp(velocity.x, -maxSpeed, maxSpeed);

        rigidbody2d.velocity = velocity;

    }

    private void SlowMovement(bool stop)
    {
        Vector2 velocity = rigidbody2d.velocity;
        float x = velocity.x;

        if (stop)
        {
            x = 0f;
            slowing = false;
        }
        else
        {
            x -= velocity.x * slowTime * Time.deltaTime;
            if (Mathf.Abs(x) <= minVelocity)
            {
                slowing = false;
                x = 0f;
            }
        }
        velocity.x = x;

        // Assign back to the body.
        rigidbody2d.velocity = velocity;
    }

    private void ChangeDirection(Direction newDirection)
    {
        // flip 180 deg, unless already changing direction

        if (facingDirection != newDirection)
        {
            facingDirection = newDirection;
            transform.localScale = facingDirection == Direction.Forwards ? Vector3.one : flippedScale;
        }
    }

    /// <summary>
    /// Check for shards
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Shard"))
        {
            Shard shard = other.gameObject.GetComponent<Shard>();
            Debug.Log("Found a shard: " + shard.shardColour);

            lamp.AddColour(shard.shardColour);
            other.gameObject.SetActive(false);
        }
        
    }

    public void InputMove(InputAction.CallbackContext context)
    {
//        Debug.Log("Move " + context.ToString());

        if (context.action.name == "Move")
        {
            if (context.action.phase == InputActionPhase.Started)
            {
                Vector2 moveValue = context.ReadValue<Vector2>();
                if (moveValue == null)
                {
                    Debug.Log("Null move value " + context);
                }
                else
                {

//                    Debug.Log("Move " + moveValue + " type " + context.interaction);

                    if (moveValue.x != 0)
                    {
                        Debug.Log("Move " + moveValue);
                        moving = true;
                        nextDirection = moveValue.x > 0 ? Direction.Forwards : Direction.Backwards;
                    }
                }
            }
            else if (context.action.phase == InputActionPhase.Canceled)
            {
                Debug.Log("Stop moving");
                moving = false;
                slowing = true;
            }
        }
    }

    public void InputJump(InputAction.CallbackContext context)
    {
        Debug.Log("Jump " + context.ToString());
        if (!jumpRemaining)
        {
            jumpRemaining = true;
        }

    }

}
