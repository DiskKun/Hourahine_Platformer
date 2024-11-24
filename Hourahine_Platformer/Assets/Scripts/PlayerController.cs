using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    float xVelocity = 0;
    float yVelocity = 0;
    float jumpVelocity;
    float gravity;


    public float apexTime;
    public float apexHeight;
    public float terminalVelocity;
    public float coyoteTime;
    float coyoteTimer;
    Rigidbody2D rb;
    public float horizontalMaxVelocity = 10;
    public float horizontalAcceleration = 1;
    public float horizontalDeceleration = 1;

    public float groundedRaycastLength;

    public enum FacingDirection
    {
        left, right
    }

    // Start is called before the first frame update
    void Start()
    {
        
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        gravity = -2 * apexHeight / Mathf.Pow(apexTime, 2);
        jumpVelocity = 2 * apexHeight / apexTime;
        //The input from the player needs to be determined and then passed in the to the MovementUpdate which should
        //manage the actual movement of the character.
        Vector2 playerInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        MovementUpdate(playerInput);
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        xVelocity += horizontalAcceleration * playerInput.x * Time.deltaTime;
        if (playerInput.x == 0 && xVelocity != 0)
        {
            xVelocity += -(Mathf.Abs(xVelocity) / xVelocity) * horizontalDeceleration * Time.deltaTime;
            if (xVelocity > -1f && xVelocity < 1f)
            {
                xVelocity = 0;
            }
        }

        xVelocity = Mathf.Clamp(xVelocity, -horizontalMaxVelocity, horizontalMaxVelocity);

        if (IsGrounded())
        {
            yVelocity = 0;
            coyoteTimer = coyoteTime;
        } else
        {
            coyoteTimer -= Time.deltaTime;
        }

        if (yVelocity < terminalVelocity)
        {
            yVelocity = terminalVelocity;
        }



        if (playerInput.y == 1 && (IsGrounded() || coyoteTimer > 0))
        {
            yVelocity = jumpVelocity;
            coyoteTimer = 0;
        }



        rb.MovePosition(rb.position + new Vector2(xVelocity, yVelocity) * Time.deltaTime);

        yVelocity += gravity * Time.deltaTime;


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
        gameObject.layer = 2;
        Debug.DrawLine(transform.position, transform.position + Vector3.down * groundedRaycastLength);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundedRaycastLength);
        gameObject.layer = 7;
        if (hit.collider != null)
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
            return FacingDirection.right;

        }
        else if (xVelocity < 0)
        {
            return FacingDirection.left;

        } else
        {
            return FacingDirection.left;
        }
      
    }
}
