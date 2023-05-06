using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Outline), typeof(Collider))]
public class Interactable : MonoBehaviour
{
	public bool hold = false;
	public bool canInteract = true;
	public UnityEvent onInteract;
	public UnityEvent onStopInteract;

	[HideInInspector] public Outline outline;

	//Reviving - Gotta merge first
	//Pushing and pushing state
	//Interact with doors

	private void Awake()
	{
		outline = GetComponent<Outline>();
		outline.enabled = false;
		gameObject.layer = 10;
	}
}
