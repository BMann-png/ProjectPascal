using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

[RequireComponent(typeof(Entity))]
public class Inventory : MonoBehaviour
{
    [SerializeField] private List<GameObject> primaryWeapons;
    [SerializeField] private List<GameObject> secondaryWeapons;
	[SerializeField] private GameManager pacifier;

	private Entity entity;

    private byte primaryIndex = 255;
    private byte secondaryIndex = 255;

    private bool hasPacifier = false;
	private bool hasMission = false;

	private byte equippedItem = 0;

	private void Awake()
	{
		entity = GetComponent<Entity>();
	}

	private void Update()
	{
		if (entity.id == GameManager.Instance.thisPlayer)
		{
			float scroll = Input.GetAxis("Scroll");
			if (scroll < 0.0f) { equippedItem = (byte)(++equippedItem % 3); }
			else if (scroll > 0.0f) { equippedItem = (byte)(--equippedItem % 3); }

			if (Input.GetKeyDown(KeyCode.Alpha1)) { equippedItem = 0; }
			else if (Input.GetKeyDown(KeyCode.Alpha2)) { equippedItem = 1; }
			else if (Input.GetKeyDown(KeyCode.Alpha3)) { equippedItem = 2; }

			Packet packet = new Packet();
			packet.type = 3;
			packet.id = entity.id;
			packet.inventory = new InventoryPacket(equippedItem, 255);

			NetworkManager.Instance.SendMessage(packet);
		}
	}

	public void EquipWeapon(byte slot, byte id)
	{
		switch(slot)
		{
			case 0: primaryIndex = id; break;
			case 1: secondaryIndex = id; break;
			case 2: hasPacifier = id > 0; break;
			case 3: hasMission = id > 0; break;
		}

		Packet packet = new Packet();
		packet.type = 3;
		packet.id = entity.id;
		packet.inventory = new InventoryPacket(equippedItem, id);

		NetworkManager.Instance.SendMessage(packet);
	}

	public void SetWeapon(byte slot, byte id)
	{
		switch (slot)
		{
			case 0: primaryIndex = id; break;
			case 1: secondaryIndex = id; break;
			case 2: hasPacifier = id > 0; break;
			case 3: hasMission = id > 0; break;
		}
	}

	public void Switch(byte slot)
	{
		equippedItem = slot;
	}
}
