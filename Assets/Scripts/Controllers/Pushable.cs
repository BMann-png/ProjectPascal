using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Interactable), typeof(Entity))]
public class Pushable : MonoBehaviour
{
	[SerializeField] private int requiredPlayers;
	[SerializeField] private float pushTime;
	[SerializeField] private Vector3 endPosition;
	[SerializeField] private UnityEvent onComplete;

	private InteractManager manager;
	private Interactable interactable;
	private Vector3 initialPosition;
	private int playerCount;
	private float pushTimer = 0.0f;
	private float timeInv;
	private bool complete = false;
	private bool pushing = false;

    void Start()
    {
		manager = FindFirstObjectByType<InteractManager>();
		interactable = GetComponent<Interactable>();

		interactable.onInteract.RemoveAllListeners();
		interactable.onInteract.AddListener(Push);
		interactable.onStopInteract.RemoveAllListeners();
		interactable.onStopInteract.AddListener(StopPush);
		interactable.canInteract = false;
		interactable.hold = true;
		gameObject.isStatic = false;

		if (pushTime <= 0.0f) { pushTime = 0.01f; }
		timeInv = 1.0f / pushTime;
		initialPosition = transform.position;
		requiredPlayers = Mathf.Min(GameManager.Instance.PlayerCount, requiredPlayers);
    }

	public void Push()
	{
		if(!pushing)
		{
			pushing = true;
			++playerCount;

			//TODO: Send pushing action message
			//TODO: Send pushing message
		}

		if (playerCount >= requiredPlayers && !complete)
		{
			pushTimer += Time.deltaTime * timeInv;

			Vector3 init = transform.position;
			transform.position = Vector3.Lerp(initialPosition, endPosition, pushTimer);
			GameManager.Instance.PushPlayer(transform.position - init);

			if (pushTimer >= 1.0f)
			{
				complete = true;
				interactable.canInteract = false;
				onComplete.Invoke();
			}
		}
	}

	public void StopPush()
	{
		pushing = false;
		--playerCount;

		//TODO: Send idle/walking action message
		//TODO: Send stop pushing message
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!complete) { interactable.canInteract = true; }
	}

	private void OnTriggerExit(Collider other)
	{
		interactable.canInteract = false;
	}
}
