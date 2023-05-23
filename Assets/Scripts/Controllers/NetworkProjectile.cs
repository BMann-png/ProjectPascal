using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkProjectile : MonoBehaviour
{
	[SerializeField] private int type;

	private void OnDestroy()
	{
		Destroy(Instantiate(GameManager.Instance.PrefabManager.Particles[type], transform.position, transform.rotation), 1.0f);
	}
}
