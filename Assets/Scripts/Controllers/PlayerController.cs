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

	[SerializeField] private new Transform camera;
	private CharacterController controller;
	private Entity entity;
	private Weapon weapon;
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
		weapon = GetComponent<Weapon>();
	}

	private void FixedUpdate()
	{
		if (sprinting && Random.Range(0.0f, 1.0f) < (TRIP_PROBABILITY * Time.fixedDeltaTime))
		{
			tripped = true;
			tripTimer = TRIP_TIME;

			EndSprint();
		}

		Packet packet = new Packet();
		packet.type = 0;
		packet.id = entity.id;
		packet.transform = new TransformPacket(transform, Camera.main.transform.eulerAngles.x + 90.0f);

		NetworkManager.Instance.SendMessage(packet);
	}

	private void Update()
	{
		movement = Vector3.down * 10.0f * Time.deltaTime;

		float vertInput = Input.GetAxis("Vertical");
		float HoriInput = Input.GetAxis("Horizontal");

		sprintTimer -= Time.deltaTime;
		sprintCooldownTimer -= Time.deltaTime;
		tripTimer -= Time.deltaTime;

		tripped = tripTimer > 0.0f;

		if (Input.GetKey(KeyCode.LeftShift) && sprintCooldownTimer <= 0.0f && vertInput > 0.0f && !sprinting)
		{
			StartSprint();
		}

		if ((Input.GetKeyUp(KeyCode.LeftShift) || vertInput <= 0.0f || sprintTimer <= 0.0f) && sprinting)
		{
			if(sprintTimer > 0.0f) { sprintCooldownTimer -= sprintTimer; }

			EndSprint();
		}

		movement += transform.forward * vertInput * MOVEMENT_SPEED * Time.deltaTime * (sprinting ? SPRINT_MOD : 1.0f);
		movement += transform.right * HoriInput * MOVEMENT_SPEED * Time.deltaTime;

		movement *= tripped ? TRIP_MOD : 1.0f;

		controller.Move(movement);

		if(entity.shoot)
		{
			entity.shoot.eulerAngles = new Vector3(Camera.main.transform.eulerAngles.x + 90.0f, transform.eulerAngles.y, 0.0f);
		}

		if(Input.GetKeyDown(KeyCode.Mouse0))
		{
			Shoot();
		}
	}

	private void StartSprint()
	{
		sprinting = true;
		sprintTimer = SPRINT_TIME;
		sprintCooldownTimer = SPRINT_COOLDOWN;

		controller.height = 2.2f;
		controller.radius = 0.3f;
		controller.center = Vector2.up * 1.1f;

		camera.localPosition = Vector3.up * 1.85f;

		Packet packet = new Packet();
		packet.type = 1;
		packet.id = entity.id;
		packet.action = new ActionPacket(0);

		NetworkManager.Instance.SendMessage(packet);
	}

	private void EndSprint()
	{
		sprinting = false;
		sprintTimer = 0.0f;

		controller.height = 1.0f;
		controller.radius = 0.5f;
		controller.center = Vector2.up * 0.5f;

		camera.localPosition = new Vector3(0.0f, 0.75f, 0.25f);

		Packet packet = new Packet();
		packet.type = 1;
		packet.id = entity.id;
		packet.action = new ActionPacket(1);

		NetworkManager.Instance.SendMessage(packet);
	}

	private void Shoot()
	{
		weapon.Shoot();
	}
}
