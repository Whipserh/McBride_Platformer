using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public enum CharacterState
    {
        idle, walk, jump, die
    }
    public CharacterState currentCharacterState = CharacterState.idle;
    public CharacterState previousCharacterState = CharacterState.idle;

    public enum FacingDirection
    {
        left, right
    }

    private FacingDirection currentFacingDirection = FacingDirection.right;
    private Rigidbody2D rb;

    void Start()
    {
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
            
            JUMPED = false;
        }


        //****************************************************************************Facing Direction


        //change the direction that the player is facing.
        if (Input.GetKeyDown(KeyCode.D))
        {
            currentFacingDirection = FacingDirection.right;
        }else if (Input.GetKeyDown(KeyCode.A))
        {
            currentFacingDirection = FacingDirection.left;
        }
        

        //The input from the player needs to be determined and then passed
        //in the to the MovementUpdate which should manage the actual
        //movement of the character.
        playerInput = Vector2.zero;
        if (Input.GetKey(KeyCode.D))
        {
            playerInput.x++;
        }

        if (Input.GetKey(KeyCode.A))
        {
            playerInput.x--;
        }



        
        if (Input.GetKeyDown(KeyCode.Space)  && (legibleJump()|| legibleWallJump())) 
        {
            JUMPED = true;
            playerInput.y++;
        }

        MovementUpdate(playerInput);

        coyoteTimeElapsed+=Time.deltaTime;

        
    }
    
    //acceleration and deceleration are both positive terms
    public float acceleration, deceleration;
    private bool LEFT = false, RIGHT = false, JUMPED = false;
    private Vector2 playerInput;
    public float maxSpeed;
    


    public bool legibleJump()
    {
        //Debug.Log(JUMPED);
       
        //coyote time should only start counting the moment the player is not longer on the ground
        return (!JUMPED && (IsGrounded() || (coyoteTimeElapsed < coyoteTime)));
    }
    private float coyoteTimeElapsed = 0; // resets after jumping

    public bool legibleWallJump()
    {
        //********************************************Get the side of the player that is touching the wall
        float direction = 0;
        if(Physics2D.BoxCast(transform.position, new Vector2(0.1f, 0.5f), 0, Vector2.left, 0.5f, solidGround))//check left
        {
            Debug.Log("left");
            direction = 0;//left
        }else if(Physics2D.BoxCast(transform.position, new Vector2(0.1f, 0.5f), 0, Vector2.right, 0.5f, solidGround))//check right
        {
            Debug.Log("right");
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
            if (Mathf.Abs(rb.velocity.x) < 0.01)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
        }

        //GRAVITY
        if (rb.velocity.y > 0) { // soft gravity
            gravity = -2 * apexHeight / Mathf.Pow(apexTime, 2);
        } else
        {
            gravity = -2 * apexHeight / Mathf.Pow(apexTime * 0.75f, 2);
        }
        rb.velocity += gravity * Time.deltaTime * Vector2.up;
    }
    private float gravity;

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
