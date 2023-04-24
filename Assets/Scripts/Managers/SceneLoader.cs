using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static SceneLoader;

public class SceneLoader : Singleton<SceneLoader>
{
	[SerializeField] private GameObject loadingScreen;

	public delegate void OnSceneLoad();

	OnSceneLoad onSceneLoad;

	protected override void Awake()
	{
		base.Awake();

		DontDestroyOnLoad(gameObject);
		DontDestroyOnLoad(loadingScreen);
		SceneManager.sceneLoaded += SceneLoaded;
	}

	//TODO: Wait for all players to load
	public void LoadScene(string sceneName)
	{
		loadingScreen.SetActive(true);
		SceneManager.LoadScene(sceneName);
	}

	public void SetOnLoad(OnSceneLoad onSceneLoad)
	{
		this.onSceneLoad = onSceneLoad;
	}

	public void SceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if(onSceneLoad != null) { onSceneLoad(); onSceneLoad = null; }
		loadingScreen.SetActive(false);
	}
}
