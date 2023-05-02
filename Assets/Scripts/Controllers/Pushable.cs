using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Pushable : MonoBehaviour
{
	[SerializeField] private int requiredPlayers;
	[SerializeField] private float pushTime;
	[SerializeField] private Vector3 endPosition;
	[SerializeField] private UnityEvent onComplete;

	private Vector3 initialPosition;
	private int playerCount;
	private float pushTimer = 0.0f;
	private float timeInv;
	private bool complete = false;

    void Start()
    {
		if(pushTime <= 0.0f) { pushTime = 0.01f; }
		timeInv = 1.0f / pushTime;
		initialPosition = transform.position;
		requiredPlayers = Mathf.Min(GameManager.Instance.PlayerCount, requiredPlayers);
    }

    void Update()
    {
        if(playerCount >= requiredPlayers && !complete)
		{
			pushTimer += Time.deltaTime * timeInv;

			transform.position = Vector3.Lerp(initialPosition, endPosition, pushTimer);

			if (pushTimer >= 1.0f)
			{
				complete = true;
				onComplete.Invoke();
			}
		}
    }

	private void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Player")
		{
			++playerCount;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if(other.tag == "Player")
		{
			--playerCount;
		}
	}
}
