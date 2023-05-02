using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Entity))]
public class EnemyController : MonoBehaviour
{
	private Entity entity;

	private void Awake()
	{
		entity = GetComponent<Entity>();
	}

	private void FixedUpdate()
	{
		if (!GameManager.Instance.Loading)
		{
			Packet packet = new Packet();
			packet.type = 0;
			packet.id = entity.id;
			packet.transform = new TransformPacket(transform, 0.0f);
			NetworkManager.Instance.SendMessage(packet);
		}
	}
}
