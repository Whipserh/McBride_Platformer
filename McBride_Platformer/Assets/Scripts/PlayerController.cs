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

        Debug.Log(Time.realtimeSinceStartup - landed_time < coyoteTime && !JUMPED);
        // (Did the player hit the button) && is the player grounded
        if (ToggleDoubleJump)
            FALLING = true;

        //*********** Removing falling enables double jump
        if (Input.GetKeyDown(KeyCode.Space)  && (IsGrounded() || (Time.realtimeSinceStartup - landed_time < coyoteTime && FALLING &&!JUMPED))) 
        {
            //Debug.Log("Jump"); 
            JUMPED = true;
            playerInput.y++;
        }

        MovementUpdate(playerInput);
    }
    public bool ToggleDoubleJump;
    //acceleration and deceleration are both positive terms
    public float acceleration, deceleration;
    private bool LEFT = false, RIGHT = false, JUMPED = false;
    private Vector2 playerInput;
    public float maxSpeed;
    private float landed_time = 0; 







    private void MovementUpdate(Vector2 playerInput)
    {

        

        //horizontal movement
        RIGHT = playerInput.x > 0;
        LEFT = playerInput.x < 0;

        //cap the max speed incase force is too strong
        if (Mathf.Abs(rb.velocity.x) > maxSpeed)
        {
            float direction = rb.velocity.normalized.x;
            rb.velocity = new Vector2(maxSpeed * direction, rb.velocity.y);
        }


   
        //JUMP - this is up here and not in the fixed update because the change happens in the frame not the , plus its an instant change not a change over time
        float initalJumpVelocoty = 2 * apexHeight / apexTime;
        if (playerInput.y > 0)//either the player is grounded or its been a couple of seconds since they left the ground
        {
            rb.velocity += initalJumpVelocoty * Vector2.up;
        }

        //PLAYER TERMINAL VELOCITY
        if (rb.velocity.y < -terminalVelocity)
        {
            rb.velocity = new Vector2(rb.velocity.x, -terminalVelocity);
        }
    }//end movement update

    private bool FALLING = false;
    
    private void FixedUpdate()
    {
        //if the player made contact the with ground then update the JUMPED variable
        if (IsGrounded() && rb.velocity.y == 0)
        {
            Debug.Log("Landed");
            JUMPED = false;
            landed_time = Time.realtimeSinceStartup;
        } 

        FALLING = rb.velocity.y < 0; //we are falling if velocity is < 0

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
            if (rb.velocity.x < 0.01)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
        }

        //GRAVITY
        float gravity = -2 * apexHeight / Mathf.Pow(apexTime, 2);
        rb.velocity += gravity * Time.deltaTime * Vector2.up;
    }

    public float coyoteTime;
    public float terminalVelocity, apexTime, apexHeight;

    public int currentHealth = 10;

    public bool IsDead()
    {
        return currentHealth <= 0;
    }

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
