using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractManager : MonoBehaviour
{
	private LayerMask layerMask;
	private Interactable lastHover;
	private HUDManager hudManager;

	private bool interacting = false;

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);

		hudManager = FindFirstObjectByType<HUDManager>();
		layerMask = LayerMask.GetMask("Interactable");
	}

	private void Update()
	{
		if (Camera.main != null)
		{
			Transform trans = Camera.main.transform;

			if (Physics.Raycast(trans.position, trans.forward, out RaycastHit hit, 3.0f, layerMask.value) &&
				hit.collider.isTrigger == false && //May not work
				hit.transform.TryGetComponent(out Interactable i) && 
				i.canInteract)
			{
				if (i != lastHover)
				{
					hudManager.SetTooltip(i.hold ? "Press and Hold E" : "Press E");

					if (lastHover != null)
					{
						lastHover.outline.enabled = false;
						if (lastHover.hold && interacting) { lastHover.onStopInteract.Invoke(); interacting = false; }
					}

					lastHover = i;
					i.outline.enabled = true;
				}

				if (i.hold)
				{
					if(Input.GetKey(KeyCode.E)) { i.onInteract.Invoke(); interacting = true; }
					else if(Input.GetKeyUp(KeyCode.E)) { lastHover.onStopInteract.Invoke(); interacting = false; }
				}
				else if (Input.GetKeyDown(KeyCode.E)) { i.onInteract.Invoke(); }
			}
			else if (lastHover)
			{
				hudManager.HideToolTip();
				lastHover.outline.enabled = false;
				if (lastHover.hold && interacting) { lastHover.onStopInteract.Invoke(); interacting = false; }
				lastHover = null;
			}
			else { hudManager.HideToolTip(); }
		}
	}
}
