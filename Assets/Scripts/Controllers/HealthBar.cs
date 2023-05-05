using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
	[SerializeField] private Sprite[] sprites;

	[SerializeField] private Image image;
	[SerializeField] private RectTransform backing;
	[SerializeField] private RectTransform tantrumFill;
	[SerializeField] private RectTransform temperFill;
	[SerializeField] private RectTransform downFill;

	private float length;

	private void Awake()
	{
		length = backing.sizeDelta.x;

		tantrumFill.sizeDelta = new Vector2(0.0f, backing.sizeDelta.y);
		temperFill.sizeDelta = new Vector2(0.0f, backing.sizeDelta.y);
		downFill.sizeDelta = new Vector2(0.0f, backing.sizeDelta.y);
	}

	public void SetImage(byte id)
	{
		image.sprite = sprites[id];
	}

	public void SetTantrumPercent(float percent)
	{
		tantrumFill.sizeDelta = new Vector2(length * percent, backing.sizeDelta.y);
	}

	public void SetTemperPercent(float percent)
	{
		temperFill.sizeDelta = new Vector2(length * percent, backing.sizeDelta.y);
	}

	public void SetDownPercent(float percent)
	{
		downFill.sizeDelta = new Vector2(length * percent, backing.sizeDelta.y);
	}
}
