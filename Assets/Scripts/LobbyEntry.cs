using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks.Data;
using TMPro;

public class LobbyEntry : MonoBehaviour
{
	[SerializeField] private TMP_Text lobbyName;
	[SerializeField] private TMP_Text playerCount;

	static SceneLoader sceneLoader;

	private Lobby lobby;

	private void Awake()
	{
		if(sceneLoader == null) { sceneLoader = FindFirstObjectByType<SceneLoader>(); }
	}

	public void CreateLobby(Lobby lobby)
	{
		this.lobby = lobby;

		lobbyName.text = lobby.GetData("Name");
		playerCount.text = $"{lobby.MemberCount}/{lobby.MaxMembers}";
	}

	public async void JoinLobby()
	{
		bool success = await NetworkManager.Instance.JoinLobby(lobby);
		if (success)
		{
			sceneLoader.LoadScene("Lobby");
		}
	}
}
