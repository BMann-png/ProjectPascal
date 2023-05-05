using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Interactable), typeof(Entity), typeof(Collider))]
public class Pickup : MonoBehaviour
{
	private Entity entity;

	private void Awake()
	{
		entity = GetComponent<Entity>();
	}

	public void PickupItem()
	{
		Destroy(gameObject);
		Debug.Log("Pickup");
	}
}
