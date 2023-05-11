using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
	[SerializeField] private AudioMixer mixer;

	[SerializeField] private AudioClip[] feetNoises;

	[SerializeField] private AudioSource source;

	public AudioSource Source { get { return source; } }

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

	public AudioClip GetFootStep()
    {
		return feetNoises[Random.Range(0, feetNoises.Length)];
    }
}
