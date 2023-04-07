using Steamworks;
using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

//TODO: Level select
//TODO: Load Level

public class LobbyHandler : MonoBehaviour
{
	[SerializeField] private GameObject player;
	[SerializeField] private Transform[] spawnPoints;

	List<GameObject> players = new List<GameObject>();

	private SceneLoader sceneLoader;

	private void Awake()
	{
		sceneLoader = FindFirstObjectByType<SceneLoader>();

		SetUpLobby();
	}

	public void SetUpLobby()
	{
		List<Friend> pls = NetworkManager.Instance.currentLobby.Members.ToList();

		int i = 1;
		foreach(Friend pl in pls)
		{
			if(pl.IsMe) { players.Add(Instantiate(player, spawnPoints[0].position, spawnPoints[0].rotation)); }
			else { players.Add(Instantiate(player, spawnPoints[i].position, spawnPoints[i++].rotation)); }
		}
	}

	public void PlayerJoined(Friend player)
	{
		SetUpLobby();
	}

	public void PlayerLeft(Friend player)
	{
		SetUpLobby();
	}

	public void LeaveLobby()
	{
		NetworkManager.Instance.LeaveLobby();
		sceneLoader.LoadScene("MainMenu");
	}
}
