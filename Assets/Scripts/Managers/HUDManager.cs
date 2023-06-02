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
        for (int i = 0; i < numOfTears; i++) 
		{ 
			int tear = Random.Range(0, tears.Length);

			Vector2 vec = Vector2.zero;
			vec.x = Random.Range(-1.0f, 1.0f);
			vec.y = Random.Range(-1.0f, 1.0f);

			vec.Normalize();

			Vector2 padding = vec;
			padding.x *= 0.375f * Screen.width;
			padding.y *= 0.375f * Screen.height;

			float prec = Random.Range(0, 0.125f);
			vec.x *= prec * Screen.width;
			vec.y *= prec * Screen.height;

			Vector2 spawn = vec + padding;

			//float x = Mathf.Cos(Mathf.Deg2Rad * Random.Range(0, 360)) * Random.Range(0.5f * Screen.width, 0.6f * Screen.width);
			//float y = Mathf.Sin(Mathf.Deg2Rad * Random.Range(0, 360)) * Random.Range(0.5f * Screen.height, 0.6f * Screen.height);

            var t = Instantiate(tears[tear], cryEffect.transform);
			t.transform.localPosition = spawn;//new Vector2(x, y);
		}
	}
}
