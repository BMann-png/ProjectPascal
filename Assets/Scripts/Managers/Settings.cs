using System.Collections;
using System.Collections.Generic;
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
		sensitivityInput.text = PlayerPrefs.GetFloat("Sensitivity").ToString();
		masterAudioInput.text = PlayerPrefs.GetFloat("MasterAudio").ToString();
		musicAudioInput.text = PlayerPrefs.GetFloat("MusicAudio").ToString();
		sfxAudioInput.text = PlayerPrefs.GetFloat("SFXAudio").ToString();
	}

	public void ChangeSensitivity()
	{
		PlayerPrefs.SetFloat("Sensitivity", sensitivity.value);
		sensitivityInput.text = PlayerPrefs.GetFloat("Sensitivity").ToString();

		PlayerPrefs.Save();
	}

	public void ChangeMasterAudio()
	{
		PlayerPrefs.SetFloat("MasterAudio", masterAudio.value);
		masterAudioInput.text = PlayerPrefs.GetFloat("MasterAudio").ToString();

		PlayerPrefs.Save();
	}

	public void ChangeMusicAudio()
	{
		PlayerPrefs.SetFloat("MusicAudio", musicAudio.value);
		musicAudioInput.text = PlayerPrefs.GetFloat("MusicAudio").ToString();

		PlayerPrefs.Save();
	}

	public void ChangeSFXAudio()
	{
		PlayerPrefs.SetFloat("SFXAudio", sfxAudio.value);
		sfxAudioInput.text = PlayerPrefs.GetFloat("SFXAudio").ToString();

		PlayerPrefs.Save();
	}
}
