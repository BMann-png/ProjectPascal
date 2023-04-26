using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Saferoom : MonoBehaviour
{
	[SerializeField] byte nextLevel;

	private int playerCount = 0;
	private int enemyCount = 0;
	private float timer = 3.0f;
	private bool ready = false;

	private void Update()
	{
		if(ready)
		{
			//TODO: Fade
			timer -= Time.deltaTime;

			if(timer <= 0 )
			{
				GameManager.Instance.ChangeLevel(nextLevel);
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Player") { ++playerCount; }
		else if(other.tag == "Enemy") { ++enemyCount; }

		if(enemyCount == 0 && playerCount == GameManager.Instance.PlayerCount)
		{
			ready = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.tag == "Player") { --playerCount; }
		else if (other.tag == "Enemy") { --enemyCount; }

		if (enemyCount == 0 && playerCount == GameManager.Instance.PlayerCount)
		{
			ready = true;
		}
	}
}
