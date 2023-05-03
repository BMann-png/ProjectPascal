using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
	[SerializeField] private TMP_Text textName;
	[SerializeField] private RectTransform backing;
	[SerializeField] private RectTransform tantrumFill;
	[SerializeField] private RectTransform temperFill;

	private float length;

	private void Awake()
	{
		length = backing.sizeDelta.x;

		tantrumFill.sizeDelta = new Vector2(0.0f, backing.sizeDelta.y);
		temperFill.sizeDelta = new Vector2(0.0f, backing.sizeDelta.y);
	}

	public void SetName(string name)
	{
		textName.text = name;
	}

	public void SetTantrumPercent(float percent)
	{
		tantrumFill.sizeDelta = new Vector2(length * percent, backing.sizeDelta.y);
	}

	public void SetTemperPercent(float percent)
	{
		temperFill.sizeDelta = new Vector2(length * percent, backing.sizeDelta.y);
	}
}
