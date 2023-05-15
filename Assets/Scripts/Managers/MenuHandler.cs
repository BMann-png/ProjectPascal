using Steamworks.Data;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MenuHandler : MonoBehaviour
{
	[SerializeField] private GameObject mainMenu;
	[SerializeField] private GameObject lobbyBrowser;
	[SerializeField] private GameObject settings;
	[SerializeField] private GameObject createLobby;
	[SerializeField] private GameObject lobbyList;
	[SerializeField] private GameObject lobbyPrefab;
	[SerializeField] private TMP_InputField lobbyName;
	[SerializeField] private Scrollbar scrollbar;

	public void Awake()
	{
		mainMenu.SetActive(true);
		lobbyBrowser.SetActive(false);
		settings.SetActive(false);
		createLobby.SetActive(false);

		//This is to initialize the GameManager
		GameManager manager = GameManager.Instance;
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

		if (lobbies == null || lobbies.Count == 0)
		{
			scrollbar.size = 1.0f;
			scrollbar.value = 1.0f;
			return;
		}

		int count = 0;
		foreach (Lobby lobby in lobbies)
		{
			string value = lobby.GetData("DaycareDescent");
			if(value == null || value != "true") { continue; }

			GameObject go = Instantiate(lobbyPrefab, lobbyList.transform);
			go.GetComponent<LobbyEntry>().CreateLobby(lobby);

			++count;
		}

		float lobbyHeight = lobbyPrefab.GetComponent<RectTransform>().rect.height;
		float lobbyListHeight = 800;
		float size = Mathf.Max(lobbyHeight * count, lobbyListHeight);

		if(size == 0.0f) { scrollbar.size = 1.0f; }
		else
		{
			scrollbar.size = Mathf.Max(1.0f, lobbyListHeight / size);
		}

		scrollbar.value = 1.0f;

		lobbyList.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
	}
}
