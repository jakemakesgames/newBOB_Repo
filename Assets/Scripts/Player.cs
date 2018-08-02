using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XboxCtrlrInput;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour {

	private Controller2D controller;

	public float moveSpeed = 6;

	// How high do we want the player to jump?
	public float jumpHeight = 4;
	// How long do we want the player to take to reach the Apex of the jump curve
	public float timeToJumpApex = .4f;

	[SerializeField] private float gravity;
	[SerializeField] private float jumpVelocity;

	public Vector3 velocity;
	private float velocityXSmoothing;

	private float acceleartionTimeAirbourne = .2f;
	private float accelerationTimeGrounded = .1f;


	void Start(){
		controller = GetComponent<Controller2D> ();

		// Calculating the gravity value
		gravity = -(2 * jumpHeight) / Mathf.Pow (timeToJumpApex, 2);
		// Calculating the Jump Velocity value
		jumpVelocity = Mathf.Abs (gravity) * timeToJumpApex;

		Debug.Log ("Gravity: " + gravity + " | Jump Velocity: " + jumpVelocity);
	} 	

	void Update(){
		
		// This IF check stops the player from accumulating gravity, therefore stopping them from falling at a super fast speed
		if (controller.collisions.above || controller.collisions.below) {
			velocity.y = 0;
		}

		// Store the Input in a Vector2 variable
		Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

		// If the Spacebar is pressed call the Jump funtion
		if (Input.GetKeyDown (KeyCode.Space) && controller.collisions.below || XCI.GetButtonDown(XboxButton.A) && controller.collisions.below) {
			Jump ();
		}

		float targetVelocityX = velocity.x = input.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : acceleartionTimeAirbourne);

		// Apply the gravity to the Velocity Vector's Y and multiply by Time.deltaTime
		velocity.y += gravity * Time.deltaTime;
		// Call the Move function on the controller and move the player using that method
		controller.Move (velocity * Time.deltaTime);
	}

	void Jump(){
		// Set the player's Velocity.Y equal to the Jump Velocity
		velocity.y = jumpVelocity;
	}
}
