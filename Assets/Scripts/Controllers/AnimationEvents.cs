using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
	private Attack attack;

	private AudioSource crySource;

	private void Awake()
	{
		attack = GetComponentInParent<Attack>();
		crySource = GetComponent<AudioSource>();
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

	public void OnCry()
    {
		if (!crySource.isPlaying) crySource.Play();
    }

	public void StopCry()
    {
		crySource.Stop();
    }
}
