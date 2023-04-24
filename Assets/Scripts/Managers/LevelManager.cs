using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
	[SerializeField] private Transform[] playerSpawnPoints;
	[SerializeField] private Transform[] enemySpawnPoints;

	private void Awake()
	{
		GameManager.Instance.OnLevelLoad(playerSpawnPoints, enemySpawnPoints);
	}
}
