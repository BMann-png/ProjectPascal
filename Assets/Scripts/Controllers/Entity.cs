using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class Entity : MonoBehaviour
{
	[HideInInspector] public byte id;   //ID 0-3 is a player
										//ID 4-38 is an enemy
										//ID 39-48 is an objective
										//ID 49-254 is a projectile
										//ID of 255 is invalid
	[HideInInspector] public byte type; //Type decides what model/kind of entity this is
										//Type 0 - Player model 1
										//Type 1 - Player model 2
										//Type 2 - Player model 3
										//Type 3 - Player model 4
										//Type 4 - Common
										//Type 5 - Spitter
										//Type 6 - Alarmer
										//Type 7 - Lurker
										//Type 8 - Hurler
										//Type 9 - Snatcher
										//Type 10 - Currupted Common
										//Type 11 - Currupted Spitter
										//Type 12 - Currupted Alarmer
										//Type 13 - Currupted Lurker
										//Type 14 - Currupted Hurler
										//Type 15 - Currupted Snatcher
										//Type 16 - Projectile

	private Vector3 targetPosition;
	private float targetRotation;

	public Transform shoot;
	public GameObject model;

	private void Awake()
	{
		targetPosition = transform.position;
		targetRotation = transform.eulerAngles.y;
	}

	private void Update()
	{
		if ((id < 4 && GameManager.Instance.thisPlayer != id) || (id > 3 && !GameManager.Instance.IsServer))
		{
			transform.position = Vector3.Lerp(transform.position, targetPosition, 0.3f);
			//transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.3f);
		}
	}

	public void SetTransform(TransformPacket tp)
	{
		targetPosition = new Vector3(tp.xPos, tp.yPos, tp.zPos);
		targetRotation = tp.yRot;

		if (type == 16)
		{
			transform.eulerAngles = new Vector3(tp.xRot, tp.yRot, 0.0f); //TODO: Lerp smoothly
		}
		else
		{
			transform.eulerAngles = new Vector3(0.0f, tp.yRot, 0.0f); //TODO: Lerp smoothly

			if (shoot != null)
			{
				shoot.eulerAngles = new Vector3(tp.xRot, tp.yRot, 0.0f);
			}
		}
	}

	public void SetModel()
	{
		//TODO: Check for existing model

		if (id < 4)
		{
			model = Instantiate(GameManager.Instance.PrefabManager.PlayerModels[id], transform);
		}
		else if (id < 15)
		{
			model = Instantiate(GameManager.Instance.PrefabManager.EnemyModels[id - 4], transform);
		}
	}

	//TODO: Health, Inventory

	public void DoAction(ActionPacket packet)
	{
		if (id < 4)
		{
			switch (packet.data)
			{
				case 0:
					{
						model.transform.localPosition = Vector3.zero;
						model.transform.localRotation = Quaternion.identity;
						CapsuleCollider collider = GetComponent<CapsuleCollider>();
						collider.height = 2.2f;
						collider.radius = 0.3f;
						collider.center = Vector3.up * 1.1f;
					}
					break;
				case 1:
					{
						model.transform.localPosition = new Vector3(0.0f, 0.25f, -1.0f);
						model.transform.localRotation = Quaternion.AngleAxis(90, Vector3.right);
						CapsuleCollider collider = GetComponent<CapsuleCollider>();
						collider.height = 1.0f;
						collider.radius = 0.5f;
						collider.center = Vector3.up * 0.5f;
					}
					break;
			}
		}
		else if (id < 15)
		{
			GetComponent<EnemyController>().ChangeColor(packet.data);
		}
	}
}
