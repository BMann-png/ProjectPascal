using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Entity))]
public class EnemyController : MonoBehaviour
{
	private static readonly float MOVEMENT_SPEED = 1.5f;

	private CharacterController controller;
	private Entity entity;

	private void Awake()
	{
		controller = GetComponent<CharacterController>();
		entity = GetComponent<Entity>();
	}

	private void FixedUpdate()
	{
		if (GameManager.Instance.Network)
		{
			Packet packet = new Packet();
			packet.type = 0;
			packet.id = entity.id;
			packet.transform = new TransformPacket(transform, 0.0f);
			NetworkManager.Instance.SendMessage(packet);
		}
	}

	private void Update()
	{

	}
}
