using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(Entity))]
public class PlayerController : MonoBehaviour, INetworked
{
	private static readonly float SPRINT_TIME = 3.0f;
	private static readonly float SPRINT_COOLDOWN = 4.0f;
	private static readonly float TRIP_TIME = 5.0f;
	private static readonly float MOVEMENT_SPEED = 3.0f;
	private static readonly float SPRINT_MOD = 2.0f;
	private static readonly float TRIP_MOD = 0.5f;
	private static readonly float REVIVE_TIME = 3.0f;
	private static readonly float SPRINT_MAX_LENGTH = 8.0f;
	private float addedReviveTime;

    [SerializeField] private new Transform camera;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject hand;
    private CharacterController controller;
    private Entity entity;
    private Health health;
    private Vector3 movement;
    private HUDManager hudManager;

    private Vector3 NetPrevPos;
    private Vector3 NetPrevRot;

    private bool isSprinting = false;
    private bool wasSprinting = false;

    private float sprintSecondsElapsed = 0.0f;
    private float sprint_max_length;
    private float sprintTimer = 0.0f;
    private float sprintCooldownTimer = 0.0f;

    private bool isTripping = false;
    private bool wasTripping = false;

    private float tripTimer = 0.0f;

    private bool isDown = false;
    private bool wasDown = false;

    private float reviveTimer = 0.0f;
    private bool reviving = false;
    private byte playersReviving = 0;

    private void Awake()
    {
        NetworkManager.Instance.TickUpdate += Tick;

        controller = GetComponent<CharacterController>();
        entity = GetComponent<Entity>();
        health = GetComponent<Health>();

        hudManager = FindAnyObjectByType<HUDManager>();
        entity.animator = animator;

        addedReviveTime = 6.0f / health.MaxTrauma;
    }

    private void FixedUpdate()
    {
        if (!GameManager.Instance.Loading)
        {
            if (isSprinting)
            {
                if (TripChance())
                {
                    isTripping = true;
                    tripTimer = TRIP_TIME;

                    Packet action = new Packet();
                    action.type = 1;
                    action.id = entity.id;
                    action.action = new ActionPacket(4);

                    NetworkManager.Instance.SendMessage(action);

                    EndSprint();


                    animator.SetTrigger("Trip");
                }
            }
        }
    }

    private void Update()
    {
        if (!GameManager.Instance.Loading)
        {
            if (health.health == 0 && !isDown)
            {
                OnDown();
            }
            else if (health.health == 0 && health.down == 0)
			{
				controller.enabled = false;
				transform.position += new Vector3(0, 1, 0) * Time.deltaTime;

				if(transform.position.y > 10)
				{
					GameManager.Instance.Spectate();
					Destroy(gameObject);
				}
				GameManager.Instance.AudioManager.StopCry();
			}

            sprintTimer -= Time.deltaTime;
            sprintCooldownTimer -= Time.deltaTime;
            tripTimer -= Time.deltaTime;

            movement = Vector3.down * 10.0f * Time.deltaTime;
            reviveTimer -= Time.deltaTime;

            if (!isDown && !hudManager.Paused)
            {
                float vertInput = Input.GetAxis("Vertical");
                float HoriInput = Input.GetAxis("Horizontal");

                isTripping = tripTimer > 0.0f;

                if (Input.GetKey(KeyCode.LeftShift) && sprintCooldownTimer <= 0.0f && vertInput > 0.0f && !isSprinting && !isTripping)
                {
                    StartSprint();
                }

                if ((Input.GetKeyUp(KeyCode.LeftShift) || vertInput <= 0.0f) && isSprinting)
                {
                    //if (sprintTimer > 0.0f) { sprintCooldownTimer -= sprintTimer; }

                    EndSprint();
                }

                Vector3 move = transform.forward * vertInput + transform.right * HoriInput;

                movement += Vector3.ClampMagnitude(move, 1.0f) * MOVEMENT_SPEED * Time.deltaTime * (isSprinting ? SPRINT_MOD : 1.0f);

                movement *= isTripping ? TRIP_MOD : 1.0f;
            }
            else if (!reviving)
            {
                health.OnDownDamage(Time.deltaTime);
            }
            else if (reviveTimer <= 0.0f)
            {
                OnRevive();
            }

			if(controller.enabled) controller.Move(movement);

            if (entity.weapon && Camera.main != null)
            {
                float x = Camera.main.transform.eulerAngles.x, y = transform.eulerAngles.y;

                entity.weapon.eulerAngles = new Vector3(x, y, 0.0f);

                Packet packet = new Packet();
                packet.type = 10;
                packet.id = entity.id;
                packet.rotation = new RotationPacket(x, y);

                NetworkManager.Instance.SendMessage(packet);
            }

            Weapon weapon = hand.GetComponentInChildren<Weapon>();
            if (weapon != null && Input.GetKeyDown(KeyCode.Mouse0) && !isDown && !hudManager.Paused && canShoot)
            {
                weapon.IsFiring = true;
                weapon.Shoot();
            }
        }
    }

    public void Tick()
    {
        CheckPosition();
        CheckSprinting();
        CheckDown();
    }

    private void CheckPosition()
    {
        Debug.Log($"currPos/prevPos: {transform.position}/{NetPrevPos}\nMagnitude: {Mathf.Abs((transform.position - NetPrevPos).magnitude)}");
        Debug.Log($"currRot/prevPos: {transform.rotation.eulerAngles}/{NetPrevRot}\nRotDiff: {transform.rotation.eulerAngles - NetPrevRot}");

        if ((transform.position != NetPrevPos && Mathf.Abs((transform.position - NetPrevPos).magnitude) > .1) || RotationDifference())
        {
            NetPrevPos = transform.position;
            NetPrevRot = transform.rotation.eulerAngles;

            Packet packet = new Packet();
            packet.type = 0;
            packet.id = entity.id;
            packet.transform = new TransformPacket(transform, Camera.main.transform.eulerAngles.x + 90.0f);
            NetworkManager.Instance.SendMessage(packet);
        }
    }

    private bool RotationDifference()
    {
        var difference = transform.rotation.eulerAngles - NetPrevRot;

        if (difference.sqrMagnitude > 0)
        {
            return true;
        }

        return false;
    }

    private void CheckSprinting()
    {
        if (isSprinting != wasSprinting)
        {
            if (isSprinting)
            {
                Packet packet = new Packet();
                packet.type = 1;
                packet.id = entity.id;
                packet.action = new ActionPacket(0);

                NetworkManager.Instance.SendMessage(packet);
            }
            else if (!isSprinting)
            {
                //no longer isSprinting
                Packet packet = new Packet();
                packet.type = 1;
                packet.id = entity.id;
                if (movement.sqrMagnitude > 0.01f) { packet.action = new ActionPacket(1); }
                else { packet.action = new ActionPacket(2); }

                NetworkManager.Instance.SendMessage(packet);
            }

            wasSprinting = isSprinting;
        }
    }

    private void CheckDown()
    {
        if (isDown != wasDown)
        {
            Packet packet = new Packet();
            if (isDown)
            {
                packet.type = 1;
                packet.id = entity.id;
                packet.action = new ActionPacket(5);

                NetworkManager.Instance.SendMessage(packet);
            }
            else
            {
                packet.type = 1;
                packet.id = entity.id;
                packet.action = new ActionPacket(6);

                NetworkManager.Instance.SendMessage(packet);
            }

            wasDown = isDown;
        }
    }

    private void StartSprint()
    {
        float x = Random.Range(50.0f, 100.0f);
        sprint_max_length = (Mathf.Pow(2, (0.07647f * x))) * 0.5f;
        sprint_max_length += 50.0f;

        isSprinting = true;
        sprintTimer = SPRINT_TIME;
        sprintCooldownTimer = SPRINT_COOLDOWN;

        controller.height = 2.2f;
        controller.radius = 0.3f;
        controller.center = Vector2.up * 1.1f;

        camera.localPosition = Vector3.up * 1.85f;

        animator.SetTrigger("Sprint");
    }

    private void EndSprint()
    {
        sprintCooldownTimer += sprintTimer * 0.25f;

        sprint_max_length = 0.0f;
        sprintSecondsElapsed = 0.0f;

        isSprinting = false;
        sprintTimer = 0.0f;

        controller.height = 1.0f;
        controller.radius = 0.5f;
        controller.center = Vector2.up * 0.5f;

        camera.localPosition = new Vector3(0.0f, 0.75f, 0.25f);

        animator.SetTrigger("StopSprint");
    }

    private void OnDown()
    {
        isDown = true;

        health.OnDown();

        animator.SetTrigger("Down");
        GameManager.Instance.AudioManager.StartCry();
    }

    private void OnRevive()
    {
        isDown = false;
        reviving = false;

        health.Revive(20);

        animator.SetTrigger("Revive");
        GameManager.Instance.AudioManager.StopCry();
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

    public bool TripChance()
    {
        float x = (sprintSecondsElapsed / SPRINT_MAX_LENGTH) * 100;
        sprintSecondsElapsed += Time.deltaTime;
        return x > sprint_max_length;
    }

    private void OnDestroy()
    {
        NetworkManager.Instance.TickUpdate -= Tick;
    }
}
