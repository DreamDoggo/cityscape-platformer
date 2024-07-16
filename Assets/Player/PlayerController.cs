using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class PlayerController : MonoBehaviour
{
    [SerializeField] KeyCode MoveLeftKey = KeyCode.A;
    [SerializeField] KeyCode MoveRightKey = KeyCode.D;
    [SerializeField] KeyCode JumpKey = KeyCode.Space;
    [SerializeField] float MoveForce = 15.0f;
    [SerializeField] float MaxVelocity = 6.0f;
    [SerializeField] float DampingCoefficient = 0.97f;
    [SerializeField] float GroundedGraceDistance;
    [SerializeField] float JumpVelocity = 6.0f;
    [SerializeField] Transform GroundCheck;

    Rigidbody2D RefRigidBody;
    BoxCollider2D RefCollider;

    bool CanJump;


    private void Awake()
    {
        RefRigidBody = GetComponent<Rigidbody2D>();
        if (RefRigidBody == null)
        {
            Debug.LogError("A rigidbody component was not found on the player");
        }

        RefCollider = GetComponent<BoxCollider2D>();
        if (RefCollider == null) 
        {
            Debug.LogError("A BoxColider component was not found on the player");
        }
    }

    private void FixedUpdate()
    {
        UpdateMovement();
        UpdateJump();
    }

    private void UpdateMovement() 
    {
        // use a variable to store where the player has to move
        // starts as (x=0, y=0) meaning our player has no velocity 
        Vector2 movementValues = Vector2.zero;

        // convert the user's input to movement values
        if (Input.GetKey(MoveLeftKey))
        {
            movementValues += Vector2.left;
        }
        else if (Input.GetKey(MoveRightKey))
        {
            movementValues += Vector2.right;
        }

        movementValues.Normalize();
        RefRigidBody.AddForce(movementValues * MoveForce);
        // Clamp the maximum velocity to our defined cap.
        if (RefRigidBody.velocity.magnitude > MaxVelocity)
        {
            // Maintain the current direction by using the normalized velocity vector
            RefRigidBody.velocity = RefRigidBody.velocity.normalized * MaxVelocity;
        }
        // We know the player let go of the controls if the input vector is nearly zero.
        if (movementValues.sqrMagnitude <= 0.1f)
        {
            // Quickly damp the movement when we let go of the inputs by multiplying the vector
            // by a value less than one each frame.
            RefRigidBody.velocity *= DampingCoefficient;
        }
    }

    private void UpdateJump() 
    {
        Vector2 rayOrigin = GroundCheck.position;
        Vector2 rayDirection = Vector2.down;
        float rayDistance = .0001f;

        RaycastHit2D rayHit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance);
        Debug.DrawRay(rayOrigin, rayDirection, Color.magenta);
        if (rayHit) 
        {
            if (CanJump == false) { Debug.Log("Jump refreshed"); }
            CanJump = true;
            
            if (RefRigidBody.velocity.y <= 0) 
            {
                
            }
        }

        if (Input.GetKeyDown(JumpKey) && CanJump) 
        {
            Vector2 velocity = RefRigidBody.velocity;
            velocity.y = JumpVelocity;
            RefRigidBody.velocity = velocity;
            CanJump = false;
            Debug.Log("Jump used");
        }

    }

    /* TODO:
     * - Seperate move function
     * - Jump
     * - Wall jump
     */


}
