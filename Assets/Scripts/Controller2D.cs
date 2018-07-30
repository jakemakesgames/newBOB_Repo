using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (BoxCollider2D))]
public class Controller2D : MonoBehaviour {

	const float skinWidth = .015f;
	public LayerMask collisionMask;

	private BoxCollider2D collider;
	private RaycastOrigins raycastOrigins;
	public CollisionInfo collisions;

	// Raycasts being drawn both Vertically and Horizontally from the player 
	public int horizontalRayCount = 4;
	public int verticalRayCount = 4;

	// Max slope angle that can be climbed
	public float maxClimbAngle = 80f;

	// The spacing between the Raycasts
	public float horizontalRaySpacing;
	public float verticalRaySpacing;

	void Start(){
		// Assigning the BoxCollider2D component equal to the collider variable
		collider = GetComponent<BoxCollider2D> ();

		// Call the CalculateRaySpacing function
		CalculateRaySpacing ();
	}

	// This is the function that moves the Player (Called from the Player script)
	public void Move(Vector3 velocity){
		// Call the UpdateRaycastOrigins function
		UpdateRaycastOrigins ();

		collisions.Reset ();

		if (velocity.x != 0) {
			// Call the HorizontalCollisions function
			HorizontalCollisions (ref velocity);
		}
		if (velocity.y != 0) {
			// Call the VerticalCollisions function
			VerticalCollisions(ref velocity);
		}

		// Moving the player via the transform.Translate function
		transform.Translate (velocity);
	}

	void HorizontalCollisions(ref Vector3 velocity){

		float directionX = Mathf.Sign (velocity.x);
		float rayLength = Mathf.Abs (velocity.x) + skinWidth;

		// Debug Draw the Bottom Left Vertical Raycast
		for (int i = 0; i < horizontalRayCount; i++) {
			Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i); 

			// Perform a Raycast from the RayOrigin, in the Y direction, give it a distance and tell the player which LayerMask to collide with
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

			//Debug.DrawRay (raycastOrigins.bottomLeft + Vector2.right * verticalRaySpacing * i, Vector2.up * -2, Color.green);
			Debug.DrawRay (rayOrigin, Vector2.right * directionX * rayLength, Color.green);

			if (hit){

				// Find and get the angle of the ground the player is standing on -> the angle is found as the distance between the surfaces normal and the global UP direction
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

				if (i == 0 && slopeAngle <= maxClimbAngle) {
					//Debug.Log ("Slope Angle: " + slopeAngle);
					float distanceToSlopeStart = 0;
					if (slopeAngle != collisions.slopeAngleOld) {
						distanceToSlopeStart = hit.distance - skinWidth;
						velocity.x -= distanceToSlopeStart * directionX;
					}
					ClimbSlope(ref velocity, slopeAngle);
					velocity.x += distanceToSlopeStart * directionX;
				}

				if (!collisions.climbingSlope || slopeAngle > maxClimbAngle) {
					velocity.x = (hit.distance - skinWidth) * directionX;
					rayLength = hit.distance;

					// if the player is moving left AND they've collided with something on the left, collisions.left is equal to true
					collisions.left = directionX == -1;
					// if the player is moving right AND they've collided with something on the right, collisions.right is equal to true
					collisions.right = directionX == 1;
				}
			}
		}
	}

	void VerticalCollisions(ref Vector3 velocity){

		float directionY = Mathf.Sign (velocity.y);
		float rayLength = Mathf.Abs (velocity.y) + skinWidth;

		// Debug Draw the Bottom Left Vertical Raycast
		for (int i = 0; i < verticalRayCount; i++) {
			Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x); 

			// Perform a Raycast from the RayOrigin, in the Y direction, give it a distance and tell the player which LayerMask to collide with
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

			Debug.DrawRay (rayOrigin, Vector2.up * directionY * rayLength, Color.green);

			if (hit){
				velocity.y = (hit.distance - skinWidth) * directionY;
				rayLength = hit.distance;

				// if the player is moving downwards AND they've collided with something below them, collisions.below is equal to true
				collisions.below = directionY == -1;
				// if the player is upwards AND they've collided with something above them, collisions.above is equal to true
				collisions.above = directionY == 1;
			}
		}
	}

	void ClimbSlope(ref Vector3 velocity, float slopeAngle){
		// Player must have same speed climbing up slopes as they would moving on a flat surface
		float moveDistance = Mathf.Abs(velocity.x);
		float climbVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;

		// Allow the player to jump on slopes
		if (velocity.y <= climbVelocityY) {

			velocity.y = climbVelocityY;
			velocity.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);

			// Set the collisions.below = true
			collisions.below = true;
			// Set the collisions.climbingSlope = true
			collisions.climbingSlope = true;
			collisions.slopeAngle = slopeAngle;
		}
	}


	// This function updates the RaycastOrigin positions
	void UpdateRaycastOrigins(){
		// Temporary Bounds variable set to the Collider components bounds
		Bounds bounds = collider.bounds;
		// Temp variable above is inwards of itself the amount of the skinWidth variable
		bounds.Expand (skinWidth * -2);

		// Setting the RaycastOrigins for the corners of the BoxCollider component
		raycastOrigins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);

	}

	// This function calculates the spacing betwen the Raycasts drawn
	void CalculateRaySpacing(){
		// Temporary Bounds variable set to the Collider components bounds
		Bounds bounds = collider.bounds;
		// Temp variable above is inwards of itself the amount of the skinWidth variable
		bounds.Expand (skinWidth * -2);

		// Make sure the RaycastCount is greater than or equal to 2 (MUST be 1 ray in each corner)
		horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
		verticalRayCount = Mathf.Clamp (verticalRayCount, 2, int.MaxValue);

		// Calculate the Ray Spacing
		horizontalRaySpacing = bounds.size.y / (horizontalRayCount -1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);


	}

	// A struct defining the Vector2 positions of the BoxCollider2D corners
	struct RaycastOrigins{
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}

	// a public struct to get info on which direction the collisiosn are being executed
	public struct CollisionInfo{
		public bool above, below;
		public bool left, right;

		public bool climbingSlope;
		public float slopeAngle, slopeAngleOld;

		public void Reset(){
			above = below = false;
			left = right = false;
			climbingSlope = false;
			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	}
}
