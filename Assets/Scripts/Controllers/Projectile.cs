using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[RequireComponent(typeof(Entity))]
public class Projectile : MonoBehaviour
{
	[SerializeField] private float speed = 100f;
	[SerializeField] private float gravityModifier = -9.81f;
	[SerializeField] private bool destoryOnCollide = false;

	private static readonly float LIFETIME = 5.0f;
	private static LayerMask ENEMY_MASK;
	private static LayerMask GROUND_MASK;
	private static LayerMask PLAYER_MASK;

	private new Rigidbody rigidbody;
	private Entity entity;
	private Damage damage;

	private float timer;

	private bool hasDamage;

	private void Awake()
	{
		rigidbody = GetComponent<Rigidbody>();
		entity = GetComponent<Entity>();
		hasDamage = TryGetComponent(out damage);
		timer = LIFETIME;
		ENEMY_MASK = LayerMask.GetMask("Enemy");
		GROUND_MASK = LayerMask.GetMask("Ground");
		PLAYER_MASK = LayerMask.GetMask("Player");
	}

	private void FixedUpdate()
	{
		Packet packet = new Packet();
		packet.type = 0;
		packet.id = entity.id;
		packet.transform = new TransformPacket(transform, transform.eulerAngles.x);

		NetworkManager.Instance.SendMessage(packet);

		if (rigidbody.useGravity) return;
		rigidbody.AddForce(0, gravityModifier * Time.deltaTime, 0, ForceMode.VelocityChange);
	}

	private void Update()
    {
        timer -= Time.deltaTime;

		if (timer <= 0.0f)
		{
			Destroy(gameObject);
		}
    }

    public void SetSpeed()
	{
		rigidbody.AddForce(transform.up * speed, ForceMode.VelocityChange);
	}
	
	private void OnCollisionEnter(Collision collision)
	{
		if (1 << collision.gameObject.layer == PLAYER_MASK.value) return;

        //TODO: Darts should stick if normal is within a range
        if (!hasDamage) return;

		if (damage.dealsDamage && 1 << collision.gameObject.layer == ENEMY_MASK.value)
		{
			//TODO: Deal damage
			Destroy(gameObject);
			damage.dealsDamage = false;
		}
		else if (1 << collision.gameObject.layer == GROUND_MASK.value)
		{
			damage.dealsDamage = false;
		}

		if (destoryOnCollide) 
		{ 
            Destroy(gameObject);
		}
    }
}
