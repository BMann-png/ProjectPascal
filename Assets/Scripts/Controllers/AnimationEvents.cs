using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
	private Attack attack;

	private void Awake()
	{
		attack = GetComponentInParent<Attack>();
	}

	public void Attack()
	{
		attack.OnAttack();
	}

	public void OnStep()
    {
		GameManager.Instance.AudioManager.Source.PlayOneShot
			(GameManager.Instance.AudioManager.GetFootStep());
    }
}
