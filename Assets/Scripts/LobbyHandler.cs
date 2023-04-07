using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//TODO: spawn player models based on player count

public class LobbyHandler : MonoBehaviour
{
	private SceneLoader sceneLoader;

	private void Awake()
	{
		sceneLoader = FindFirstObjectByType<SceneLoader>();
		DontDestroyOnLoad(gameObject);
	}

	public void SetUpLobby()
	{
		GameObject.Find("Leave").GetComponent<Button>().onClick.AddListener(LeaveLobby);
	}

	public void LeaveLobby()
	{
		NetworkManager.Instance.LeaveLobby();
		sceneLoader.LoadScene("MainMenu");
	}

	public void SpawnPlayer()
	{

	}
}
