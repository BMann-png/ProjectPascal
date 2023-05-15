using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
	[SerializeField] private Slider sensitivity;
	[SerializeField] private Slider masterAudio;
	[SerializeField] private Slider musicAudio;
	[SerializeField] private Slider sfxAudio;
	[SerializeField] private TMP_InputField sensitivityInput;
	[SerializeField] private TMP_InputField masterAudioInput;
	[SerializeField] private TMP_InputField musicAudioInput;
	[SerializeField] private TMP_InputField sfxAudioInput;

	private AudioManager audioManager;

	private void Awake()
	{
		audioManager = GetComponent<AudioManager>();

		if(!PlayerPrefs.HasKey("Sensitivity")) { PlayerPrefs.SetFloat("Sensitivity", 1.0f); }
		if(!PlayerPrefs.HasKey("MasterAudio")) { PlayerPrefs.SetFloat("MasterAudio", 1.0f); }
		if(!PlayerPrefs.HasKey("MusicAudio")) { PlayerPrefs.SetFloat("MusicAudio", 1.0f); }
		if(!PlayerPrefs.HasKey("SFXAudio")) { PlayerPrefs.SetFloat("SFXAudio", 1.0f); }

		PlayerPrefs.Save();

		sensitivity.value = PlayerPrefs.GetFloat("Sensitivity");
		masterAudio.value = PlayerPrefs.GetFloat("MasterAudio");
		musicAudio.value = PlayerPrefs.GetFloat("MusicAudio");
		sfxAudio.value = PlayerPrefs.GetFloat("SFXAudio");
		sensitivityInput.text = string.Format("{0:0.0}", PlayerPrefs.GetFloat("Sensitivity"));
		masterAudioInput.text = string.Format("{0:0.0}", PlayerPrefs.GetFloat("MasterAudio"));
		musicAudioInput.text = string.Format("{0:0.0}", PlayerPrefs.GetFloat("MusicAudio"));
		sfxAudioInput.text = string.Format("{0:0.0}", PlayerPrefs.GetFloat("SFXAudio"));
	}

	public void ChangeSensitivity()
	{
		PlayerPrefs.SetFloat("Sensitivity", sensitivity.value);
		sensitivityInput.text = string.Format("{0:0.0}", PlayerPrefs.GetFloat("Sensitivity"));

		PlayerPrefs.Save();
	}

	public void ChangeSensitivityInput()
	{
		if (sensitivityInput.text != null)
		{
			float value = Mathf.Clamp(float.Parse(sensitivityInput.text), 0.5f, 2.0f);
			sensitivityInput.text = string.Format("{0:0.0}", value);
			PlayerPrefs.SetFloat("Sensitivity", value);
			sensitivity.value = PlayerPrefs.GetFloat("Sensitivity");

			PlayerPrefs.Save();
		}
		else
		{
			sensitivityInput.text = string.Format("{0:0.0}", PlayerPrefs.GetFloat("Sensitivity"));
		}
	}

	public void ChangeMasterAudio()
	{
		PlayerPrefs.SetFloat("MasterAudio", masterAudio.value);
		masterAudioInput.text = string.Format("{0:0.0}", PlayerPrefs.GetFloat("MasterAudio"));

		audioManager.ChangeMasterVolume(masterAudio.value);

		PlayerPrefs.Save();
	}

	public void ChangeMasterAudioInput()
	{
		if (masterAudioInput.text != null)
		{
			float value = Mathf.Clamp(float.Parse(masterAudioInput.text), 0.0f, 1.0f);
			masterAudioInput.text = string.Format("{0:0.0}", value);
			PlayerPrefs.SetFloat("MasterAudio", value);
			masterAudio.value = PlayerPrefs.GetFloat("MasterAudio");

			audioManager.ChangeMasterVolume(value);

			PlayerPrefs.Save();
		}
		else
		{
			masterAudioInput.text = string.Format("{0:0.0}", PlayerPrefs.GetFloat("MasterAudio"));
		}
	}

	public void ChangeMusicAudio()
	{
		PlayerPrefs.SetFloat("MusicAudio", musicAudio.value);
		musicAudioInput.text = string.Format("{0:0.0}", PlayerPrefs.GetFloat("MusicAudio"));

		audioManager.ChangeMasterVolume(musicAudio.value);

		PlayerPrefs.Save();
	}

	public void ChangeMusicAudioInput()
	{
		if (musicAudioInput.text != null)
		{
			float value = Mathf.Clamp(float.Parse(musicAudioInput.text), 0.0f, 1.0f);
			musicAudioInput.text = string.Format("{0:0.0}", value);
			PlayerPrefs.SetFloat("MusicAudio", value);
			musicAudio.value = PlayerPrefs.GetFloat("MusicAudio");

			audioManager.ChangeMasterVolume(value);

			PlayerPrefs.Save();
		}
		else
		{
			musicAudioInput.text = string.Format("{0:0.0}", PlayerPrefs.GetFloat("MusicAudio"));
		}
	}

	public void ChangeSFXAudio()
	{
		PlayerPrefs.SetFloat("SFXAudio", sfxAudio.value);
		sfxAudioInput.text = string.Format("{0:0.0}", PlayerPrefs.GetFloat("SFXAudio"));

		audioManager.ChangeMasterVolume(sfxAudio.value);

		PlayerPrefs.Save();
	}

	public void ChangeSFXAudioInput()
	{
		if (sfxAudioInput.text != null)
		{
			float value = Mathf.Clamp(float.Parse(sfxAudioInput.text), 0.0f, 1.0f);
			sfxAudioInput.text = string.Format("{0:0.0}", value);
			PlayerPrefs.SetFloat("SFXAudio", value);
			sfxAudio.value = PlayerPrefs.GetFloat("SFXAudio");

			audioManager.ChangeMasterVolume(value);

			PlayerPrefs.Save();
		}
		else
		{
			sfxAudioInput.text = string.Format("{0:0.0}", PlayerPrefs.GetFloat("SFXAudio"));
		}
	}

	public void ExitGame()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#endif
		Application.Quit();
	}
}
