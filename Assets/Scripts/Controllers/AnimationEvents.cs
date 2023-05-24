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
		GameManager.Instance.AudioManager.Sfx.PlayOneShot(GameManager.Instance.AudioManager.GetFootStep());
    }

	public void OnTrip()
    {
		GameManager.Instance.AudioManager.Sfx.PlayOneShot(GameManager.Instance.AudioManager.GetTrip());
    }
}
