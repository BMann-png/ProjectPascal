using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class Projectile : MonoBehaviour
{
	private static readonly float LIFETIME = 10.0f;

	private new Rigidbody rigidbody;
	private Entity entity;
	private float timer;


	private void Awake()
	{
		rigidbody= GetComponent<Rigidbody>();
		entity = GetComponent<Entity>();
		timer = LIFETIME;
	}

	private void FixedUpdate()
	{
		Packet packet = new Packet();
		packet.type = 0;
		packet.id = entity.id;
		packet.transform = new TransformPacket(transform);

		NetworkManager.Instance.SendMessage(packet);
	}

	private void Update()
    {
        timer -= Time.deltaTime;

		if(timer <= 0.0f)
		{
			Destroy(gameObject);
		}
    }

	public void SetSpeed(float speed)
	{
		rigidbody.AddForce(Vector3.forward * speed, ForceMode.VelocityChange);
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.layer == LayerMask.GetMask("Enemy"))
		{
			//collision.gameObject.GetComponent<EnemyController>().Damage();
		}
		else if(collision.gameObject.layer == LayerMask.GetMask("Ground"))
		{
			Destroy(gameObject);
		}
	}
}
