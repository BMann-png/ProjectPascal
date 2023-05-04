using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
	private BaseAI ai;

	private void Awake()
	{
		ai = GetComponentInParent<BaseAI>();
	}

	public void Attack()
	{
		ai.Attack();
	}
}
