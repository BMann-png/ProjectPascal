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

	public void GotoCreateLobby()
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

	async public void CreateLobby()
	{
		string name = lobbyName.text;

		if(name.Length == 0) { return; }

		Dictionary<string, string> lobbyData = new Dictionary<string, string>
			{
				{ "Name", name },
				{ "Count", "1" }
			};

		await NetworkManager.Instance.CreateLobby(lobbyData);
	}

	public async void FillLobbyList()
	{
		foreach (Transform child in lobbyList.transform)
		{
			Destroy(child.gameObject);
		}

		List<Lobby> lobbies = await NetworkManager.Instance.GetLobbies();

		if(lobbies == null) { return; }

		float lobbyHeight = lobbyPrefab.GetComponent<RectTransform>().rect.height;
		float lobbyListHeight = 800;
		float size = Mathf.Max(lobbyHeight * lobbies.Count, lobbyListHeight);
		lobbyList.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);

		foreach(Lobby lobby in lobbies)
		{
			//TODO: get meta data
			//lobby.GetData();

			Instantiate(lobbyPrefab, lobbyList.transform);
		}
	}
}
