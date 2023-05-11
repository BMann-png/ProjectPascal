using UnityEngine;
using Steamworks.Data;
using TMPro;

public class LobbyEntry : MonoBehaviour
{
	[SerializeField] private TMP_Text lobbyName;
	[SerializeField] private TMP_Text playerCount;

	private Lobby lobby;

	public void CreateLobby(Lobby lobby)
	{
		this.lobby = lobby;

		lobbyName.text = lobby.GetData("Name");
		playerCount.text = $"{lobby.MemberCount}/{lobby.MaxMembers}";
	}

	public void JoinLobby()
	{
		GameManager.Instance.SceneLoader.LoadScene("Lobby");
		NetworkManager.Instance.JoinLobby(lobby);
	}
}
