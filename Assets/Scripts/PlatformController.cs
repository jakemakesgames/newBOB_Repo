using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : RaycastController {

	public LayerMask passengerMask;
	public Vector3 move;

	public override void Start(){
		base.Start ();
	}

	void Update(){
		// Call the UpdateRaycastOrigins funtion
		UpdateRaycastOrigins ();

		Vector3 velocity = move * Time.deltaTime;

		MovePassengers (velocity);
		transform.Translate (velocity);
	}

	// This function will move any object with a Controller2D component attached to it, whether they're above, below, to th left or to the right.
	void MovePassengers(Vector3 velocity){
		HashSet<Transform> movedPassengers = new HashSet<Transform> ();

		float directionX = Mathf.Sign (velocity.x);
		float directionY = Mathf.Sign (velocity.y);

		// Vertically momving platform
		if (velocity.y != 0){
			float rayLength = Mathf.Abs (velocity.y) + skinWidth;

			// Debug Draw the Bottom Left Vertical Raycast
			for (int i = 0; i < verticalRayCount; i++) {
				Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
				rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x); 

				// Perform a Raycast from the RayOrigin, in the Y direction, give it a distance and tell the player which LayerMask to collide with
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

				// If there is a passenger, how far will we move the
				if (hit) {

					if (!movedPassengers.Contains (hit.transform)) {
						movedPassengers.Add (hit.transform);
						float pushX = (directionY == 1) ? velocity.x : 0;
						float pushY = velocity.y - (hit.distance - skinWidth) * directionY;

						hit.transform.Translate (new Vector3 (pushX, pushY));
					}
				}
			}
		}

		// Horizontally moving platform
		if (velocity.x !=0){
			float rayLength = Mathf.Abs (velocity.x) + skinWidth;

			// Debug Draw the Bottom Left Vertical Raycast
			for (int i = 0; i < horizontalRayCount; i++) {
				Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
				rayOrigin += Vector2.up * (horizontalRaySpacing * i); 

				// Perform a Raycast from the RayOrigin, in the Y direction, give it a distance and tell the player which LayerMask to collide with
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);

				if (hit) {

					if (!movedPassengers.Contains (hit.transform)) {
						movedPassengers.Add (hit.transform);
						float pushX = velocity.x - (hit.distance - skinWidth) * directionX;
						float pushY = 0;

						hit.transform.Translate (new Vector3 (pushX, pushY));
					}
				}
			}
		}

		// Passenger ontop of a horizontally or downward moving platform
		if (directionY == -1 || velocity.y == 0 && velocity.x != 0){
			float rayLength = skinWidth * 2;

			// Debug Draw the Bottom Left Vertical Raycast
			for (int i = 0; i < verticalRayCount; i++) {
				Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i + velocity.x); 

				// Perform a Raycast from the RayOrigin, in the Y direction, give it a distance and tell the player which LayerMask to collide with
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

				// If there is a passenger, how far will we move the
				if (hit) {

					if (!movedPassengers.Contains (hit.transform)) {
						movedPassengers.Add (hit.transform);
						float pushX = velocity.x;
						float pushY = velocity.y;

						hit.transform.Translate (new Vector3 (pushX, pushY));
					}
				}
			}
		}
	}
}
