using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//TODO: Get notified when a player joins
//TODO: Level select
//TODO: Load Level

public class LobbyHandler : MonoBehaviour
{
	[SerializeField] private GameObject player;
	[SerializeField] private Transform[] spawnPoints;

	private SceneLoader sceneLoader;

	private void Awake()
	{
		sceneLoader = FindFirstObjectByType<SceneLoader>();
		SetUpLobby();
	}

	public void SetUpLobby()
	{
		int count = NetworkManager.Instance.currentLobby.MemberCount;

		for (int i = 0; i < count; ++i)
		{
			Instantiate(player, spawnPoints[i].position, spawnPoints[i].rotation);
		}
	}

	public void LeaveLobby()
	{
		NetworkManager.Instance.LeaveLobby();
		sceneLoader.LoadScene("MainMenu");
	}
}
