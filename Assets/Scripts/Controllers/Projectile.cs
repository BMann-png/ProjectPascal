using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;


[RequireComponent(typeof(Entity))]
public class Projectile : MonoBehaviour
{
	private static readonly float LIFETIME = 5.0f;
	private static LayerMask ENEMY_MASK;
	private static LayerMask GROUND_MASK;

	private new Rigidbody rigidbody;
	private Entity entity;
	private float timer;
	bool dealsDamage;

	private void Awake()
	{
		rigidbody = GetComponent<Rigidbody>();
		entity = GetComponent<Entity>();
		timer = LIFETIME;
		ENEMY_MASK = LayerMask.GetMask("Enemy");
		GROUND_MASK = LayerMask.GetMask("Ground");
	}

	private void FixedUpdate()
	{
		if (GameManager.Instance.Network)
		{
			Packet packet = new Packet();
			packet.type = 0;
			packet.id = entity.id;
			packet.transform = new TransformPacket(transform, transform.eulerAngles.x);

			NetworkManager.Instance.SendMessage(packet);
		}
	}

	private void Update()
    {
        timer -= Time.deltaTime;

		if(timer <= 0.0f)
		{
			GameManager.Instance.Destroy(entity);
		}
    }

	public void SetSpeed(float speed)
	{
		dealsDamage = true;
		rigidbody.AddForce(transform.up * speed, ForceMode.VelocityChange);
	}

	private void OnCollisionEnter(Collision collision)
	{
		//TODO: Darts should stick if normal is within a range

		if (dealsDamage && 1 << collision.gameObject.layer == ENEMY_MASK.value)
		{
			//TODO: Deal damage
			GameManager.Instance.Destroy(entity);
			dealsDamage = false;
		}
		else if (1 << collision.gameObject.layer == GROUND_MASK.value)
		{
			dealsDamage = false;
		}
	}
}
