using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(Entity))]
public class PlayerController : MonoBehaviour
{
	private static readonly float SPRINT_TIME = 3.0f;
	private static readonly float SPRINT_COOLDOWN = 6.0f;
	private static readonly float TRIP_TIME = 1.0f;
	private static readonly float TRIP_PROBABILITY = 0.8f / SPRINT_TIME;
	private static readonly float MOVEMENT_SPEED = 3.0f;
	private static readonly float SPRINT_MOD = 2.0f;
	private static readonly float TRIP_MOD = 0.5f;
	private static readonly float REVIVE_TIME = 3.0f;
	private float addedReviveTime;

	[SerializeField] private new Transform camera;
	[SerializeField] private GameObject hand;
	private CharacterController controller;
	private Entity entity;
	private Health health;
	private Vector3 movement;

	private bool sprinting = false;
	private bool tripped = false;

	private bool down = false;
	private float sprintTimer = 0.0f;
	private float sprintCooldownTimer = 0.0f;
	private float tripTimer = 0.0f;
	private float reviveTimer = 0.0f;
	private bool reviving = false;
	private byte playersReviving = 0;

	private void Awake()
	{
		controller = GetComponent<CharacterController>();
		entity = GetComponent<Entity>();
		health = GetComponent<Health>();

		addedReviveTime = 6.0f / health.MaxTrauma;
	}

	private void FixedUpdate()
	{
		if (!GameManager.Instance.Loading)
		{
			if (sprinting && Random.Range(0.0f, 1.0f) < (TRIP_PROBABILITY * Time.fixedDeltaTime))
			{
				tripped = true;
				tripTimer = TRIP_TIME;

				Packet action = new Packet();
				action.type = 1;
				action.id = entity.id;
				action.action = new ActionPacket(4);

				NetworkManager.Instance.SendMessage(action);

				EndSprint();
			}

			Packet packet = new Packet();
			packet.type = 0;
			packet.id = entity.id;
			packet.transform = new TransformPacket(transform, Camera.main.transform.eulerAngles.x + 90.0f);

			NetworkManager.Instance.SendMessage(packet);
		}
	}

	private void Update()
	{
		if (!GameManager.Instance.Loading)
		{
			if(health.health == 0 && !down)
			{
				OnDown();
			}
			else if(health.health == 0 && health.down == 0)
			{
				//TODO: Die
			}

			movement = Vector3.down * 10.0f * Time.deltaTime;
			reviveTimer -= Time.deltaTime;

			if (!down)
			{
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
					if (sprintTimer > 0.0f) { sprintCooldownTimer -= sprintTimer; }

					EndSprint();
				}

				Vector3 move = transform.forward * vertInput + transform.right * HoriInput;

				movement += Vector3.ClampMagnitude(move, 1.0f) * MOVEMENT_SPEED * Time.deltaTime * (sprinting ? SPRINT_MOD : 1.0f);

				movement *= tripped ? TRIP_MOD : 1.0f;
			}
			else if(!reviving)
			{
				health.OnDownDamage(Time.deltaTime);
			}
			else if(reviveTimer <= 0.0f)
			{
				OnRevive();
			}

			controller.Move(movement);

			if (entity.shoot)
			{
				entity.shoot.eulerAngles = new Vector3(Camera.main.transform.eulerAngles.x + 90.0f, transform.eulerAngles.y, 0.0f);
			}
			
            Weapon weapon = hand.GetComponentInChildren<Weapon>();
			if (Input.GetKeyDown(KeyCode.Mouse0) && !down)
			{
				weapon.IsFiring = true;
				weapon.Shoot();
			}
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

	private void OnDown()
	{
		down = true;
		Packet packet = new Packet();
		packet.type = 1;
		packet.id = entity.id;
		packet.action = new ActionPacket(5);

		NetworkManager.Instance.SendMessage(packet);

		health.OnDown();
	}

	private void OnRevive()
	{
		down = false;
		reviving = false;

		Packet packet = new Packet();
		packet.type = 1;
		packet.id = entity.id;
		packet.action = new ActionPacket(6);

		NetworkManager.Instance.SendMessage(packet);

		health.Revive(20);
	}

	public void StartRevive()
	{
		if (++playersReviving == 1)
		{
			reviving = true;
			reviveTimer = REVIVE_TIME + health.trauma * addedReviveTime;
		}
	}

	public void EndRevive()
	{
		if (--playersReviving == 0)
		{
			reviving = false;
			reviveTimer = 0.0f;
		}
	}
}
