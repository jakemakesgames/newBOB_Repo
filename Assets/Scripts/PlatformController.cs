using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : RaycastController {

	public LayerMask passengerMask;
	public Vector3 move;

	public Vector3[] localWaypoints;
	Vector3[] globalWaypoints;

	List<PassengerMovement> passengerMovement;
	Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();

	public override void Start(){
		base.Start ();

		globalWaypoints = new Vector3[localWaypoints.Length];
		for (int i = 0; i < localWaypoints.Length; i++) {
			globalWaypoints [i] = localWaypoints [i] + transform.position;
		}
	}

	void Update(){
		// Call the UpdateRaycastOrigins funtion
		UpdateRaycastOrigins ();

		Vector3 velocity = move * Time.deltaTime;

		// Call the CalculatePassengerMovement Function (This funtion passes in the velocity variable)
		CalculatePassengerMovement (velocity);

		// Return TRUE for the MovePassengers Bool
		MovePassengers (true);
		// Move the platform via the transform.Translate method (passing in the velocity variable)
		transform.Translate (velocity);

		// Return FALSE for the MovePassengers Bool
		MovePassengers (false);
	}

	void MovePassengers(bool beforeMovePlatform){
		foreach (PassengerMovement passenger in passengerMovement) {
			if (!passengerDictionary.ContainsKey(passenger.transform)) {
				passengerDictionary.Add (passenger.transform, passenger.transform.GetComponent<Controller2D> ());
			}

			if (passenger.moveBeforePlatform == beforeMovePlatform) {
				passengerDictionary[passenger.transform].Move (passenger.velocity, passenger.standingOnPlatform);
			}
		}
	}

	// This function will calculate the passengers movement
	void CalculatePassengerMovement(Vector3 velocity){
		HashSet<Transform> movedPassengers = new HashSet<Transform> ();

		passengerMovement = new List<PassengerMovement> ();

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

						passengerMovement.Add (new PassengerMovement (hit.transform, new Vector3 (pushX, pushY), directionY == 1, true));
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
						float pushY = -skinWidth;

						passengerMovement.Add (new PassengerMovement (hit.transform, new Vector3 (pushX, pushY), false, true));
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

						passengerMovement.Add (new PassengerMovement (hit.transform, new Vector3 (pushX, pushY), true, false));
					}
				}
			}
		}
	}

	struct PassengerMovement{
		public Transform transform;
		public Vector3 velocity;
		public bool standingOnPlatform;
		public bool moveBeforePlatform;

		public PassengerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform){
			transform = _transform;
			velocity = _velocity;
			standingOnPlatform = _standingOnPlatform;
			moveBeforePlatform = _moveBeforePlatform;
		}
	}

	// This funtion is responsible for drawing the Waypoint Gizmos showing the path of the moving platforms. Purely for designing and debugging
	//  THIS WILL NOT BE SHOWN DURING RUNTUIME/ GAME WINDOW
	void OnDrawGizmos(){
		if (localWaypoints != null) {
			Gizmos.color = Color.red;
			float size = .3f;

			for (int i = 0; i < localWaypoints.Length; i++) {
				// If the Application IS running, use the global waypoints array, else use the local waypoints array
				Vector3 globalWaypointPos = (Application.isPlaying)?globalWaypoints[i]:localWaypoints [i] + transform.position;
				// Draw the vertical line of the waypoint
				Gizmos.DrawLine (globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
				// Draw the horizontal line of the waypoint
				Gizmos.DrawLine (globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
			}
		}
	}
}
