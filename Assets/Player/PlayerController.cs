using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    [Header("------ Keycodes ------")]
    [SerializeField] KeyCode MoveLeftKey = KeyCode.A;
    [SerializeField] KeyCode MoveRightKey = KeyCode.D;
    [SerializeField] KeyCode JumpKey = KeyCode.Space;
    [SerializeField] KeyCode SlideKey = KeyCode.S;

    [Header("------ Horizontal Movement ------")]
    [Tooltip("The amount of horizontal force applied to the player when they move")]
    [SerializeField] float MoveForce = 15.0f;

    [Tooltip("The maximum speed the player can move at")]
    [SerializeField] float MaxVelocity = 6.0f;

    [Tooltip("How quickly the player horizontally decellerates")]
    [SerializeField] float DampingCoefficient = 0.97f;

    [Header("------ Jumping ------")]
    [Tooltip("How powerful the player's jump is")]
    [SerializeField] float JumpVelocity = 12.0f;

    [Tooltip("How much to dampen the player's jump when they don't hold the jump button for long enough" +
        "\nUse values between 0 and 1, smaller values produce greater results")]
    [SerializeField] float JumpDampingCoefficient = 0.5f;
    
    [Tooltip("How close to the ground is the player considered grounded (don't make too small)")]
    [SerializeField] float GroundedGraceDistance = .005f;

    [Header("------ Sliding ------")]
    [Tooltip("How far sliding sends the player in the direction they're facing horizontally")]
    [SerializeField] float SlideForce = 45f;

    [Tooltip("How much smaller to make the player's hitbox when sliding")]
    [SerializeField] float SlideSquish = .5f;

    [Tooltip("If the player doesn't run into a wall, how long should they be considered sliding for in seconds")]
    [SerializeField] float SlideDuration = 1.0f;
    
    [Tooltip("How close to the ceiling should the player be considered touching it")]
    [SerializeField] float CeilingGraceDistance = .005f;

    [Header("------ Wall Cling & Wall Jump ------")]
    [Tooltip("How far away should the player check to see if they are touching a wall")]
    [SerializeField] float WallGraceDistance = .55f;

    [Tooltip("How slow should the player fall when against a wall?" +
        "\n Use a value between 0 and 1, smaller values produce greater results")]
    [SerializeField] float WallClingFactor = .9f;

    [Tooltip("What is the tag of the surface that cannot be held on to")]
    [SerializeField] string GlassTag = "NoWallCling";

    [Tooltip("How much horizontal velocity should be added during a wall jump")]
    [SerializeField] float WallJumpForceX = 15f;

    [Tooltip("How much vertical velocity should be added during a wall jump")]
    [SerializeField] float WallJumpForceY = 10f;

    [Header("------ Positional Checks ------")]
    [Tooltip("Where on the player do they check to see if they are grounded")]
    [SerializeField] Transform GroundCheck;

    [Tooltip("Where on the player do they check to see if they are near the ceiling")]
    [SerializeField] Transform CeilingCheck;

    [Tooltip("What number layer is the ground on?")]
    [SerializeField] int GroundLayer;

    Rigidbody2D RefRigidBody;
    BoxCollider2D RefCollider;
    SpriteRenderer RefSprite;
    Animator RefAnimator;

    private bool IsGrounded;
    private bool IsJumping;
    private bool IsSliding;
    private bool IsFacingLeft;
    private bool IsTouchingWall;
    private bool IsTouchingLeftWall;
    private bool IsTouchingRightWall;
    private bool IsTouchingCeiling;
    private bool IsClingingToWall;
    private bool IsWallJumping;

    private string TagOfWallTouching;
    private float DefaultColliderHeight;

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
        else 
        {
            DefaultColliderHeight = RefCollider.size.y;
        }

        RefSprite = GetComponent<SpriteRenderer>();
        if (RefSprite == null) 
        {
            Debug.LogError("The player doesn't have a Sprite!");
        }

        RefAnimator = GetComponent<Animator>();
        if (RefAnimator == null) 
        {
            Debug.LogError("The player doesn't have an animator component!");
        }
    }

    private void Update()
    {
        DampenJump();
        UpdateWallJump();
        UpdateAnimation();
    }
    private void FixedUpdate()
    {
        UpdateMovement();
        UpdateTouchingWall();
        UpdateTouchingCeiling();
        UpdateJump();
        UpdateSlide();
        UpdateWallCling();
        
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
            RefSprite.flipX = true;
        }
        else if (Input.GetKey(MoveRightKey))
        {
            movementValues += Vector2.right;
            IsFacingLeft = false;
            RefSprite.flipX = false;
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

    /// <summary>
    /// Shoot a ray from the bottom of the player to see if they are grounded
    /// </summary>
    private void UpdateGrounded() 
    {
        // define the properties of the ray we will use to check if the player is grounded
        Vector2 rayOrigin = GroundCheck.transform.position;
        Vector2 rayDirection = Vector2.down;

        // If the player is close enough to the ground, give them the ability to jump. Otherwise, take it away.
        RaycastHit2D rayHit = Physics2D.Raycast(rayOrigin, rayDirection, GroundedGraceDistance);
        Debug.DrawRay(rayOrigin, rayDirection * rayHit.distance, Color.magenta);
        if (rayHit && !IsGrounded)
        {
            if (RefRigidBody.velocity.y <= 0 && rayHit.distance <= GroundedGraceDistance && IsGrounded == false)
            {
                // if (IsGrounded == false) { Debug.Log("Jump refreshed"); }
                IsGrounded = true;
            } 
        }
        else
        {
            IsGrounded = false;
        }
    }

    // add vertical velocity to the player if they jump
    private void Jump() 
    {
        // modify the vertical velocity of the player's rigidbody
        if (Input.GetKey(JumpKey) && IsGrounded)
        {
            Vector2 velocity = RefRigidBody.velocity;
            velocity.y = JumpVelocity;
            RefRigidBody.velocity = velocity;
            IsJumping = true;
            // Debug.Log("Jump used");
        }
        else if (IsJumping && IsGrounded)
        {
            IsJumping = false;
        }
    }

    // the player will do a shorter jump if they don't hold the jump key for long enough
    private void DampenJump() 
    {
        if (Input.GetKeyUp(JumpKey) && IsJumping && RefRigidBody.velocity.y > 0)
        {
            RefRigidBody.velocity = new Vector2(RefRigidBody.velocity.x, RefRigidBody.velocity.y * JumpDampingCoefficient);
        }
    }

    // handles all updates related to the slide movement
    private void UpdateSlide() 
    {
        if (Input.GetKey(SlideKey) && IsGrounded && !IsSliding)
        {
            IsSliding = true;

            RefCollider.size = new Vector2(RefCollider.size.x, DefaultColliderHeight * SlideSquish);
            RefCollider.offset = new Vector2(0, 0 - SlideSquish / 2);

            if (IsFacingLeft && !IsTouchingLeftWall) 
            {
                RefRigidBody.velocity += new Vector2(Vector2.left.x * SlideForce, 0); 
            }
            else if (!IsFacingLeft && !IsTouchingRightWall) 
            {
                RefRigidBody.velocity += new Vector2(Vector2.right.x * SlideForce, 0);
            }

            StartCoroutine(SlideTimer());
        }
        else if (IsTouchingWall && IsSliding) 
        {
            StopCoroutine(SlideTimer());
            ResetColliderSize();
            IsSliding = false;
        }
    }

    private IEnumerator SlideTimer() 
    {
        yield return new WaitForSeconds(SlideDuration);
        ResetColliderSize();
        IsSliding = false;
    }

    private void ResetColliderSize() 
    {
        if (IsTouchingCeiling) 
        {
            // repeat until space is availible to reset the collider size
            Invoke("ResetColliderSize", .1f);
            return;
        }
        else 
        {
            RefCollider.size = new Vector2(RefCollider.size.x, DefaultColliderHeight);
            RefCollider.offset = Vector2.zero;
        }
        
    }

    // raycast directly to the left and right of the player to check if they are practically against a wall
    private void UpdateTouchingWall() 
    {
        Vector2 rayOrigin = new Vector2(RefRigidBody.position.x, RefRigidBody.position.y - .2f);
        Vector2 leftRayDirection = Vector2.left;
        Vector2 rightRayDirection = Vector2.right;

        RaycastHit2D leftRayHit = Physics2D.Raycast(rayOrigin, leftRayDirection, WallGraceDistance, GroundLayer);
        RaycastHit2D rightRayHit = Physics2D.Raycast(rayOrigin, rightRayDirection, WallGraceDistance, GroundLayer);

        Debug.DrawRay(rayOrigin, leftRayDirection * leftRayHit.distance, Color.magenta);
        Debug.DrawRay(rayOrigin, rightRayDirection * rightRayHit.distance, Color.cyan);

        if (leftRayHit && leftRayHit.distance <= WallGraceDistance) 
        {
            IsTouchingWall = true;
            IsTouchingLeftWall = true;
            TagOfWallTouching = leftRayHit.collider.gameObject.tag;

            // Debug.Log("Touching left wall!");
            // Debug.Log($"Against object with tag: {TagOfWallTouching}");
        }
        else if (rightRayHit && rightRayHit.distance <= WallGraceDistance) 
        {
            IsTouchingWall = true;
            IsTouchingRightWall = false;
            TagOfWallTouching = rightRayHit.collider.gameObject.tag;
            
            // Debug.Log("Touching right wall!");
            // Debug.Log($"Against object with tag: {TagOfWallTouching}");
        }
        else 
        {
            IsTouchingWall = false;
            IsTouchingLeftWall = false;
            IsTouchingRightWall = false;
            TagOfWallTouching = null;
        }
    }

    // raycast directly upwards from the player to check if they are practically touching the ceiling
    private void UpdateTouchingCeiling() 
    {
        Vector2 rayOrigin = CeilingCheck.position;
        Vector2 rayDirection = Vector2.up;
        
        RaycastHit2D ceilingRayHit = Physics2D.Raycast(rayOrigin, rayDirection, CeilingGraceDistance, GroundLayer);

        if (ceilingRayHit && ceilingRayHit.distance <= CeilingGraceDistance)
        {
            IsTouchingCeiling = true;
            // Debug.Log("Touching Ceiling");
        }
        else 
        {
            IsTouchingCeiling = false; 
        }
    }

    // if against a wall, moving into the wall, and falling downwards, reduce vertical momentum
    private void UpdateWallCling()
    {
        bool fallingDownLeftWall = IsTouchingLeftWall && Input.GetKey(MoveLeftKey) && RefRigidBody.velocity.y < 0 && !IsGrounded;
        bool fallingDownRightWall = IsTouchingRightWall && IsTouchingWall && Input.GetKey(MoveRightKey) && RefRigidBody.velocity.y < 0 && !IsGrounded;

        if ((fallingDownLeftWall || fallingDownRightWall) && TagOfWallTouching != GlassTag)
        {
            IsClingingToWall = true;
            RefRigidBody.velocity *= new Vector2(1, WallClingFactor);
        }
        else 
        {
            IsClingingToWall = false;
        }
    }

    // handle operations related to the wall jump
    private void UpdateWallJump() 
    {
        if (!IsGrounded && IsTouchingWall && TagOfWallTouching != GlassTag) 
        {
            Vector2 velocity = RefRigidBody.velocity;
            velocity.y = WallJumpForceY;
            velocity.x = WallJumpForceX;

            if (IsFacingLeft && Input.GetKeyDown(JumpKey)) 
            {
                IsWallJumping = true;
                RefRigidBody.velocity = velocity;
            }
            else if (!IsFacingLeft && Input.GetKeyDown(JumpKey))
            {
                IsWallJumping = true;
                velocity.x *= -1;
                RefRigidBody.velocity = velocity;
            }
            else 
            {
                IsWallJumping = false; 
            }
        }
    }

    private void UpdateAnimation() 
    {
        RefAnimator.SetBool("IsJumping", IsJumping);
        RefAnimator.SetBool("IsSliding", IsSliding);
        RefAnimator.SetBool("Grounded", IsGrounded);
    }



    /* TODO:
     * - 
     */


}
