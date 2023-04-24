using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class Entity : MonoBehaviour
{
	[HideInInspector] public byte id;   //ID 0-3 - player
										//ID 4-33 - common enemy
										//ID 34 - Spitter
										//ID 35 - Alarmer
										//ID 36 - Lurker
										//ID 37 - Hurler
										//ID 38 - Snatcher
										//ID 39 - Corrupted Spitter
										//ID 40 - Corrupted Alarmer
										//ID 41 - Corrupted Lurker
										//ID 42 - Corrupted Hurler
										//ID 43 - Corrupted Snatcher
										//ID 44-48 is an objective
										//ID 49-254 is a projectile
										//ID of 255 is invalid

	private Vector3 targetPosition;
	private Vector3 movement;
	private float targetRotation;

	public Transform shoot;
	[HideInInspector] public GameObject model;

	private void Awake()
	{
		targetPosition = transform.position;
		targetRotation = transform.eulerAngles.y;
	}

	private void FixedUpdate()
	{
		if ((id < 4 && GameManager.Instance.ThisPlayer != id) || (id > 3 && !GameManager.Instance.IsServer))
		{
			transform.position += movement;
			//transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.3f);
		}
	}

	public void SetTransform(TransformPacket tp)
	{
		targetPosition = new Vector3(tp.xPos, tp.yPos, tp.zPos);
		movement = (targetPosition - transform.position) / 60.0f;
		targetRotation = tp.yRot;

		if (id > 48)
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
		else if (id < 34)
		{
			model = Instantiate(GameManager.Instance.PrefabManager.EnemyModels[0], transform);
		}
		else
		{
			model = Instantiate(GameManager.Instance.PrefabManager.EnemyModels[id - 33], transform);
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
		else if (id < 39)
		{

		}
	}
}
