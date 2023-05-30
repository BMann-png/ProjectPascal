using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
	[SerializeField] private AudioMixer mixer;

	[SerializeField] private AudioClip[] feetNoises;
	[SerializeField] private AudioClip[] heelNoises;
	[SerializeField] private AudioClip[] handNoises;
	[SerializeField] private AudioClip[] faceNoises;
	[SerializeField] private AudioClip[] mouthNoises;

	[SerializeField] private AudioClip[] dartShots;
	[SerializeField] private AudioClip[] bubbleShots;
	[SerializeField] private AudioClip[] squirtShots;

	[SerializeField] private AudioSource sfx;
	[SerializeField] private AudioSource music;
	[SerializeField] private AudioSource crySource;

	public AudioSource Sfx { get { return sfx; } }
	public AudioSource Music { get { return music; } }

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}

	public void ChangeMasterVolume(float volume)
	{
		mixer.SetFloat("MasterVolume", volume * 80.0f - 80.0f);
	}

	public void ChangeMusicVolume(float volume)
	{
		mixer.SetFloat("MusicVolume", volume * 80.0f - 80.0f);
	}

	public void ChangeSFXVolume(float volume)
	{
		mixer.SetFloat("SFXVolume", volume * 80.0f - 80.0f);
	}

	public AudioClip GetFootStep()
    {
		return feetNoises[Random.Range(0, feetNoises.Length)];
    }

	public AudioClip GetSprintStep()
    {
		return heelNoises[Random.Range(0, heelNoises.Length)];
    }

	public AudioClip GetTrip()
    {
		return faceNoises[Random.Range(0, faceNoises.Length)];
    }

	public AudioClip GetPunch()
    {
		return handNoises[Random.Range(0, handNoises.Length)];
	}

	public AudioClip GetMouth()
    {
		return mouthNoises[Random.Range(0, mouthNoises.Length)];
	}

	public AudioClip GetShots(int type)
    {
        switch (type)
        {
			case 0:
				return dartShots[Random.Range(0, dartShots.Length)];
			case 1:
				return bubbleShots[Random.Range(0, bubbleShots.Length)];
			case 2:
				return squirtShots[Random.Range(0, squirtShots.Length)];
			default:
				return null;
        }		
	}

	public void StartCry()
	{
		if (!crySource.isPlaying) crySource.Play();
	}

	public void StopCry()
	{
		crySource.Stop();
	}
}
