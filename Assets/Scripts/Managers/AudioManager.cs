using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
	[SerializeField] private AudioMixer mixer;

	public void ChangeMasterVolume(float volume)
	{
		mixer.SetFloat("MasterVolume", volume);
	}

	public void ChangeMusicVolume(float volume)
	{
		mixer.SetFloat("MusicVolume", volume);
	}

	public void ChangeSFXVolume(float volume)
	{
		mixer.SetFloat("SFXVolume", volume);
	}
}
