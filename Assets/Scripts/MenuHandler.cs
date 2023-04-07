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

	private SceneLoader sceneLoader;
	private LobbyHandler lobbyHandler;

	public void Awake()
	{
		mainMenu.SetActive(true);
		lobbyBrowser.SetActive(false);
		settings.SetActive(false);
		createLobby.SetActive(false);

		sceneLoader = FindFirstObjectByType<SceneLoader>();
		lobbyHandler = FindFirstObjectByType<LobbyHandler>();
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

	async public void CreateLobby()
	{
		string name = lobbyName.text;

		if (name.Length == 0) { return; }

		Dictionary<string, string> lobbyData = new Dictionary<string, string> {
			{ "Name", name },
		};

		bool result = await NetworkManager.Instance.CreateLobby(lobbyData, 4);

		if(result)
		{
			sceneLoader.LoadScene("Lobby");
		}
		else
		{
			//TODO: Failure to join message
		}
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
