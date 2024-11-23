using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum FacingDirection
    {
        left, right
    }

    private FacingDirection currentFacingDirection = FacingDirection.right;
    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.DrawLine(transform.position + new Vector3(0, apexHeight), transform.position + new Vector3(2, apexHeight), Color.red);
        
        
        //change the direction that the player is facing.
        if (Input.GetKeyDown(KeyCode.D))
        {
            currentFacingDirection = FacingDirection.right;
        }else if (Input.GetKeyDown(KeyCode.A))
        {
            currentFacingDirection = FacingDirection.left;
        }
        

        //The input from the player needs to be determined and then passed in the to the MovementUpdate which should
        //manage the actual movement of the character.
        playerInput = Vector2.zero;
        if (Input.GetKey(KeyCode.D))
        {
            playerInput.x++;
        }

        if (Input.GetKey(KeyCode.A))
        {
            playerInput.x--;
        }
        if (Input.GetKeyDown(KeyCode.Space)  &&(IsGrounded() || Time.realtimeSinceStartup - landed_time < coyoteTime))
        {
            
            playerInput.y++;
        }


        playerInput.Normalize();//normalize the direction
        MovementUpdate(playerInput);
    }

    //acceleration and deceleration are both positive terms
    
    public float maxSpeed;
    private float landed_time = 0;
    private void MovementUpdate(Vector2 playerInput)
    {
        
        if(IsGrounded())
        {
            Debug.Log("Landed");
            JUMPED = false;
            landed_time = Time.realtimeSinceStartup;
        }
        

        //horizontal movement
        RIGHT = playerInput.x > 0;
        LEFT = playerInput.x < 0;

        //cap the max speed incase force is too strong
        if (Mathf.Abs(rb.velocity.x) > maxSpeed)
        {
            float direction = rb.velocity.normalized.x;
            rb.velocity = new Vector2(maxSpeed * direction, rb.velocity.y);
        }

        //Debug.Log(Time.realtimeSinceStartup - landed_time);
        Debug.Log(Time.realtimeSinceStartup - landed_time < coyoteTime);
        //JUMP - this is up here and not in the fixed update because the change happens in the frame not the , plus its an instant change not a change over time
        float initalJumpVelocoty = 2 * apexHeight / apexTime;
        if (playerInput.y > 0 && !JUMPED)//either the player is grounded or its been a couple of seconds since they left the ground
        {
            JUMPED = true;
            rb.velocity += initalJumpVelocoty * Vector2.up;
        }

        //PLAYER TERMINAL VELOCITY
        if (rb.velocity.y < -terminalVelocity)
        {
            rb.velocity = new Vector2(rb.velocity.x, -terminalVelocity);
        }
    }//end movement update

    public float acceleration, deceleration;
    private bool LEFT = false, RIGHT = false, JUMPED = false;
    private Vector2 playerInput;

    private void FixedUpdate()
    {
        //horizontal movement
        if (RIGHT)
        {
            rb.velocity += Vector2.right * Time.fixedDeltaTime * acceleration;
        }
        else if (LEFT)
        {
            rb.velocity += Vector2.left * Time.fixedDeltaTime * acceleration;
        }
        else // if we aren't moving then we should slow down
        {
            rb.velocity -= new Vector2(rb.velocity.x, 0).normalized * Time.fixedDeltaTime * deceleration;
        }

        //GRAVITY
        float gravity = -2 * apexHeight / Mathf.Pow(apexTime, 2);
        rb.velocity += gravity * Time.deltaTime * Vector2.up;
    }

    public float coyoteTime;
    public float terminalVelocity, apexTime, apexHeight;

    public bool IsWalking()
    {
        //if our character's horizontal speed is not 0 and they are not falling
        if (rb.velocity.x != 0f && IsGrounded())
            return true;
        return false;
    }

    public LayerMask solidGround;
    public Vector2 boxSize;
    
    public bool IsGrounded()
    {
        
        //make a box cast
        if (!Physics2D.BoxCast(transform.position, boxSize, 0f, -transform.up, 0.5f, solidGround)) 
        {
            
            return false;
        }
        return true;
    }

    public FacingDirection GetFacingDirection()
    {
        return currentFacingDirection;
    }
}
