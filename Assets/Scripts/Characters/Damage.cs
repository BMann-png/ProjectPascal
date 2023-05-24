using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[RequireComponent(typeof(Entity))]
public class Damage : MonoBehaviour
{
    public GameObject Owner { get; set; }

    public bool dealsDamage = true;
    [SerializeField] bool damageOverTime = false;
    [SerializeField] int damage = 0;
    [SerializeField] int trauma = 0;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == Owner) return;

        Health collisionHealth;
		if (!dealsDamage || !collision.gameObject.TryGetComponent(out collisionHealth)) { return; }

        if (damageOverTime)
        {
            collisionHealth.Decay(damage);
        }
        else
		{
			collisionHealth.OnDamaged(damage);
			Destroy(Instantiate(GameManager.Instance.PrefabManager.Particles[3], transform.position, transform.rotation), 1.0f);
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
