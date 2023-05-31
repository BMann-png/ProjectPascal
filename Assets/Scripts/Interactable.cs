using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Outline), typeof(Collider))]
public class Interactable : MonoBehaviour
{
	public bool hold = false;
	public bool canInteract = true;
	public byte id;
	public UnityEvent onInteract;
	public UnityEvent onStopInteract;
	[HideInInspector] public UnityEvent onInteractOther;
	[HideInInspector] public UnityEvent onStopInteractOther;

	[HideInInspector] public Outline outline;

	private void Awake()
	{
		outline = GetComponent<Outline>();
		outline.enabled = false;
		gameObject.layer = 10;
	}

	public void SetEvents(UnityEvent onInteract, UnityEvent onStopInteract, UnityEvent onComplete)
	{
		onInteractOther = onInteract;
		onStopInteractOther = onStopInteract;
		if(TryGetComponent(out Pushable p)) { p.onCompleteOther = onComplete; }
	}

	public void EnableInteract(bool b)
	{
		canInteract = b;
	}
}
