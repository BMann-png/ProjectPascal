using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
	public UnityEvent onInteract;

	//Reviving
	//Interact with doors
	//Pushing and pushing state

	private void Awake()
	{
		gameObject.layer = 10;
	}
}
