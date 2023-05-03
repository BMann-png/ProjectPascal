using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Entity))]
public class Health : MonoBehaviour
{
	[SerializeField] private float maxHealth = 100;
	[SerializeField] private float maxTrauma = 50;
	[SerializeField] private float healthVal;
	[SerializeField] private float traumaVal;
	private float decayRate;

	private HealthBar healthBar;

	public float health { get { return healthVal; } set { 
			healthVal = Mathf.Clamp(value, 0, maxHealth); 
			if (healthBar != null) { healthBar.SetTantrumPercent(healthVal / maxHealth); } 
		} }
	public float trauma { get { return traumaVal; } set {
			traumaVal = Mathf.Clamp(value, 0, maxTrauma);
			if (healthBar != null) { healthBar.SetTemperPercent(traumaVal / maxHealth); }
		} }

	private void Awake()
	{
		health = maxHealth;
		traumaVal = 0;
	}

	private void Update()
	{
		if (decayRate > 0.0f) { OnDamaged(decayRate); }
	}

	public void OnDamaged(float damage)
	{
		health -= damage;

		Packet packet = new Packet();
		packet.type = 2;
		packet.health = new HealthPacket((byte)healthVal, (byte)traumaVal);
		NetworkManager.Instance.SendMessage(packet);
	}

	public void OnTrauma(float traumaDamage)
	{
		trauma += traumaDamage;

		Packet packet = new Packet();
		packet.type = 2;
		packet.health = new HealthPacket((byte)healthVal, (byte)traumaVal);
		NetworkManager.Instance.SendMessage(packet);
	}

	public void Decay(int dps)
	{
		decayRate = dps;
	}

	public void AttachHealthBar(HealthBar healthBar)
	{
		this.healthBar = healthBar;
		healthBar.SetTantrumPercent(healthVal / maxHealth);
		healthBar.SetTemperPercent(traumaVal / maxHealth);
	}
}
