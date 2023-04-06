using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyHandler : MonoBehaviour
{
	private Lobby lobby;

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}

	public void SetLobby(Lobby lobby)
	{
		this.lobby = lobby;
	}

	public void LeaveLobby()
	{
		lobby.Leave();
	}
}
