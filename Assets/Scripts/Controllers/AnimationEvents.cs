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
}
