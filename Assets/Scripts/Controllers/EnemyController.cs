using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum AIState
{
	PATROL,
	IDLE,
	ACTION
}

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Entity))]
public class EnemyController : MonoBehaviour
{
	private static readonly float MOVEMENT_SPEED = 1.5f;
	private static Color[] colors =
	{
		Color.red,
		Color.yellow,
		Color.green,
		Color.blue,
		Color.magenta
	};

	private CharacterController controller;
	private Entity entity;

	private AIState activeState;
	private NavigationNode[] nodes;
	private Vector3 targetMovement;
	private Vector3 direction;
	private int index = 0;

	private void Awake()
	{
		nodes = FindObjectsByType<NavigationNode>(FindObjectsSortMode.None);
		controller = GetComponent<CharacterController>();
		entity = GetComponent<Entity>();
		targetMovement = nodes[0].transform.position;
		direction = (targetMovement - transform.position).normalized;
	}

	private void FixedUpdate()
	{
		Packet packet = new Packet();
		packet.type = 0;
		packet.id = entity.id;
		packet.transform = new TransformPacket(transform);
		NetworkManager.Instance.SendMessage(packet);
	}

	private void Update()
	{
		if (GameManager.Instance.IsServer)
		{
			switch (activeState)
			{
				case AIState.PATROL:
					Patrol();
					if (Random.Range(0, 500) == 0)
					{
						activeState = AIState.IDLE;
					}
					break;
				case AIState.IDLE:
					Idle();
					break;
				case AIState.ACTION:
					Action();
					break;
				default:
					break;
			}
		}
	}

	private void Patrol()
	{
		Vector3 move = (direction * MOVEMENT_SPEED + Vector3.down * 2.0f) * Time.deltaTime;

		controller.Move(move);

		if ((targetMovement - transform.position).sqrMagnitude < 0.25f)
		{
			index = ++index % nodes.Length;
			targetMovement = nodes[index].transform.position;
			direction = (targetMovement - transform.position).normalized;
		}
	}

	private void Idle()
	{
		if (Random.Range(0, 250) == 0)
		{
			activeState = AIState.ACTION;
		}
	}

	private void Action()
	{
		byte index = (byte)Random.Range(0, colors.Length);
		ChangeColor(index);

		Packet packet = new Packet();
		packet.type = 1;
		packet.id = entity.id;
		packet.action = new ActionPacket(index);
		NetworkManager.Instance.SendMessage(packet);

		activeState = AIState.PATROL;
	}

	public void ChangeColor(byte index)
	{
		GetComponent<MeshRenderer>().material.color = colors[index];
	}
}
