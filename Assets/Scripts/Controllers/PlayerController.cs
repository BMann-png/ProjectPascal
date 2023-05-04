using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Entity))]
public class PlayerController : MonoBehaviour, INetworked
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
    private Vector3 movement;

    private Vector3 prevPos;
    Vector3 NetPrevPos { get { return prevPos; } set { prevPos = value; } }

    private bool isSprinting = false;
    private bool wasSprinting = false;
    private bool isTripping = false;
    private bool wasTripping = false;

    private float sprintTimer = 0.0f;
    private float sprintCooldownTimer = 0.0f;
    private float tripTimer = 0.0f;

    private void Awake()
    {
        NetworkManager.Instance.tickUpdate += NetworkUpdate;
        controller = GetComponent<CharacterController>();
        entity = GetComponent<Entity>();
    }

    private void FixedUpdate()
    {
        if (!GameManager.Instance.Loading)
        {
            if (isSprinting && Random.Range(0.0f, 1.0f) < (TRIP_PROBABILITY * Time.fixedDeltaTime))
            {
                isTripping = true;
                tripTimer = TRIP_TIME;

                EndSprint();
            }
        }
    }

    private void Update()
    {
        if (!GameManager.Instance.Loading)
        {
            movement = Vector3.down * 10.0f * Time.deltaTime;

            float vertInput = Input.GetAxis("Vertical");
            float HoriInput = Input.GetAxis("Horizontal");

            sprintTimer -= Time.deltaTime;
            sprintCooldownTimer -= Time.deltaTime;
            tripTimer -= Time.deltaTime;

            isTripping = tripTimer > 0.0f;

            if (Input.GetKey(KeyCode.LeftShift) && sprintCooldownTimer <= 0.0f && vertInput > 0.0f && !isSprinting)
            {
                StartSprint();
            }

            if ((Input.GetKeyUp(KeyCode.LeftShift) || vertInput <= 0.0f || sprintTimer <= 0.0f) && isSprinting)
            {
                if (sprintTimer > 0.0f) { sprintCooldownTimer -= sprintTimer; }

                EndSprint();
            }

            movement += transform.forward * vertInput * MOVEMENT_SPEED * Time.deltaTime * (isSprinting ? SPRINT_MOD : 1.0f);
            movement += transform.right * HoriInput * MOVEMENT_SPEED * Time.deltaTime;

            movement *= isTripping ? TRIP_MOD : 1.0f;

            controller.Move(movement);

            if (entity.shoot)
            {
                entity.shoot.eulerAngles = new Vector3(Camera.main.transform.eulerAngles.x + 90.0f, transform.eulerAngles.y, 0.0f);
            }

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Shoot();
            }
        }
    }

    public void NetworkUpdate()
    {
        Packet packet;
        if (NetPrevPos != transform.position && Mathf.Abs((transform.position - NetPrevPos).magnitude) > .5)
        {
            packet = new Packet();
            packet.type = 0;
            packet.id = entity.id;
            packet.transform = new TransformPacket(transform, Camera.main.transform.eulerAngles.x + 90.0f);
            NetworkManager.Instance.SendMessage(packet);
        }


        //isSprinting
        if (isSprinting != wasSprinting)
        {
            if (isSprinting)
            {
                packet = new Packet();
                packet.type = 1;
                packet.id = entity.id;
                packet.action = new ActionPacket(0);

                NetworkManager.Instance.SendMessage(packet);
            }
            else if (!isSprinting)
            {
                //no longer isSprinting
                packet = new Packet();
                packet.type = 1;
                packet.id = entity.id;
                if (movement.sqrMagnitude > 0.01f) { packet.action = new ActionPacket(1); }
                else { packet.action = new ActionPacket(2); }

                NetworkManager.Instance.SendMessage(packet);
            }
        }

        wasSprinting = isSprinting; wasTripping = isTripping;
    }

    private void StartSprint()
    {
        isSprinting = true;
        sprintTimer = SPRINT_TIME;
        sprintCooldownTimer = SPRINT_COOLDOWN;

        controller.height = 2.2f;
        controller.radius = 0.3f;
        controller.center = Vector2.up * 1.1f;

        camera.localPosition = Vector3.up * 1.85f;
    }

    private void EndSprint()
    {
        isSprinting = false;
        sprintTimer = 0.0f;

        controller.height = 1.0f;
        controller.radius = 0.5f;
        controller.center = Vector2.up * 0.5f;

        camera.localPosition = new Vector3(0.0f, 0.75f, 0.25f);
    }

    private void Shoot()
    {
        GameManager.Instance.Shoot(0);
    }

    void INetworked.NetworkUpdate()
    {
        throw new System.NotImplementedException();
    }
}
