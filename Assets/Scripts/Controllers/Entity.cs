using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ID 0-3 - player
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
//ID 44-10000 is an interactable
//ID 10001-20000 is a water projectile
//ID 20001-30000 is a bubble projectile
//ID 30001-40000 is a dart projectile
//ID 40001-50000 is a other projectile
//ID of 65535 is invalid

public class Entity : MonoBehaviour
{
	[HideInInspector] public ushort id;

	private Vector3 previousPosition;
	private Vector3 targetPosition;
	private float targetRotation;

	public Transform shoot;
	[HideInInspector] public GameObject model;
	[HideInInspector] public Animator animator;

	[SerializeField] bool inLobby = false;
	private bool quitting = false;

	private void Awake()
	{
		targetPosition = transform.position;
		targetRotation = transform.eulerAngles.y;
	}

	private void Update()
	{
		if ((id < 4 && GameManager.Instance.ThisPlayer != id) || (id > 3 && !GameManager.Instance.IsServer))
		{
			transform.position = Vector3.Lerp(transform.position, targetPosition, 0.3f);
			//transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.3f);
		}

		if (!inLobby && animator != null && transform.position != previousPosition)
		{
			float speed = (transform.position - previousPosition).magnitude / Time.deltaTime;

			animator.SetFloat("Speed", Mathf.Abs(speed));

			previousPosition = transform.position;
		}
	}

	public void SetTransform(TransformPacket tp)
	{
		targetPosition = new Vector3(tp.xPos, tp.yPos, tp.zPos);
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
			model = Instantiate(GameManager.Instance.PrefabManager.RandomCommon(), transform);
		}
		else if (id < 44)
		{
			model = Instantiate(GameManager.Instance.PrefabManager.SpecialModels[id - 34], transform);
		}
		else if (id < 20001) // Water Projectile
		{
			//TODO: Set Item Model
		}
		else if (id < 20001) // Bubble Projectile
		{
			//TODO: Set Item Model
		}
		else if (id < 20001) // Dart Projectile
		{
			//TODO: Set Item Model
		}
		else if (id < 20001) // Other Projectile
		{
			//TODO: Set Item Model
		}

		model.TryGetComponent(out animator);
	}

	//TODO: Health, Inventory

	public void DoAction(ActionPacket packet)
	{
		if (id < 4)
		{
			switch (packet.data)
			{
				case 0: //Sprint
				{
					CapsuleCollider collider = GetComponent<CapsuleCollider>();
					collider.height = 2.2f;
					collider.radius = 0.3f;
					collider.center = Vector3.up * 1.1f;
					animator.SetTrigger("Sprint");
				}
				break;
				case 1: //Crawl
				{
					CapsuleCollider collider = GetComponent<CapsuleCollider>();
					collider.height = 1.0f;
					collider.radius = 0.5f;
					collider.center = Vector3.up * 0.5f;
					animator.SetTrigger("Crawl");
				}
				break;
				case 2: //Idle
				{
					animator.SetTrigger("Idle");
				}
				break;
				case 4: //Pushing
				{

				}
				break;
			}
		}
		else if (id < 34)
		{
			switch (packet.data)
			{
				case 0: //Attack
				{
					animator.SetBool("isAttacking", true);
				}
				break;
				case 1: //Attack
				{
					animator.SetBool("isAttacking", false);
				}
				break;
				case 2: //Death
				{
					animator.SetTrigger("isDead");
				}
				break;
			}
		}
		else if (id < 44)
		{

		}	
		else if (id < 10001)
		{
			if(packet.data == 0) { GetComponent<Pushable>().OtherPush(); }
			else { GetComponent<Pushable>().OtherStop(); }
		}
	}

	private void OnApplicationQuit()
	{
		quitting = true;
	}

	private void OnDestroy()
	{
		if (!quitting)
		{
			GameManager.Instance.Destroy(this);
		}
	}
}
