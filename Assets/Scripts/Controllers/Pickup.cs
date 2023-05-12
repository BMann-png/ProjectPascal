using UnityEngine;

[RequireComponent(typeof(Interactable), typeof(Entity))]
public class Pickup : MonoBehaviour
{
	[SerializeField] private byte id;

	[HideInInspector] private bool destroyOnPickup = true;

	public void PickupItem()
	{
		GameManager.Instance.PickupItem(id);

		if (destroyOnPickup) { Destroy(gameObject); }
	}
}
