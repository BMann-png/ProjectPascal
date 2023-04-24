using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static SceneLoader;

public class SceneLoader : MonoBehaviour
{
	[SerializeField] private GameObject loadingScreen;

	public delegate void OnSceneLoad();

	OnSceneLoad onSceneLoad;

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
		SceneManager.sceneLoaded += SceneLoaded;
		loadingScreen.SetActive(false);
	}

	public void SetLoadingScreen(bool b)
	{
		loadingScreen.SetActive(b);
	}

	//TODO: Wait for all players to load
	public void LoadScene(string sceneName)
	{
		SceneManager.LoadScene(sceneName);
	}

	public void SetOnLoad(OnSceneLoad onSceneLoad)
	{
		this.onSceneLoad = onSceneLoad;
	}

	public void SceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if(onSceneLoad != null) { onSceneLoad(); onSceneLoad = null; }
	}
}
