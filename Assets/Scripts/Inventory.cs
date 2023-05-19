using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Entity))]
public class Inventory : MonoBehaviour
{
	[SerializeField] private List<GameObject> primaryWeapons;
	[SerializeField] private List<GameObject> secondaryWeapons;
	[SerializeField] private GameManager pacifier;

	private Entity entity;

	[SerializeField] private byte primaryIndex = 255;
	[SerializeField] private byte secondaryIndex = 0;

	private bool hasPacifier = false;
	private bool hasMission = false;

	private byte equipItem = 0;
	private byte prevEquipItem = 255;

	private float scrollTime = 0.1f;
	private float scrollTimer = 0.0f;

	private Weapon weapon = null;

	private void Awake()
	{
		entity = GetComponent<Entity>();
		SetupPacket(1, secondaryIndex);
	}

	private void Update()
	{
		if (entity.id == GameManager.Instance.ThisPlayer)
		{
			scrollTimer -= Time.deltaTime;

			prevEquipItem = equipItem;

			if (scrollTimer > 0.0f || weapon == null || weapon.IsFiring) return;

			float scroll = Input.GetAxis("Mouse ScrollWheel");
			if (scroll < 0.0f)
			{
				byte newEquip = (byte)((equipItem + 1) % 3);

				switch (newEquip)
				{
					case 0: if (primaryIndex < 255) { equipItem = 0; } else if (secondaryIndex < 255) { equipItem = 1; } else { equipItem = 2; } break;
					case 1: if (secondaryIndex < 255) { equipItem = 1; } else if (hasPacifier) { equipItem = 2; } else { equipItem = 0; } break;
					case 2: if (hasPacifier) { equipItem = 2; } else if (primaryIndex < 255) { equipItem = 0; } else { equipItem = 1; } break;
				}
			}
			else if (scroll > 0.0f)
			{
				byte newEquip;
				if (equipItem == 0) { newEquip = 2; }
				else { newEquip = (byte)(equipItem - 1); }

				switch (newEquip)
				{
					case 0: if (primaryIndex < 255) { equipItem = 0; } else if (hasPacifier) { equipItem = 2; } else { equipItem = 1; } break;
					case 1: if (secondaryIndex < 255) { equipItem = 1; } else if (primaryIndex < 255) { equipItem = 0; } else { equipItem = 2; } break;
					case 2: if (hasPacifier) { equipItem = 2; } else if (secondaryIndex < 255) { equipItem = 1; } else { equipItem = 0; } break;
				}
			}

			if (Input.GetKeyDown(KeyCode.Alpha1) && primaryIndex < 255) { equipItem = 0; }
			else if (Input.GetKeyDown(KeyCode.Alpha2) && secondaryIndex < 255) { equipItem = 1; }
			else if (Input.GetKeyDown(KeyCode.Alpha3) && hasPacifier) { equipItem = 2; }

			if (prevEquipItem != equipItem)
			{
				scrollTimer = scrollTime;
				byte id = 255;

				switch (equipItem)
				{
					case 0:
						id = primaryIndex;
						break;
					case 1:
						id = secondaryIndex;
						break;
					case 2:
						id = 0;
						break;
				}

				SetupPacket(equipItem, id);
			}
		}
	}

	public void EquipWeapon(byte slot)
	{
		switch (prevEquipItem)
		{
			case 0:
				if (primaryIndex != 255) { primaryWeapons[primaryIndex].SetActive(false); }
				break;
			case 1:
				if (secondaryIndex != 255) { secondaryWeapons[secondaryIndex].SetActive(false); }
				break;
			case 2:
				//TODO: Pacifier
				break;
		}

		switch (slot)
		{
			case 0:
				if (primaryIndex == 255) { return; }
				primaryWeapons[primaryIndex].SetActive(true);
				weapon = primaryWeapons[primaryIndex].GetComponent<Weapon>();
				entity.shoot = weapon.shoot;
				break;
			case 1:
				if (secondaryIndex == 255) { return; }
				secondaryWeapons[secondaryIndex].SetActive(true);
				weapon = secondaryWeapons[secondaryIndex].GetComponent<Weapon>();
				entity.shoot = weapon.shoot;
				break;
			case 2:
				//TODO: Pacifier
				break;
		}

		equipItem = slot;
	}

	public void EquipWeapon(byte slot, byte id)
	{
		switch (prevEquipItem)
		{
			case 0:
				if (primaryIndex != 255) { primaryWeapons[primaryIndex].SetActive(false); }
				break;
			case 1:
				if (secondaryIndex != 255) { secondaryWeapons[secondaryIndex].SetActive(false); }
				break;
			case 2:
				//TODO: Pacifier
				break;
		}

		if (id == 255) { return; }

		switch (slot)
		{
			case 0:
				primaryIndex = id;
				primaryWeapons[primaryIndex].SetActive(true);
				weapon = primaryWeapons[primaryIndex].GetComponent<Weapon>();
				entity.shoot = weapon.shoot;
				break;
			case 1:
				secondaryIndex = id;
				secondaryWeapons[secondaryIndex].SetActive(true);
				weapon = secondaryWeapons[secondaryIndex].GetComponent<Weapon>();
				entity.shoot = weapon.shoot;
				break;
			case 2:
				//TODO: Pacifier
				break;
		}
	}

	public void SetWeapon(byte slot, byte id)
	{
		switch (equipItem)
		{
			case 0:
				if (primaryIndex != 255) { primaryWeapons[primaryIndex].SetActive(false); }
				break;
			case 1:
				if (secondaryIndex != 255) { secondaryWeapons[secondaryIndex].SetActive(false); }
				break;
			case 2:
				//TODO: Pacifier
				break;
		}

		switch (slot)
		{
			case 0:
				if (primaryIndex == id) { return; }
				primaryIndex = id;
				SetupPacket(slot, primaryIndex);
				break;
			case 1:
				if (secondaryIndex == id) { return; }
				secondaryIndex = id;
				SetupPacket(slot, secondaryIndex);
				break;
			case 2:
				if (id > 0 == hasPacifier) { return; }
				hasPacifier = id > 0;
				break;
			case 3:
				if (id > 0 == hasMission) { return; }
				hasMission = id > 0;
				break;
		}
	}

	public void Switch(byte slot)
	{
		equipItem = slot;
	}

	private void SetupPacket(byte slot, byte id)
	{
		EquipWeapon(slot);

		Packet packet = new Packet();
		packet.type = 3;
		packet.id = entity.id;
		packet.inventory = new InventoryPacket(slot, id);

		NetworkManager.Instance.SendMessage(packet);
	}

	public Weapon GetCurrentWeapon()
	{
		switch (equipItem)
		{
			case 0:
				return primaryWeapons[primaryIndex].GetComponent<Weapon>();
			case 1:
				return secondaryWeapons[secondaryIndex].GetComponent<Weapon>();
			case 2:
				// TODO: Zach, do your damn job! gimme me binki D:
				break;
		}
		return null;
	}
}
