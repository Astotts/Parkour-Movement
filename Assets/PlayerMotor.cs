using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    private Vector3 moveVector, airMoveVector, lastMove;
    [SerializeField] private float speed = 10;
    private bool holdingWall = false;
    private bool leptFromWall = false;

    private float verticalVelocity;
    [SerializeField] private float jumpforce = 8f;
    [SerializeField] private float gravity = 30f;

    private float airTime;
    

    private CharacterController controller;
    private ControllerColliderHit hit;

    private float timer = 0;
    private bool startTimer = false;

    // Start is called before the first frame update
    void Start()
    {
        //CursorLockMode.None to unlock or press esc in the editor
        Cursor.lockState = CursorLockMode.Locked;
        controller = GetComponent<CharacterController>();    
    }

    // Update is called once per frame
    void Update()
    {
        //Vect3 = right of the character * horizonal axis (returns 1 or -1 depending on if a or d is pressed)
        moveVector = transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical");

        //The controller class contains an isGrounded variable which checks whether or not the base of the controller collider is touching another object
        if(controller.isGrounded){
            //If touching the ground reset all conditions regaurding wall bouncing
            airTime = 2;
            verticalVelocity = -1;
            holdingWall = false;
            leptFromWall = false;
            //Update verticalVelocity to jumpforce instead of -1 and make the last move equal to the direction we were heading as we pressed the "jump" key
            if(Input.GetButtonDown("Jump")){
                verticalVelocity = jumpforce;
                lastMove = moveVector.normalized * speed;
            }
        }
        //If the controller is not touching the ground we should start falling
        else{
            //Time.deltatime represents the time between frames
            verticalVelocity -= gravity * Time.deltaTime;
        }

        //You can initialize a timer by adding the time between frames every frame
        if(startTimer == true){
            timer += Time.deltaTime;
        }
        
        //If we press the jump button while in the error (hit != null tells us that we're touching the wall since we set it to null when we arent touching a wall)
        if(Input.GetButton("Jump") && hit != null && !controller.isGrounded){
            holdingWall = true;
        }

        //Letting go of the jump button will stop the timer and if we've held down the jump button while touching a wall for over 0.5 seconds it will
        //Trigger our hold behaviour, else it will trigger the tap behaviour
        if(Input.GetButtonUp("Jump") && hit != null){
            startTimer = false;
            if(timer >= 0.5f){
                Debug.Log("hold");
                //We're leaving the wall so we're no longer holding it and we set airMove as the direction we're facing times our base speed
                holdingWall = false;
                airMoveVector = transform.forward * speed;
                //Debug.DrawRay(transform.position, airMoveVector, Color.magenta, 5f);
            }
            else{
                Debug.Log("tap");
                //We first set our last move to our base speed, then we reflect it off the wall
                //https://docs.unity3d.com/ScriptReference/Vector3.Reflect.html
                airMoveVector = lastMove.normalized * speed;
                airMoveVector = Vector3.Reflect(airMoveVector, hit.normal);
                //Debug.DrawRay(transform.position, airMoveVector, Color.magenta, 5f);
            }
            //Set conditions to leave the wall
            verticalVelocity = jumpforce;
            airTime = 2;
            leptFromWall = true;
            holdingWall = false;
            hit = null;
        }

        // Jumping and Gravity 
        moveVector.y = 0;
        moveVector.Normalize();
        moveVector *= speed;
        moveVector.y = verticalVelocity;

        //Normal walking
        if(controller.isGrounded){
            controller.Move(moveVector * Time.deltaTime);
        }
        //Movement after wall bouncing
        else if(!controller.isGrounded && !holdingWall && leptFromWall){
            airTime -= 0.0002f * Time.deltaTime;
            airMoveVector.y = verticalVelocity;
            controller.Move(((airMoveVector * airTime) + (Vector3)((Vector2)moveVector / 2)) * Time.deltaTime);
        }
        //Regular jump air movement
        else if(!controller.isGrounded && !holdingWall && !leptFromWall){
            controller.Move(moveVector * Time.deltaTime);
        }
    }

    private void OnControllerColliderHit (ControllerColliderHit hit)
    {
        if(!controller.isGrounded && hit.normal.y < 0.1f)
        {
            //Debug.DrawRay(hit.point, hit.normal, Color.red, 0.25f);
            //While against wall in the air
            if(Input.GetButton("Jump")){
                //Debug.Log("Hit Wall");
                //Debug.DrawRay(hit.point, hit.normal, Color.blue, 5f);
                this.hit = hit;
                startTimer = true;
                timer = 0;
            }
        }
    }
}
