using TMPro;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
	[SerializeField] private TMP_Text toolTip;
	[SerializeField] private GameObject pauseMenu;

	public bool Paused { get; private set; }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
		toolTip.gameObject.SetActive(false);
		pauseMenu.SetActive(false);
	}

	private void Update()
	{
		if (GameManager.Instance.InGame && Input.GetKeyDown(KeyCode.Escape))
		{
			pauseMenu.SetActive(!pauseMenu.activeSelf);
			Paused = !Paused;

			if(Paused)
			{
				Cursor.lockState = CursorLockMode.Confined;
				Cursor.visible = true;
			}
			else
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
		}
	}

	public void SetTooltip(string text)
	{
		toolTip.gameObject.SetActive(true);
		toolTip.text = text;
	}

    public void HideToolTip()
    {
        toolTip.gameObject.SetActive(false);
	}
}
