using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}

	public void LoadScene(String sceneName)
	{
		SceneManager.LoadScene(sceneName);
	}
}
