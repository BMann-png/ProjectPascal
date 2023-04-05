using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MenuHandler : MonoBehaviour
{
	[SerializeField] private GameObject mainMenu;
	[SerializeField] private GameObject lobbyBrowser;
	[SerializeField] private GameObject settings;
	[SerializeField] private GameObject lobbyList;
	[SerializeField] private GameObject lobbyPrefab;

	public void Awake()
	{
		mainMenu.SetActive(true);
		lobbyBrowser.SetActive(false);
		settings.SetActive(false);
	}

	public void GoToMainMenu()
	{
		mainMenu.SetActive(true);
		lobbyBrowser.SetActive(false);
		settings.SetActive(false);
	}

	public void GoToLobbyBrowser()
	{
		FillLobbyList();

		mainMenu.SetActive(false);
		lobbyBrowser.SetActive(true);
		settings.SetActive(false);
	}

	public void GoToSettings()
	{
		mainMenu.SetActive(false);
		lobbyBrowser.SetActive(false);
		settings.SetActive(true);
	}

	public void ExitGame()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#endif
		Application.Quit();
	}

	public void FillLobbyList()
	{
		foreach (Transform child in lobbyList.transform)
		{
			Destroy(child.gameObject);
		}

		int lobbyCount = 20;

		float lobbyHeight = lobbyPrefab.GetComponent<RectTransform>().rect.height;
		float lobbyListHeight = 800;
		float size = Mathf.Max(lobbyHeight * lobbyCount, lobbyListHeight);
		lobbyList.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);

		for (int i = 0; i < lobbyCount; ++i)
		{
			Instantiate(lobbyPrefab, lobbyList.transform);
		}
	}
}
