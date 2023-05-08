using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Interactable), typeof(Entity))]
public class Animate : MonoBehaviour
{
	[SerializeField] bool canInteract = true;
	[SerializeField] private new Animation animation;

	private Interactable interactable;

	private void Awake()
	{
		interactable = GetComponent<Interactable>();

		interactable.canInteract = canInteract;
		interactable.onInteract.RemoveAllListeners();
		interactable.onInteract.AddListener(StartAnimation);
	}

	public void StartAnimation()
	{
		animation.Play();
	}
}
