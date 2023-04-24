using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
	[SerializeField] private Slider sensitivity;
	[SerializeField] private Slider masterAudio;
	[SerializeField] private Slider musicAudio;
	[SerializeField] private Slider sfxAudio;

	private void Awake()
	{
		if(!PlayerPrefs.HasKey("Sensitivity")) { PlayerPrefs.SetFloat("Sensitivity", 1.0f); }
		if(!PlayerPrefs.HasKey("MasterAudio")) { PlayerPrefs.SetFloat("MasterAudio", 1.0f); }
		if(!PlayerPrefs.HasKey("MusicAudio")) { PlayerPrefs.SetFloat("MusicAudio", 1.0f); }
		if(!PlayerPrefs.HasKey("SFXAudio")) { PlayerPrefs.SetFloat("SFXAudio", 1.0f); }

		PlayerPrefs.Save();

		sensitivity.value = PlayerPrefs.GetFloat("Sensitivity");
		masterAudio.value = PlayerPrefs.GetFloat("MasterAudio");
		musicAudio.value = PlayerPrefs.GetFloat("MusicAudio");
		sfxAudio.value = PlayerPrefs.GetFloat("SFXAudio");
	}

	public void ChangeSensitivity()
	{
		PlayerPrefs.SetFloat("Sensitivity", sensitivity.value);

		PlayerPrefs.Save();
	}

	public void ChangeMasterAudio()
	{
		PlayerPrefs.SetFloat("MasterAudio", masterAudio.value);

		PlayerPrefs.Save();
	}

	public void ChangeMusicAudio()
	{
		PlayerPrefs.SetFloat("MusicAudio", musicAudio.value);

		PlayerPrefs.Save();
	}

	public void ChangeSFXAudio()
	{
		PlayerPrefs.SetFloat("SFXAudio", sfxAudio.value);

		PlayerPrefs.Save();
	}
}
