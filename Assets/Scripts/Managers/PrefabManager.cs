using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
	[SerializeField] private GameObject player;
	public GameObject Player { get => player; }

	[SerializeField] private GameObject networkPlayer;
	public GameObject NetworkPlayer { get => networkPlayer; }

	[SerializeField] private GameObject lobbyPlayer;
	public GameObject LobbyPlayer { get => lobbyPlayer; }
  
	[SerializeField] private GameObject enemy;
	public GameObject Enemy { get => enemy; }

	[SerializeField] private GameObject projectile;
	public GameObject Projectile { get => projectile; }

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}
}
