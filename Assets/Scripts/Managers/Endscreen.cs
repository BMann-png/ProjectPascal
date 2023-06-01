using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Endscreen : MonoBehaviour
{
	[SerializeField] private GameObject win;
	[SerializeField] private GameObject lose;

	void Start()
	{
		if (GameManager.Instance.Lose) { lose.SetActive(true); win.SetActive(false); }
		else { lose.SetActive(false); win.SetActive(true); }

		Cursor.lockState = CursorLockMode.Confined;
		Cursor.visible = true;

		GameManager.Instance.FinishLoading();
	}

	public void Quit()
	{
		GameManager.Instance.LeaveLobby();
		NetworkManager.Instance.LeaveLobby();
		GameManager.Instance.SceneLoader.LoadScene("MainMenu");
	}
}
