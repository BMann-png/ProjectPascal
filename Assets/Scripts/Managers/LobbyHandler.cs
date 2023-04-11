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

		if(!GameManager.Instance.IsServer)
		{
			startButton.SetActive(false);
			levelSelect.SetActive(false);
		}
	}

	public void LeaveLobby()
	{
		GameManager.Instance.LeaveLobby();
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

	public void CheckOwner()
	{
		if (!GameManager.Instance.IsServer)
		{
			startButton.SetActive(false);
			levelSelect.SetActive(false);
		}
		else
		{
			startButton.SetActive(true);
			levelSelect.SetActive(true);
		}
	}
}
