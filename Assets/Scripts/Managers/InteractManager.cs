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

			RaycastHit[] hits = Physics.RaycastAll(trans.position, trans.forward, 3.0f, layerMask.value);

			bool found = false;

			foreach(RaycastHit hit in hits)
			{
				if(hit.collider.isTrigger == true) { continue; }

				found = true;

				if (hit.transform.TryGetComponent(out Interactable i) && i.canInteract)
				{
					if (i != lastHover)
					{
						hudManager.SetTooltip(i.hold ? "Press and Hold E" : "Press E");

						if (lastHover != null)
						{
							lastHover.outline.enabled = false;
							if (lastHover.hold && interacting)
							{
								lastHover.onStopInteract.Invoke();
								lastHover.onStopInteractOther.Invoke();
								interacting = false;
							}
						}

						lastHover = i;
						i.outline.enabled = true;
					}

					if (i.hold)
					{
						if (Input.GetKey(KeyCode.E))
						{
							i.onInteract.Invoke();
							i.onInteractOther.Invoke();
							interacting = true;
						}
						else if (Input.GetKeyUp(KeyCode.E))
						{
							lastHover.onStopInteract.Invoke();
							lastHover.onStopInteractOther.Invoke();
							interacting = false;
						}
					}
					else if (Input.GetKeyDown(KeyCode.E))
					{
						i.onInteract.Invoke();
						i.onInteractOther.Invoke();
					}
				}
				else if(lastHover)
				{
					hudManager.HideToolTip();
					lastHover.outline.enabled = false;
					if (lastHover.hold && interacting)
					{
						lastHover.onStopInteract.Invoke();
						lastHover.onStopInteractOther.Invoke();
						interacting = false;
					}
					lastHover = null;
				}

				break;
			}

			if(!found)
			{
				if (lastHover)
				{
					hudManager.HideToolTip();
					lastHover.outline.enabled = false;
					if (lastHover.hold && interacting)
					{
						lastHover.onStopInteract.Invoke();
						lastHover.onStopInteractOther.Invoke();
						interacting = false;
					}
					lastHover = null;
				}
				else { hudManager.HideToolTip(); }
			}
		}
	}
}
