using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Entity))]
public class Health : MonoBehaviour
{
    public const int MIN_HEALTH = 0;

    [SerializeField] int maxHealth = 100;
    [SerializeField] int healthVal;
    public int decayRate;

    public int health { get { return healthVal; } set { healthVal = Mathf.Clamp(value, MIN_HEALTH, maxHealth); } }

	private void Awake()
	{
        health = maxHealth;
	}

	private void FixedUpdate()
    {
        health -= decayRate;
    }

    public void OnDamaged(int damage)
    {
        health -= damage;

        Packet packet = new Packet();
        packet.type = 2;
        packet.health = new HealthPacket();
        packet.health.data = (byte)health;
        NetworkManager.Instance.QueueMessage(packet);

        if(health <= 0)
        {
            Debug.Log("Ded");
        }
    }
}
