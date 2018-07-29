using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (BoxCollider2D))]
public class Controller2D : MonoBehaviour {

	const float skinWidth = .015f;
	public LayerMask collisionMask;

	private BoxCollider2D collider;
	private RaycastOrigins raycastOrigins;

	// Raycasts being drawn both Vertically and Horizontally from the player 
	public int horizontalRayCount = 4;
	public int verticalRayCount = 4;

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
				velocity.x = (hit.distance - skinWidth) * directionX;
				rayLength = hit.distance;
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
			}
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
}
