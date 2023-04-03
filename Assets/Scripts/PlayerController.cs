using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
	[SerializeField] private CharacterController controller;
	[SerializeField] private float movementSpeed;

	private void OnValidate()
	{
		if(controller == null) { controller = GetComponent<CharacterController>(); }
	}

	private void FixedUpdate()
	{
		Vector3 movement = Vector3.zero;

		if(Input.GetKey(KeyCode.W)) { movement.z += movementSpeed; }
		if(Input.GetKey(KeyCode.S)) { movement.z -= movementSpeed; }
		if(Input.GetKey(KeyCode.A)) { movement.x -= movementSpeed; }
		if(Input.GetKey(KeyCode.D)) { movement.x += movementSpeed; }

		movement *= Time.fixedDeltaTime;

		//TODO: rotate with camera

		controller.Move(movement);

		SendUpdate();
	}

	private void SendUpdate()
	{
		//TODO: send player transform
	}
}
