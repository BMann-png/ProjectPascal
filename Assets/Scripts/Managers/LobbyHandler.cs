using Steamworks;
using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyHandler : MonoBehaviour
{
	[SerializeField] private Transform[] spawnPoints;

	private void Awake()
	{
		GameManager.Instance.OnJoinLobby(spawnPoints);
	}

	public void LeaveLobby()
	{
		NetworkManager.Instance.LeaveLobby();
		SceneLoader.Instance.LoadScene("MainMenu");
	}

	public void StartGame()
	{
		GameManager.Instance.StartGame();
	}

	public void SelectLevel(TMP_Dropdown change)
	{
		GameManager.Instance.SelectLevel(change);
	}
}
