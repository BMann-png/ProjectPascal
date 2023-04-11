using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;

public class MenuHandler : MonoBehaviour
{
	[SerializeField] private GameObject mainMenu;
	[SerializeField] private GameObject lobbyBrowser;
	[SerializeField] private GameObject settings;
	[SerializeField] private GameObject createLobby;
	[SerializeField] private GameObject lobbyList;
	[SerializeField] private GameObject lobbyPrefab;
	[SerializeField] private TMP_InputField lobbyName;

	public void Awake()
	{
		mainMenu.SetActive(true);
		lobbyBrowser.SetActive(false);
		settings.SetActive(false);
		createLobby.SetActive(false);
	}

	public void GoToMainMenu()
	{
		mainMenu.SetActive(true);
		lobbyBrowser.SetActive(false);
		settings.SetActive(false);
		createLobby.SetActive(false);
	}

	public void GoToLobbyBrowser()
	{
		FillLobbyList();

		mainMenu.SetActive(false);
		lobbyBrowser.SetActive(true);
		settings.SetActive(false);
		createLobby.SetActive(false);
	}

	public void GoToSettings()
	{
		mainMenu.SetActive(false);
		lobbyBrowser.SetActive(false);
		settings.SetActive(true);
		createLobby.SetActive(false);
	}

	public void GoToCreateLobby()
	{
		mainMenu.SetActive(false);
		lobbyBrowser.SetActive(false);
		settings.SetActive(false);
		createLobby.SetActive(true);
	}

	public void ExitGame()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#endif
		Application.Quit();
	}

	public void CreateLobby()
	{
		GameManager.Instance.CreateLobby(lobbyName.text);
	}

	public async void FillLobbyList()
	{
		foreach (Transform child in lobbyList.transform)
		{
			Destroy(child.gameObject);
		}

		List<Lobby> lobbies = await NetworkManager.Instance.GetLobbies();

		if (lobbies == null || lobbies.Count == 0) { return; }

		int count = 0;
		foreach (Lobby lobby in lobbies)
		{
			string name = lobby.GetData("Name");
			if(name == null || name.Length == 0) { continue; }

			GameObject go = Instantiate(lobbyPrefab, lobbyList.transform);
			go.GetComponent<LobbyEntry>().CreateLobby(lobby);

			++count;
		}

		float lobbyHeight = lobbyPrefab.GetComponent<RectTransform>().rect.height;
		float lobbyListHeight = 800;
		float size = Mathf.Max(lobbyHeight * count, lobbyListHeight);
		lobbyList.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
	}
}
