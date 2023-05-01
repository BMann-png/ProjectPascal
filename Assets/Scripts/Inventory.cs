using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

[RequireComponent(typeof(Entity))]
public class Inventory : MonoBehaviour
{
	[SerializeField] private GameObject hand;
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
		if (entity.id == GameManager.Instance.ThisPlayer)
		{
			float scroll = Input.GetAxis("Mouse ScrollWheel");
			if (scroll < 0.0f) { equippedItem = (byte)(++equippedItem % 3); }
			else if (scroll > 0.0f) { equippedItem = (byte)(--equippedItem % 3); }

			if (Input.GetKeyDown(KeyCode.Alpha1)) { equippedItem = 0; }
			else if (Input.GetKeyDown(KeyCode.Alpha2)) { equippedItem = 1; }
			else if (Input.GetKeyDown(KeyCode.Alpha3)) { equippedItem = 2; }

			EquipWeapon(equippedItem, 0);

			Packet packet = new Packet();
			packet.type = 3;
			packet.id = entity.id;

			packet.inventory = new InventoryPacket(equippedItem, 255);

			NetworkManager.Instance.SendMessage(packet);
		}
	}

	public void EquipWeapon(byte slot, byte id)
	{
		SetWeapon(slot, id);

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
			case 0:
				if (primaryIndex == id) return;
				primaryIndex = id;
				//if (hand.GetComponentInChildren<> != null) Destroy(hand.GetComponentInChildren<Weapon>().gameObject);
				Instantiate(primaryWeapons[primaryIndex], hand.transform);
                break;
			case 1:
                if (secondaryIndex == id) return;
                secondaryIndex = id;
                //if (hand.GetComponentInChildren<Weapon>().gameObject != null) Destroy(hand.GetComponentInChildren<Weapon>().gameObject);
                Instantiate(secondaryWeapons[secondaryIndex], hand.transform);
                break;
			case 2:
                if (id > 0 == hasPacifier) return;
                hasPacifier = id > 0; 
				break;
			case 3:
                if (id > 0 == hasMission) return;
                hasMission = id > 0; 
				break;
		}
    }

	public void Switch(byte slot)
	{
		equippedItem = slot;
	}
}
