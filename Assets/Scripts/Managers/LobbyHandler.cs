using TMPro;
using UnityEngine;

public class LobbyHandler : MonoBehaviour
{
	[SerializeField] private Transform[] spawnPoints;
	[SerializeField] private GameObject startButton;
	[SerializeField] private GameObject levelSelect;

	[SerializeField] private GameObject[] levels;
	private int level;

	private void Awake()
	{
		level = 0;
		levels[0].SetActive(true);
		for (int i = 1; i < levels.Length; ++i) { levels[1].SetActive(false); }

		GameManager.Instance.SetupLobby(spawnPoints);

		if (GameManager.Instance.IsServer)
		{
			startButton.SetActive(true);
			levelSelect.SetActive(true);
		}
		else
		{
			startButton.SetActive(false);
			levelSelect.SetActive(false);
		}
	}

	public void LeaveLobby()
	{
		if (!GameManager.Instance.Fading)
		{
			GameManager.Instance.LeaveLobby();
			NetworkManager.Instance.LeaveLobby();
			GameManager.Instance.SceneLoader.LoadScene("MainMenu");
		}
	}

	public void StartGame()
	{
		GameManager.Instance.StartGame();
	}

	public void SelectLevel(TMP_Dropdown change)
	{
		levels[level].SetActive(false);
		level = change.value;
		levels[level].SetActive(true);

		GameManager.Instance.SelectLevel(change);
	}
}
