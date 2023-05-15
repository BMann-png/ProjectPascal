using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
	[SerializeField] private GameObject loadingScreen;
	[SerializeField] private Image fade;

	public delegate void OnSceneLoad();

	OnSceneLoad onSceneLoad;

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
		SceneManager.sceneLoaded += SceneLoaded;
		loadingScreen.SetActive(false);
		fade.color = Color.clear;
	}

	public void SetLoadingScreen(bool b)
	{
		loadingScreen.SetActive(b);
	}

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

	public delegate void OnFinishFade(int scene);

	public IEnumerator FadeToLoad(float time, int scene, OnFinishFade finish)
	{
		float fadeAmount = 1.0f / time;

		while(fade.color.a < 1.0f)
		{
			fade.color = new Color(0.0f, 0.0f, 0.0f, fade.color.a + fadeAmount * Time.deltaTime);
			yield return null;
		}

		finish(scene);
	}

	public void ResetScreen()
	{
		fade.color = Color.clear;
	}
}
