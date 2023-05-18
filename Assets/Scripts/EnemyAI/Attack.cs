using UnityEngine;

public class Attack : MonoBehaviour
{
	private LayerMask attackMask;

	private void Awake()
	{
		attackMask = LayerMask.GetMask("Player");
	}

	public void OnAttack()
	{
		Quaternion rot = Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up);

		Collider[] colliders = Physics.OverlapBox(transform.position + rot * Vector3.forward, new Vector3(0.75f, 0.75f, 0.45f), rot, attackMask.value);

		foreach (Collider collider in colliders)
		{
			if (collider.gameObject.TryGetComponent(out Health health))
			{
				if (health.down > 0) { health.OnDownDamage(3); }
				else { health.OnDamaged(10); }
				GameManager.Instance.AudioManager.Source.PlayOneShot
					(GameManager.Instance.AudioManager.GetPunch());
			}
		}
	}
}
