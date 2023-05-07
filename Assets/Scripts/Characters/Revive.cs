using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Entity), typeof(Interactable))]
public class Revive : MonoBehaviour
{
	private Entity entity;
	private Interactable interactable;

	private void Awake()
	{
		entity = GetComponent<Entity>();
		interactable = GetComponent<Interactable>();
	}

	public void OnDown()
	{
		interactable.canInteract = true;
	}

	public void OnRevive()
	{
		interactable.canInteract = false;
	}

	public void BeginRevive()
	{
		Packet packet = new Packet();
		packet.type = 1;
		packet.id = entity.id;
		packet.action = new ActionPacket(10);

		NetworkManager.Instance.SendMessage(packet);
	}

	public void EndRevive()
	{
		Packet packet = new Packet();
		packet.type = 1;
		packet.id = entity.id;
		packet.action = new ActionPacket(11);

		NetworkManager.Instance.SendMessage(packet);
	}
}
