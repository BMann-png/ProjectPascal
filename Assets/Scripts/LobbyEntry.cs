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
	static LobbyHandler lobbyHandler;

	private Lobby lobby;

	private void Awake()
	{
		if(sceneLoader == null) { sceneLoader = FindFirstObjectByType<SceneLoader>(); }
		if(lobbyHandler == null) { lobbyHandler = FindFirstObjectByType<LobbyHandler>(); }
	}

	public void CreateLobby(Lobby lobby)
	{
		this.lobby = lobby;

		lobbyName.text = lobby.GetData("Name");
		playerCount.text = $"{lobby.MemberCount}/{lobby.MaxMembers}";
	}

	public void JoinLobby()
	{
		NetworkManager.Instance.JoinLobby(lobby);
		sceneLoader.SetOnLoad(lobbyHandler.SetUpLobby);
		sceneLoader.LoadScene("Lobby");
	}
}
