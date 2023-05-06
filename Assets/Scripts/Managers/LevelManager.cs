using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
	private PlayerSpawner[] playerSpawners;
	private EnemySpawner[] enemySpawners;
	private InteractableSpawner[] interactableSpawners;

	//TODO: Initial spawn and hidden spawn

	private void Awake()
	{
		playerSpawners = FindObjectsByType<PlayerSpawner>(FindObjectsSortMode.InstanceID);
		enemySpawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.InstanceID);
		interactableSpawners = FindObjectsByType<InteractableSpawner>(FindObjectsSortMode.InstanceID);

		GameManager.Instance.OnLevelLoad(this);
	}

	public Transform GetPlayerSpawn(int index)
	{
		return playerSpawners[index].transform;
	}

	public Transform GetEnemySpawn(int index)
	{
		return enemySpawners[index].transform;
	}

	public ushort InteractableSpawnCount()
	{
		return (ushort)interactableSpawners.Length;
	}

	public InteractableSpawner GetInteractableSpawn(int index)
	{
		return interactableSpawners[index];
	}

	public byte RandomEnemySpawn()
	{
		return (byte)Random.Range(0, enemySpawners.Length);
	}
}
