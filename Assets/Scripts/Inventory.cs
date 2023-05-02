using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Entity))]
public class Inventory : MonoBehaviour
{
    [SerializeField] private List<GameObject> primaryWeapons;
    [SerializeField] private List<GameObject> secondaryWeapons;
	[SerializeField] private GameManager pacifier;

	private Entity entity;

	private GameObject curPrimary = null;
	private GameObject curSecondary = null;

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

			EquipWeapon(0, equippedItem);

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
				if (curPrimary != null) curPrimary.SetActive(false);
				curPrimary = primaryWeapons[primaryIndex];
                curPrimary.SetActive(true);
                break;
			case 1:
                if (secondaryIndex == id) return;
                secondaryIndex = id;
                if (curSecondary != null) curSecondary.SetActive(false);
                curSecondary = secondaryWeapons[secondaryIndex];
                curSecondary.SetActive(true);
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
