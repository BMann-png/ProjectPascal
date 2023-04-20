using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static SceneLoader;

public class SceneLoader : Singleton<SceneLoader>
{
	public delegate void OnSceneLoad();

	OnSceneLoad onSceneLoad;

	protected override void Awake()
	{
		base.Awake();

		DontDestroyOnLoad(gameObject);
		SceneManager.sceneLoaded += SceneLoaded;
	}

	//TODO: Loading screen
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
