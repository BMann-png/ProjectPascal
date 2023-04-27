using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

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
		if (!GameManager.Instance.Fading)
		{
			GameManager.Instance.LeaveLobby();
			NetworkManager.Instance.LeaveLobby();
			GameManager.Instance.SceneLoader.LoadScene("MainMenu");
		}
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
