using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
	[SerializeField] private GameObject[] playerModels;
	public GameObject[] PlayerModels { get => playerModels; }

	[SerializeField] private GameObject[] enemyModels;
	public GameObject[] EnemyModels { get => enemyModels; }

	[SerializeField] private GameObject player;
	public GameObject Player { get => player; }

	[SerializeField] private GameObject networkPlayer;
	public GameObject NetworkPlayer { get => networkPlayer; }

	[SerializeField] private GameObject lobbyPlayer;
	public GameObject LobbyPlayer { get => lobbyPlayer; }
  
	[SerializeField] private GameObject enemy;
	public GameObject Enemy { get => enemy; }

	[SerializeField] private GameObject[] projectiles;
	public GameObject[] Projectiles { get => projectiles; }

	[SerializeField] private GameObject[] networkProjectiles;
	public GameObject[] NetworkProjectiles { get => networkProjectiles; }

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}
}
