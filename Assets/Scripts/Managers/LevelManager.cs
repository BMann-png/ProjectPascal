using UnityEngine;

public class LevelManager : MonoBehaviour
{
	private PlayerSpawner[] playerSpawners;
	private EnemySpawner[] enemySpawners;

	//TODO: Initial spawn and hidden spawn

	private void Awake()
	{
		playerSpawners = FindObjectsByType<PlayerSpawner>(FindObjectsSortMode.InstanceID);
		enemySpawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.InstanceID);

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

	public byte RandomEnemySpawn()
	{
		return (byte)Random.Range(0, enemySpawners.Length);
	}
}
