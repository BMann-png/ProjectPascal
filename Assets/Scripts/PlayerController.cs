using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
	[SerializeField] private CharacterController controller;
	[SerializeField] private float movementSpeed;
	[SerializeField] private float sprintSpeed;

	private new Transform camera;

	private bool sprinting;
	private float sprintTimer;
	private float sprintCooldown;

	private void OnValidate()
	{
		if(controller == null) { controller = GetComponent<CharacterController>(); }
		camera = transform.GetChild(0);
	}

	private void FixedUpdate()
	{
		Vector3 movement = Vector3.zero;

		sprintTimer -= Time.fixedDeltaTime;
		sprintCooldown -= Time.fixedDeltaTime;

		sprinting = sprintTimer > 0.0f;

		if (Input.GetKey(KeyCode.LeftShift) && sprintCooldown <= 0.0f)
		{
			sprinting = true;
			sprintTimer = 1.0f;
			sprintCooldown = 10.0f;
		}

		if (sprinting)
		{
			movement = camera.forward * sprintSpeed * Time.fixedDeltaTime;
		}
		else
		{
			movement += camera.forward * Input.GetAxis("Vertical") * movementSpeed * Time.fixedDeltaTime;
			movement += camera.right * Input.GetAxis("Horizontal") * movementSpeed * Time.fixedDeltaTime;
		}

		movement.y = 0.0f;

		controller.Move(movement);

		SendUpdate();
	}

	private void SendUpdate()
	{
		//TODO: send player transform
	}
}
