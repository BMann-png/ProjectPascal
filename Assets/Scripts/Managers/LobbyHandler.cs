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
	[SerializeField] private GameObject startButton;
	[SerializeField] private GameObject levelSelect;

	private void Awake()
	{
		GameManager.Instance.OnJoinLobby(spawnPoints);
	}

	private void Update()
	{
		if (GameManager.Instance.IsServer)
		{
			startButton.SetActive(true);
			levelSelect.SetActive(true);
		}
		else
		{
			startButton.SetActive(false);
			levelSelect.SetActive(false);
		}
	}

	public void LeaveLobby()
	{
		GameManager.Instance.LeaveLobby();
		NetworkManager.Instance.LeaveLobby();
		GameManager.Instance.SceneLoader.LoadScene("MainMenu");
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
