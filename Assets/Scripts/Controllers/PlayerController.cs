using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Entity))]
public class PlayerController : MonoBehaviour
{
	private static readonly float SPRINT_TIME = 3.0f;
	private static readonly float SPRINT_COOLDOWN = 6.0f;
	private static readonly float TRIP_TIME = 1.0f;
	private static readonly float TRIP_PROBABILITY = 0.8f / SPRINT_TIME;
	private static readonly float MOVEMENT_SPEED = 6.66f;
	private static readonly float SPRINT_MOD = 1.5f;
	private static readonly float TRIP_MOD = 0.5f;

	private CharacterController controller;
	private Entity entity;
	private Vector3 movement;

	private bool sprinting = false;
	private bool tripped = false;
	private float sprintTimer = 0.0f;
	private float sprintCooldownTimer = 0.0f;
	private float tripTimer = 0.0f;

	private void Awake()
	{
		controller = GetComponent<CharacterController>();
		entity = GetComponent<Entity>();
	}

	private void FixedUpdate()
	{
		if (sprinting && Random.Range(0.0f, 1.0f) < (TRIP_PROBABILITY * Time.fixedDeltaTime))
		{
			tripped = true;
			sprinting = false;
			sprintTimer = 0.0f;
			tripTimer = TRIP_TIME;
		}

		if (movement.sqrMagnitude > 0.0f)
		{
			Packet packet = new TransformPacket(entity.id, transform);

			NetworkManager.Instance.SendMessage(packet);
		}
	}

	private void Update()
	{
		movement = Vector3.zero;

		float vertInput = Input.GetAxis("Vertical");
		float HoriInput = Input.GetAxis("Horizontal");

		sprintTimer -= Time.deltaTime;
		sprintCooldownTimer -= Time.deltaTime;
		tripTimer -= Time.deltaTime;

		tripped = tripTimer > 0.0f;

		if (Input.GetKey(KeyCode.LeftShift) && sprintCooldownTimer <= 0.0f && vertInput > 0.0f && !sprinting)
		{
			sprinting = true;
			sprintTimer = SPRINT_TIME;
			sprintCooldownTimer = SPRINT_COOLDOWN;
		}

		if ((Input.GetKeyUp(KeyCode.LeftShift) || vertInput <= 0.0f || sprintTimer <= 0.0f) && sprinting)
		{
			if(sprintTimer > 0.0f) { sprintCooldownTimer -= sprintTimer; }
			sprinting = false;
			sprintTimer = 0.0f;
		}

		movement += transform.forward * vertInput * MOVEMENT_SPEED * Time.deltaTime * (sprinting ? SPRINT_MOD : 1.0f);
		movement += transform.right * HoriInput * MOVEMENT_SPEED * Time.deltaTime;
		movement.y = 0.0f;

		movement *= tripped ? TRIP_MOD : 1.0f;

		controller.Move(movement);

		SendUpdate();
	}

	private void SendUpdate()
	{
		//TODO: send player transform
	}
}
