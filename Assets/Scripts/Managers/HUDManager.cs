using TMPro;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
	[SerializeField] private TMP_Text toolTip;
	[SerializeField] private GameObject pauseMenu;
	[SerializeField] private RectTransform progress;
	[SerializeField] private RectTransform fill;

	private float length;

	public bool Paused { get; private set; }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
		toolTip.gameObject.SetActive(false);
		pauseMenu.SetActive(false);

		length = progress.sizeDelta.x;
		fill.gameObject.SetActive(false);
		progress.gameObject.SetActive(false);
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

	public void SetProgress(float percent)
	{
		if(percent == 0)
		{
			fill.gameObject.SetActive(false);
			progress.gameObject.SetActive(false);
		}
		else
		{
			fill.gameObject.SetActive(true);
			progress.gameObject.SetActive(true);
			fill.sizeDelta = new Vector2(percent * length, fill.sizeDelta.y);
		}
	}
}
