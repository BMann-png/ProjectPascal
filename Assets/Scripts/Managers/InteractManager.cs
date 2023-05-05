using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractManager : MonoBehaviour
{
	private LayerMask layerMask;

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);

		layerMask = LayerMask.GetMask("Interactable");
	}

	private void Update()
	{
		Transform trans = Camera.main.transform;

		if (Physics.Raycast(trans.position, trans.forward, out RaycastHit hit, 3.0f, layerMask.value) &&
			hit.transform.TryGetComponent(out Interactable i))
		{
			//TODO: Highlight

			if (Input.GetKeyDown(KeyCode.E))
			{
				i.onInteract.Invoke();
			}
		}
	}
}
