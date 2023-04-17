using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Entity))]
public class DamageModel : MonoBehaviour
{
    public const int MIN_HEALTH = 0, MAX_HEALTH = 100;

    [SerializeField] int health;
    [SerializeField] int damage;
    [SerializeField] int decayRate;


    public int Health { get { return health; } private set { health = Mathf.Clamp(value, MIN_HEALTH, MAX_HEALTH); } }
    public int Damage { get { return damage; } private set { damage = value; } }
    public int DecayRate { get { return decayRate; } private set { decayRate = value; } }

    public bool isDamaging = true;

    private void FixedUpdate()
    {
        Health -= DecayRate;
    }

    public void OnDamaged(int damage)
    {
        if (isDamaging)
        {
            Health -= damage;

            Packet packet = new Packet();
            packet.type = 2;
            packet.health = new HealthPacket();
            packet.health.data = (byte)Health;
            NetworkManager.Instance.SendMessage(packet);
        }
    }
}
