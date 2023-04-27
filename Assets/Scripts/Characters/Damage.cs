using UnityEngine;

[RequireComponent(typeof(Entity))]
public class Damage : MonoBehaviour
{
    public bool dealsDamage = true;
    [SerializeField] bool damageOverTime = false;
    [SerializeField] int damage = 0;

    private void OnCollisionEnter(Collision collision)
    {
        Health collisionHealth;
        if (!dealsDamage || !collision.gameObject.TryGetComponent(out collisionHealth)) return;

        if (damageOverTime)
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
        if (damageOverTime && !collision.gameObject.TryGetComponent(out collisionHealth))
        {
            collisionHealth.decayRate = 0;
        }
    }
}
