using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Entity))]
public class Health : MonoBehaviour
{
	[SerializeField] private float maxHealth = 100;
	public float MaxHealth { get { return maxHealth; } }

	[SerializeField] private float maxTrauma = 50;
	public float MaxTrauma { get { return maxTrauma; } }

	[SerializeField] private float maxDown = 100;
	public float MaxDown { get { return maxDown; } }

	[SerializeField] private float healthVal;
	[SerializeField] private float traumaVal;
	[SerializeField] private float downVal;
	private float decayRate;

	private HealthBar healthBar;
	private Entity entity;

	public float health { get { return healthVal; } set { 
			healthVal = Mathf.Clamp(value, 0, maxHealth - trauma); 
			if (healthBar != null) { healthBar.SetTantrumPercent(1.0f - healthVal / maxHealth); } 
		} }
	public float trauma { get { return traumaVal; } set {
			traumaVal = Mathf.Clamp(value, 0, maxTrauma);
			if (healthBar != null) { healthBar.SetTemperPercent(traumaVal / maxHealth); }
		} }

	public float down { get { return downVal;} set
		{
			downVal = Mathf.Clamp(value, 0, maxDown);
			if (healthBar != null) { healthBar.SetDownPercent(downVal / maxDown); }
		} }

	private void Awake()
	{
		entity = GetComponent<Entity>();
		health = maxHealth;
		traumaVal = 0;
		downVal = 0;
	}

	private void Update()
	{
		if (decayRate > 0.0f) { OnDamaged(decayRate); }
	}

	public void Revive(float health)
	{
		this.health = health;
		down = 0;

		Packet packet = new Packet();
		packet.type = 2;
		packet.id = entity.id;
		packet.health = new HealthPacket((byte)healthVal, (byte)traumaVal, (byte)downVal);
		NetworkManager.Instance.SendMessage(packet);
	}

	public void OnDamaged(float damage)
	{
		health -= damage;

		if (entity.id < 4)
		{
			Packet packet = new Packet();
			packet.type = 2;
			packet.id = entity.id;
			packet.health = new HealthPacket((byte)healthVal, (byte)traumaVal, (byte)downVal);
			NetworkManager.Instance.SendMessage(packet);
		}

		GameManager.Instance.HudManager.SpawnTear(Random.Range(1, 4));
	}

	public void OnTrauma(float traumaDamage)
	{
		trauma += traumaDamage;

		Packet packet = new Packet();
		packet.type = 2;
		packet.id = entity.id;
		packet.health = new HealthPacket((byte)healthVal, (byte)traumaVal, (byte)downVal);
		NetworkManager.Instance.SendMessage(packet);
	}

	public void OnDown()
	{
		down = maxDown;

		Packet packet = new Packet();
		packet.type = 2;
		packet.id = entity.id;
		packet.health = new HealthPacket((byte)healthVal, (byte)traumaVal, (byte)downVal);
		NetworkManager.Instance.SendMessage(packet);
	}

	public void OnDownDamage(float downDamage)
	{
		down -= downDamage;

		Packet packet = new Packet();
		packet.type = 2;
		packet.id = entity.id;
		packet.health = new HealthPacket((byte)healthVal, (byte)traumaVal, (byte)downVal);
		NetworkManager.Instance.SendMessage(packet);
	}

	public void Decay(int dps)
	{
		decayRate = dps;
	}

	public void AttachHealthBar(HealthBar healthBar)
	{
		this.healthBar = healthBar;
		healthBar.SetTantrumPercent(1.0f - healthVal / maxHealth);
		healthBar.SetTemperPercent(traumaVal / maxHealth);
		healthBar.SetDownPercent(downVal / maxDown);
	}
}
