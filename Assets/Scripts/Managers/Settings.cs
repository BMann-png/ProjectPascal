using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
	[SerializeField] private Slider sensitivity;

	private void Awake()
	{
		if(!PlayerPrefs.HasKey("Sensitivity")) { PlayerPrefs.SetFloat("Sensitivity", 1.0f); }

		PlayerPrefs.Save();

		sensitivity.value = PlayerPrefs.GetFloat("Sensitivity");
	}

	public void ChangeSensitivity()
	{
		PlayerPrefs.SetFloat("Sensitivity", sensitivity.value);

		PlayerPrefs.Save();
	}
}
