using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Entity))]
public class Damage : MonoBehaviour
{
    [SerializeField] bool DamageOverTime = false;
    [SerializeField] int damage = 0;

    private void OnCollisionEnter(Collision collision)
    {
        Health collisionHealth;
        if (!collision.gameObject.TryGetComponent(out collisionHealth)) return;

        if (DamageOverTime)
        {
            collisionHealth.decayRate = damage;
        }
        else
        {
            collisionHealth.OnDamaged(damage);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        Health collisionHealth;
        if (DamageOverTime && !collision.gameObject.TryGetComponent(out collisionHealth))
        {
            collisionHealth.decayRate = 0;
        }
    }
}
