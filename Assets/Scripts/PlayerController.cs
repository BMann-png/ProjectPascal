using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
	[SerializeField] private float movementSpeed = 5.0f;
	[SerializeField] private float sprintSpeed = 10.0f;
	[SerializeField] private float sprintTime = 2.0f;
	[SerializeField] private float sprintCooldown = 5.0f;

	private CharacterController controller;

	private bool sprinting;
	private float sprintTimer;
	private float sprintCooldownTimer;

	private void OnValidate()
	{
		if(controller == null) { controller = GetComponent<CharacterController>(); }
	}

	private void Update()
	{
		Vector3 movement = Vector3.zero;

		sprintTimer -= Time.deltaTime;
		sprintCooldownTimer -= Time.deltaTime;

		sprinting = sprintTimer > 0.0f;

		if (Input.GetKey(KeyCode.LeftShift) && sprintCooldownTimer <= 0.0f)
		{
			sprinting = true;
			sprintTimer = sprintTime;
			sprintCooldownTimer = sprintCooldown;
		}

		if (sprinting)
		{
			movement = transform.forward * sprintSpeed * Time.deltaTime;
		}
		else
		{
			movement += transform.forward * Input.GetAxis("Vertical") * movementSpeed * Time.deltaTime;
			movement += transform.right * Input.GetAxis("Horizontal") * movementSpeed * Time.deltaTime;
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
