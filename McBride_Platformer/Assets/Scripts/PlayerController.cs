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

        Debug.Log(IsGrounded());

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
        Vector2 playerInput = new Vector2();
        if (Input.GetKey(KeyCode.D))
        {
            playerInput.x++;
        }

        if (Input.GetKey(KeyCode.A))
        {
            playerInput.x--;
        }


        playerInput.Normalize();//normalize the direction
        MovementUpdate(playerInput);
    }

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
        
        //cap the max speed incase force is too strong
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }

    }

    public bool IsWalking()
    {
        //if our character's horizontal speed is not 0 and they are not falling
        if (rb.velocity.x != 0f && IsGrounded())
            return true;
        return false;
    }

    public LayerMask solidGround;
    public new Vector2 boxSize;
    public float fallDistance;
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
