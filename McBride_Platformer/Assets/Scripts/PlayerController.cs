using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerController : MonoBehaviour
{


    //acceleration and deceleration are both positive terms
    public float acceleration, deceleration;
    private bool LEFT = false, RIGHT = false, JUMPED = false;
    private Vector2 playerInput;
    public float maxSpeed;

    private float coyoteTimeElapsed = 0; // resets after jumping

    public CharacterState currentCharacterState = CharacterState.idle;
    public CharacterState previousCharacterState = CharacterState.idle;


    //variables for dashing mechanics
    private bool DASHING; // if we are in the state of dashing
    private bool hadDashed; //if we had already dashed before value is reset
    public float dashDistance, dashTime;
    private float dashElapsedTime = 0;


    private bool FALLING = false;

    private float gravity;
    public float terminalVelocity, apexTime, apexHeight;
    public float fallTime;
    public float coyoteTime;
    

    public int currentHealth = 10;

    //is ground variables
    public LayerMask solidGround;//what layers act as the ground
    public Vector2 boxSize;//rough bottom hitbox


    public enum CharacterState
    {
        idle, walk, jump, die
    }

    public enum FacingDirection
    {
        left, right
    }

    private FacingDirection currentFacingDirection = FacingDirection.right;
    private Rigidbody2D rb;

    void Start()
    {
        DASHING = false;
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        //****************************************************************************Character state

        //save the previous character state
        previousCharacterState = currentCharacterState;

        //update the current state that we are in
        if (IsDead())//if our character is dead then we don't need to look at anything else. 
        {
            currentCharacterState = CharacterState.die;
        }
        
        switch (currentCharacterState)
        {
            case CharacterState.idle:
                if (IsWalking())
                {
                    currentCharacterState = CharacterState.walk;
                }
                if (!IsGrounded())
                {
                    currentCharacterState = CharacterState.jump;
                }
                break;
            case CharacterState.walk:
                if (!IsWalking())
                {
                    //if we are on the ground from walking and we are not walking
                    currentCharacterState = CharacterState.idle;
                }
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
                    }
                    else
                    {
                        currentCharacterState = CharacterState.idle;
                    }
                }
                break;
            case CharacterState.die:
                //this does not have anything cause we want a very hard death moment
                break;
        }


        if (!IsGrounded())//we are in the air
        {
            //Debug.Log("in the air");
            coyoteTimeElapsed = 0;
        }
        else//we landed
        {
            hadDashed = false; // reset the dash when the player touches the floor
            JUMPED = false; //reset the jump when the player touches the ground
        }


        //****************************************************************************Facing Direction

        if(!DASHING)
        //change the direction that the player is facing.
        if (Input.GetKey(KeyCode.D))
        {
            currentFacingDirection = FacingDirection.right;
        }else if (Input.GetKey(KeyCode.A))
        {
            currentFacingDirection = FacingDirection.left;
        }
        //****************************************************************************Dash controls
        if (Input.GetMouseButtonDown(0)  && dashLegible()) //can only DASHING if we aren't dashing
        {
            hadDashed = true;
            Debug.Log("Dash");
            dashElapsedTime = 0;
            DASHING = true;
        }

        //*****************************************************************************Movement co
        //The input from the player needs to be determined and then passed
        //in the to the MovementUpdate which should manage the actual
        //movement of the character.
        playerInput = Vector2.zero;

        //disable the controls if we are dashing
        if (!DASHING)
        {
            if (Input.GetKey(KeyCode.D))
            {
                playerInput.x++;
            }

            if (Input.GetKey(KeyCode.A))
            {
                playerInput.x--;
            }

            //Jump
            if (Input.GetKeyDown(KeyCode.Space) && (legibleJump() || legibleWallJump()))
            {
                JUMPED = true;
                playerInput.y++;
            }
        }
        MovementUpdate(playerInput);

        coyoteTimeElapsed+=Time.deltaTime;

        
    }


    public bool legibleJump()
    {
        //Debug.Log(JUMPED);

        //coyote time should only start counting the moment the player is not longer on the ground
        return (!JUMPED && (IsGrounded() || (coyoteTimeElapsed < coyoteTime)));
    }

    public bool legibleWallJump()
    {
        //********************************************Get the side of the player that is touching the wall
        float direction = 0;
        if(Physics2D.BoxCast(transform.position, new Vector2(0.1f, 0.5f), 0, Vector2.left, 0.75f, solidGround))//check left
        {
            //Debug.Log("left");
            direction = 0;//left
        }else if(Physics2D.BoxCast(transform.position, new Vector2(0.1f, 0.5f), 0, Vector2.right, 0.75f, solidGround))//check right
        {
            //Debug.Log("right");
            direction = 1;//right
        }
        else//we don't see any legible position so 
        {
            return false;
        }
        //there is an object besides us if we make it HERE


        //check to see if the player is mid air
        if (!IsGrounded())
        {
            //project the player in the opposite direction from the wall
            rb.velocity= Vector2.Lerp(new Vector2(2*maxSpeed, rb.velocity.y), new Vector2(-2*maxSpeed, rb.velocity.y), direction);
            return true;
        }
        return false;
    }



    public bool dashLegible()
    {
        if (!hadDashed)// if we hadn't dashed AND we aren't in the middle of a dash
        {
            return true;
        }
        return false;
    }

    /**
     * 
     * */
    public int wallCollision()
    {
        int direction = -1;
        if (Physics2D.BoxCast(transform.position, new Vector2(0.1f, 0.5f), 0, Vector2.left, 0.75f, solidGround))//check left
        {
            //Debug.Log("left");
            direction = 0;//left
        }
        else if (Physics2D.BoxCast(transform.position, new Vector2(0.1f, 0.5f), 0, Vector2.right, 0.75f, solidGround))//check right
        {
            //Debug.Log("right");
            direction = 1;//right
        }
        else//we don't see any legible position so 
        {
            direction = -1;
        }
        return direction;
    }


    private void MovementUpdate(Vector2 playerInput)
    {

        Debug.Log(playerInput);

        //horizontal movement
        RIGHT = playerInput.x > 0;
        LEFT = playerInput.x < 0;

        //cap the max speed incase force is too strong
        if (Mathf.Abs(rb.velocity.x) > maxSpeed)
        {
            float direction = new Vector2(rb.velocity.x, 0).normalized.x;
            rb.velocity = new Vector2(maxSpeed * direction, rb.velocity.y);
        }

        //JUMP - this is up here and not in the fixed update because the change happens in the frame not the , plus its an instant change not a change over time
        float initalJumpVelocoty = 2 * apexHeight / apexTime;
        if (playerInput.y > 0)//either the player is grounded or its been a couple of seconds since they left the ground
        {
            rb.velocity = new Vector2(rb.velocity.x, initalJumpVelocoty);
        }

        //PLAYER TERMINAL VELOCITY
        if (rb.velocity.y < -terminalVelocity)
        {
            rb.velocity = new Vector2(rb.velocity.x, -terminalVelocity);
        }
    }//end movement update
    


    private void FixedUpdate()
    {
        

        //******************************************** DASH
        dashElapsedTime += Time.fixedDeltaTime;
        //stop dash if either the elasped time is the dash time OR if they hit a wall
        if (dashElapsedTime >= dashTime || wallCollision() != -1)
        {
            DASHING = false;
        }


        //if we are dashing we move in a specific direction
        if (DASHING)
        {
            if(GetFacingDirection() == FacingDirection.right)
                rb.velocity = new Vector2(dashDistance/dashTime, rb.velocity.y);
            else
                rb.velocity = new Vector2(-dashDistance / dashTime, rb.velocity.y);
        }




        FALLING = rb.velocity.y < 0; //we are falling if velocity is < 0

        //horizontal movement if the player pressed a button
        if (RIGHT)
        {
            //Debug.Log("right");
            rb.velocity += Vector2.right * Time.fixedDeltaTime * acceleration;
        }
        else if (LEFT)
        {
            //Debug.Log("left");
            rb.velocity += Vector2.left * Time.fixedDeltaTime * acceleration;
        }
        else // if we aren't moving then we should slow down
        {

            //Debug.Log(currentFacingDirection);
            rb.velocity -= new Vector2(rb.velocity.x, 0).normalized * Time.fixedDeltaTime * deceleration;
            if (Mathf.Abs(rb.velocity.x) < 0.1)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
        }

        
        //GRAVITY
        if (rb.velocity.y > 0) { // soft gravity
            gravity = -2 * apexHeight / Mathf.Pow(apexTime, 2);
        } else
        {
            gravity = -2 * apexHeight / Mathf.Pow(fallTime, 2);
        }
        if(!IsGrounded())
        rb.velocity += gravity * Time.deltaTime * Vector2.up;
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }

    public bool IsWalking()
    {
        //if our character's horizontal speed is not 0 and they are not falling
        if (Mathf.Abs(rb.velocity.x) >= 0.1f && IsGrounded())
            return true;
        return false;
    }


    public bool IsGrounded()
    {
        return Physics2D.BoxCast(transform.position, boxSize, 0f, -transform.up, 0.5f, solidGround) && rb.velocity.y < 0.01f;
    }

    //referenced in the animator
    public void OnAnimationDeathCompleet()
    {
        gameObject.SetActive(false);
    }

    public FacingDirection GetFacingDirection()
    {
        return currentFacingDirection;
    }
}
