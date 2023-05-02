using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animation))]
public class Animate : MonoBehaviour
{
	private new Animation animation;

	private void Awake()
	{
		animation = GetComponent<Animation>();
	}

	public void StartAnimation()
	{
		animation.Play();
	}
}
