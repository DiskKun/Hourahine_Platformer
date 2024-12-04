using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public GameObject shovelPrefab;
    public float projectileForce;

    LayerMask groundedMask;

    float xVelocity = 0;
    float yVelocity = 0;
    float jumpVelocity;
    float gravity;

    bool facingLeft;


    public float apexTime;
    public float apexHeight;
    public float terminalVelocity;
    public float coyoteTime;

    public int health = 10;

    float coyoteTimer;
    Rigidbody2D rb;

    public float horizontalMaxVelocity = 10;
    public float horizontalAcceleration = 1;
    public float horizontalDeceleration = 1;

    public float groundedRaycastLength;

    bool dashButton;
    bool shovelButton;
    float dashTimer;
    public float dashTime;
    public float dashStrength;

    Vector2 playerInputAxis;

    public enum FacingDirection
    {
        left, right
    }

    public enum CharacterState
    {
        idle, walk, jump, die
    }
    public CharacterState currentCharacterState = CharacterState.idle;
    public CharacterState previousCharacterState = CharacterState.idle;

    // Start is called before the first frame update
    void Start()
    {
        
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        groundedMask = LayerMask.GetMask("Ground");

        playerInputAxis = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        dashButton = Input.GetKey(KeyCode.Space);
        shovelButton = Input.GetKeyDown(KeyCode.S);

        if (shovelButton)
        {
            GameObject shovel = Instantiate(shovelPrefab, transform.position, Quaternion.identity);
            shovel.GetComponent<Rigidbody2D>().AddForce(projectileForce * new Vector2(xVelocity, yVelocity));
        }

        previousCharacterState = currentCharacterState;

        switch(currentCharacterState)
        {
            case CharacterState.die:

                break;
            case CharacterState.idle:
                //walk
                if (IsWalking())
                {
                    currentCharacterState = CharacterState.walk;
                }

                //jump
                if (!IsGrounded())
                {
                    currentCharacterState = CharacterState.jump;
                }
                break;
            case CharacterState.walk:
                //walk
                if (!IsWalking())
                {
                    currentCharacterState = CharacterState.idle;
                }

                //jump
                if (!IsGrounded())
                {
                    currentCharacterState = CharacterState.jump;
                }
                break;
            case CharacterState.jump:
                if (IsGrounded())
                {
                    if (IsWalking())
                    {
                        currentCharacterState = CharacterState.walk;
                    } else
                    {
                        currentCharacterState = CharacterState.idle;
                    }
                }

                break;
        }
        if (IsDead())
        {
            currentCharacterState = CharacterState.die;
        }

    }



    // Update is called once per frame
    void FixedUpdate()
    {
        gravity = -2 * apexHeight / Mathf.Pow(apexTime, 2);
        jumpVelocity = 2 * apexHeight / apexTime;
        //The input from the player needs to be determined and then passed in the to the MovementUpdate which should
        //manage the actual movement of the character.
        MovementUpdate(playerInputAxis);
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        // handle horizontal acceleration
        xVelocity += horizontalAcceleration * playerInput.x * Time.deltaTime;


        // apply friction
        if (playerInput.x == 0 && xVelocity != 0 && IsGrounded())
        {
            xVelocity += -(Mathf.Abs(xVelocity) / xVelocity) * horizontalDeceleration * Time.deltaTime;
            // stop player
            if (xVelocity > -1f && xVelocity < 1f)
            {
                xVelocity = 0;
            }
        }


        // when not dashing, and while in air, apply the max horizontal speed
        if (dashTimer < 0 || IsGrounded())
        {
            xVelocity = Mathf.Clamp(xVelocity, -horizontalMaxVelocity, horizontalMaxVelocity);

        }

        // if not on the ground, decrease the coyoteTimer. otherwise, set it to the starting timer value
        if (IsGrounded())
        {
            yVelocity = 0;
            coyoteTimer = coyoteTime;
        } else
        {
            coyoteTimer -= Time.deltaTime;
        }

        // terminal velocity changes depending on whether or not you are sliding against a wall
        float newTerminalVelocity;

        if (IsHittingWall())
        {
            newTerminalVelocity = terminalVelocity / 2;
        } else
        {
            newTerminalVelocity = terminalVelocity;
        }
        
        // apply terminal velocity
        if (yVelocity < newTerminalVelocity)
        {
            yVelocity = newTerminalVelocity;
        }


        // check to see if you can jump, and apply jumpvelocity if true
        // checks to see if up is being pressed, and the player is grounded OR if they coyotetimer hasn't ran out, OR if you are hitting a wall and sliding down it.
        if (playerInput.y == 1 && (IsGrounded() || coyoteTimer > 0 || (IsHittingWall() && yVelocity < 0)))
        {
            yVelocity = jumpVelocity;
            coyoteTimer = 0;
        }

        // decrease the dashtimer
        dashTimer -= Time.deltaTime;

        // apply the dash boost when you press the dash button
        if (dashButton && dashTimer <= 0)
        {
            dashTimer = dashTime;
            yVelocity = playerInput.y * dashStrength;
            xVelocity = xVelocity * dashStrength;
        }

        //move the player
        rb.MovePosition(rb.position + new Vector2(xVelocity, yVelocity) * Time.deltaTime);

        // apply gravity
        yVelocity += gravity * Time.deltaTime;


    }

    public void OnDeathAnimationComplete()
    {
        gameObject.SetActive(false);
    }

    public bool IsWalking()
    {
        if (xVelocity != 0)
        {
            return true;
        } else
        {
            return false;
        }
    }
    public bool IsGrounded()
    {
        // perform a raycast to see if the player is touching the ground.
        Debug.DrawLine(transform.position, transform.position + Vector3.down * groundedRaycastLength);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundedRaycastLength, groundedMask);
        if (hit.collider != null)
        {
            return true;
        } else
        {
            return false;
        }
    }

    public bool IsHittingWall()
    {
        //perform two raycasts (one left, one right) to see if the player is hitting a wall horizontally
        Debug.DrawLine(transform.position, transform.position + Vector3.right * groundedRaycastLength);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right, groundedRaycastLength, groundedMask);
        RaycastHit2D hit2 = Physics2D.Raycast(transform.position, Vector2.left, groundedRaycastLength, groundedMask);
        if (hit.collider != null || hit2.collider != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsDead()
    {
        if (health <= 0)
        {
            return true;
        } else
        {
            return false;
        }
    }

    public FacingDirection GetFacingDirection()
    {
        if (xVelocity > 0)
        {
            facingLeft = false;
            return FacingDirection.right;
        }
        else if (xVelocity < 0)
        {
            facingLeft = true;
            return FacingDirection.left;
        } else
        {
            if (facingLeft)
            {
                return FacingDirection.left;
            } else
            {
                return FacingDirection.right;
            }
        }
    }
}
