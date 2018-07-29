﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour {

	private Controller2D controller;

	public float gravity = -20f;
	public Vector3 velocity;
	public float moveSpeed = 6;

	void Start(){
		controller = GetComponent<Controller2D> ();
	}

	void Update(){
		// Store the Input in a Vector2 variable
		Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

		velocity.x = input.x * moveSpeed;

		// Apply the gravity to the Velocity Vector's Y and multiply by Time.deltaTime
		velocity.y += gravity * Time.deltaTime;
		// Call the Move function on the controller and move the player using that method
		controller.Move (velocity * Time.deltaTime);
	}
}
