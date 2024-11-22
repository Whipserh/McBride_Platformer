using JetBrains.Annotations;
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
        //------------------------------------------------------player side to side movement
        //Debug.Log(IsGrounded());

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
        Vector2 playerInput = Vector2.zero;
        if (Input.GetKey(KeyCode.D))
        {
            playerInput.x++;
        }

        if (Input.GetKey(KeyCode.A))
        {
            playerInput.x--;
        }

        //----------------------------------------------------------player jump controls
        // only jump if our character is touching the ground
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            playerInput.y++;
        }


        

        playerInput.Normalize();//normalize the direction
        MovementUpdate(playerInput);


        
    }
    public float apexTime, apexHeight, terminalFallingSpeed;






    //acceleration and deceleration are both positive terms
    public float acceleration, deceleration;
    public float maxSpeed;

    private void MovementUpdate(Vector2 playerInput)
    {
        Debug.Log(playerInput);

        //horizontal movement

        float force;
        
        if (playerInput.x != 0)//if we are moving then we add accleration force
            force = rb.mass * acceleration;
        else//else we are slowing down
            force = rb.mass * -1* deceleration;
        
        //apply the force
        rb.AddForce(playerInput * force);
        /*
        //cap the max speed incase force is too strong
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
        */



        //-----------------------------------------------------------------------------------------------JUMP
        float initalJumpVelocity = 2 * apexHeight / apexTime;//inital velocity is equal to the inital acceleration of the character
        float gravity = -2 * apexHeight / (Mathf.Pow(apexTime, 2)); // this is equivalent to acceletation gravity
        if (playerInput.y > 0)// the player jumped
        {
            //apply the force
            Debug.Log((transform.up * initalJumpVelocity) * rb.mass);
            rb.AddForce((transform.up * initalJumpVelocity) * rb.mass );
        }//---------------------------------------------------------------------GRAVITY

            //apply the gravity to the player
            //mass of player * direction acceleration
            //direction acceleration = direction_V * gravity

            rb.AddForce(rb.mass * gravity * (transform.up));
        


        
       /* 
        
        if(rb.velocity.y > terminalFallingSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, terminalFallingSpeed);
        }
       */
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
