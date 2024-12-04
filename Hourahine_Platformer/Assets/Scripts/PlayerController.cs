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
        xVelocity += horizontalAcceleration * playerInput.x * Time.deltaTime;

        if (playerInput.x == 0 && xVelocity != 0 && IsGrounded())
        {
            xVelocity += -(Mathf.Abs(xVelocity) / xVelocity) * horizontalDeceleration * Time.deltaTime;
            if (xVelocity > -1f && xVelocity < 1f)
            {
                xVelocity = 0;
            }
        }

        if (dashTimer < 0 || IsGrounded())
        {
            xVelocity = Mathf.Clamp(xVelocity, -horizontalMaxVelocity, horizontalMaxVelocity);

        }


        if (IsGrounded())
        {
            yVelocity = 0;
            coyoteTimer = coyoteTime;
        } else
        {
            coyoteTimer -= Time.deltaTime;
        }

        float newTerminalVelocity;

        if (IsHittingWall())
        {
            newTerminalVelocity = terminalVelocity / 2;
        } else
        {
            newTerminalVelocity = terminalVelocity;
        }

        if (yVelocity < newTerminalVelocity)
        {
            yVelocity = newTerminalVelocity;
        }



        if (playerInput.y == 1 && (IsGrounded() || coyoteTimer > 0 || (IsHittingWall() && yVelocity < 0)))
        {
            yVelocity = jumpVelocity;
            coyoteTimer = 0;
        }

        dashTimer -= Time.deltaTime;
        Debug.Log(dashTimer);

        if (dashButton && dashTimer <= 0)
        {
            Debug.Log("dash!");
            dashTimer = dashTime;
            yVelocity = playerInput.y * dashStrength;
            xVelocity = xVelocity * dashStrength;
        }

        rb.MovePosition(rb.position + new Vector2(xVelocity, yVelocity) * Time.deltaTime);

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
