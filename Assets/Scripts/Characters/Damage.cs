using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Entity))]
public class Damage : MonoBehaviour
{
    public bool dealsDamage = true;
    [SerializeField] bool damageOverTime = false;
    [SerializeField] int damage = 0;
    [SerializeField] int trauma = 0;

    private void OnCollisionEnter(Collision collision)
    {
        Health collisionHealth;
		if (!dealsDamage || !collision.gameObject.TryGetComponent(out collisionHealth)) { return; }

        if (damageOverTime)
        {
            collisionHealth.Decay(damage);
        }
        else
        {
            collisionHealth.OnDamaged(damage);
        }

		if(trauma > 0)
		{
			collisionHealth.OnTrauma(trauma);
		}
    }

    private void OnCollisionExit(Collision collision)
    {
        Health collisionHealth;
        if (damageOverTime && !collision.gameObject.TryGetComponent(out collisionHealth))
        {
            collisionHealth.Decay(0);
        }
    }
}
