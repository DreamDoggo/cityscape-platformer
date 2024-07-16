using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class PlayerController : MonoBehaviour
{ 
    // This is very unfinished atm sorry :(  -Wyatt
    Rigidbody2D rigidBody;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        if (rigidBody == null ) 
        {
            Debug.LogError("A rigidbody component was not found on the player");    
        }
    }

    private void FixedUpdate()
    {
        // use a variable to store where the player has to move
        // starts as (x=0, y=0) meaning our player has no velocity 
        Vector2 movementValues = Vector2.zero;

        // convert the user's input to movement values
        movementValues.x = Input.GetAxis("Horizontal");

        //movementValues.Normalize();
        rigidBody.velocity = movementValues;

    }

    /* TODO:
     * - Seperate move function
     * - Jump
     * - Wall jump
     */


}
