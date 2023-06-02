using TMPro;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
	[SerializeField] private TMP_Text toolTip;
	[SerializeField] private GameObject pauseMenu;
	[SerializeField] private GameObject cryEffect;
	[SerializeField] private GameObject[] tears;
	[SerializeField] private RectTransform progress;
	[SerializeField] private RectTransform fill;

	private float length;

	public bool Paused { get; private set; }

    private void Awake()
    {
		if(!GameManager.Instance.FirstMenu)
		{
			Destroy(gameObject);
			return;
		}

		DontDestroyOnLoad(gameObject);
		toolTip.gameObject.SetActive(false);
		pauseMenu.SetActive(false);
		cryEffect.SetActive(false);

		length = progress.sizeDelta.x;
		fill.gameObject.SetActive(false);
		progress.gameObject.SetActive(false);
	}

	private void Update()
	{
		if (!GameManager.Instance.GameOver && GameManager.Instance.InGame && Input.GetKeyDown(KeyCode.Escape))
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

		if((GameManager.Instance.GameOver || !GameManager.Instance.InGame) && pauseMenu.activeSelf)
		{
			pauseMenu.SetActive(false);
			Paused = false;
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

	public void HidePauseMenu()
	{
		pauseMenu.SetActive(false);
		Paused = false;
	}

	public void SetCryEffect(bool isActive)
	{
		cryEffect.SetActive(isActive);
	}

	public void SpawnTear(int numOfTears = 0)
	{
        int screenHalfWidth = Screen.width / 2;
        int screenHalfHeight = Screen.height / 2;

        for (int i = 0; i < numOfTears; i++) 
		{ 
			int tear = Random.Range(0, tears.Length);
			Instantiate(tears[tear], new Vector2(Random.Range(screenHalfWidth - 300, screenHalfWidth + 301), Random.Range(screenHalfHeight - 400, screenHalfHeight + 401)), cryEffect.transform.rotation, cryEffect.transform);
		}
	}
}
