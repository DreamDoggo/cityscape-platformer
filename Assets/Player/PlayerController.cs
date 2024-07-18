using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    [SerializeField] KeyCode MoveLeftKey = KeyCode.A;
    [SerializeField] KeyCode MoveRightKey = KeyCode.D;
    [SerializeField] KeyCode JumpKey = KeyCode.Space;
    [SerializeField] KeyCode SlideKey = KeyCode.S;

    [Tooltip("The amount of horizontal force applied to the player when they move")]
    [SerializeField] float MoveForce = 15.0f;

    [Tooltip("The maximum speed the player can move at")]
    [SerializeField] float MaxVelocity = 6.0f;

    [Tooltip("How quickly the player horizontally decellerates")]
    [SerializeField] float DampingCoefficient = 0.97f;

    [Tooltip("How close to the ground is the player considered grounded (don't make too small)")]
    [SerializeField] float GroundedGraceDistance = .005f;

    [Tooltip("How powerful the player's jump is")]
    [SerializeField] float JumpVelocity = 12.0f;

    [Tooltip("How much to dampen the player's jump when they don't hold the jump button for long enough" +
        "\nUse values between 0 and 1, smaller values produce greater results")]
    [SerializeField] float JumpDampingCoefficient = 0.5f;

    [Tooltip("How far sliding sends the player in the direction they're facing horizontally")]
    [SerializeField] float SlideForce = 45f;

    [Tooltip("How much smaller to make the player's hitbox when sliding")]
    [SerializeField] float SlideSquish = .5f;

    [SerializeField] float SlideDuration = 1.0f;

    [Tooltip("Where on the player do they check to see if they are grounded")]
    [SerializeField] Transform GroundCheck;

    [Tooltip("What number layer is the ground on?")]
    [SerializeField] int GroundLayer;

    Rigidbody2D RefRigidBody;
    BoxCollider2D RefCollider;
    SpriteRenderer RefSprite;

    private bool IsGrounded;
    private bool IsJumping;
    private bool IsSliding;
    private bool IsFacingLeft;



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

        RefSprite = GetComponent<SpriteRenderer>();
        if (RefSprite == null) 
        {
            Debug.LogError("The player doesn't have a Sprite!");
        }
    }

    private void Update()
    {
        DampenJump();
    }
    private void FixedUpdate()
    {
        UpdateMovement();
        UpdateJump();
        //UpdateSlide();
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
            IsFacingLeft = true;
            Debug.Log(IsFacingLeft);
        }
        else if (Input.GetKey(MoveRightKey))
        {
            movementValues += Vector2.right;
            IsFacingLeft = false;
            Debug.Log(IsFacingLeft);
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

    /// <summary>
    /// Performs most operations related to the player jumping
    /// </summary>
    private void UpdateJump() 
    {
        UpdateGrounded();
        Jump();
    }

    private void UpdateGrounded() 
    {
        // define the properties of the ray we will use to check if the player is grounded
        Vector2 rayOrigin = GroundCheck.transform.position;
        Vector2 rayDirection = Vector2.down;

        // If the player is close enough to the ground, give them the ability to jump. Otherwise, take it away.
        RaycastHit2D rayHit = Physics2D.Raycast(rayOrigin, rayDirection, GroundedGraceDistance, GroundLayer);
        // Debug.DrawRay(rayOrigin, rayDirection * rayHit.distance, Color.magenta);
        if (rayHit)
        {
            if (RefRigidBody.velocity.y <= 0 && rayHit.distance <= GroundedGraceDistance)
            {
                if (IsGrounded == false) { Debug.Log("Jump refreshed"); }
                IsGrounded = true;
            }
        }
    }

    private void Jump() 
    {
        // modify the vertical velocity of the player's rigidbody
        if (Input.GetKey(JumpKey) && IsGrounded)
        {
            Vector2 velocity = RefRigidBody.velocity;
            velocity.y = JumpVelocity;
            RefRigidBody.velocity = velocity;
            IsGrounded = false;
            IsJumping = true;
            Debug.Log("Jump used");
        }
    }

    private void DampenJump() 
    {
        if (Input.GetKeyUp(JumpKey) && IsJumping && RefRigidBody.velocity.y > 0)
        {
            RefRigidBody.velocity = new Vector2(RefRigidBody.velocity.x, RefRigidBody.velocity.y * JumpDampingCoefficient);
        }
    }

    /*private void UpdateSlide() 
    {
        if (Input.GetKeyDown(SlideKey) && IsGrounded && !IsSliding)
        {
            
            //RefRigidBody.constraints = RigidbodyConstraints2D.FreezePositionY;
            
            //RefRigidBody.constraints = RigidbodyConstraints2D.FreezePositionY;

            if (IsFacingLeft) 
            {
                //RefRigidBody.velocity += new Vector2(Vector2.left.x * SlideForce, 0);
                StartCoroutine(PerformSlide(Vector2.left.x));
                
            }
            else 
            {
                //RefRigidBody.velocity += new Vector2(Vector2.right.x * SlideForce, 0);
                StartCoroutine (PerformSlide(Vector2.right.x));
            }   
        }
    }

    private IEnumerator PerformSlide(float direction) 
    {
        IsSliding = true;
        RefCollider.size = new Vector2(RefCollider.size.x, RefCollider.size.y * SlideSquish);
        RefCollider.offset = new Vector2(0, 0 - SlideSquish / 2);

        // calculate how long the slide should last
        float slideLength = Time.time + SlideDuration;
        float slideStartTime = Time.time;

        while (Time.time - slideStartTime < slideLength) 
        {
            RefRigidBody.AddForce(new Vector2(direction * SlideForce * Time.deltaTime, 0));
        }

        ResetColliderSize();
        IsSliding = false;
        yield return null;

    }

    private void ResetColliderSize() 
    {
        RefCollider.size = new Vector2(RefCollider.size.x, RefCollider.size.y / SlideSquish);
        RefCollider.offset = Vector2.zero;
    } */


    /* TODO:
     * - Wall jump
     * - Slide
     * - Reset jump on all objects
     */


}
