using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
	public Transform shoot;
	[SerializeField] private byte type = 0;
	[SerializeField] private float delay = 0.1f;
	[SerializeField] private int numsOfShots = 1;

	public bool IsFiring { get; set; } = false;

	private float timer = 0f;
	private readonly float duration = 0.25f;
	private void Update()
	{
		timer -= Time.deltaTime;

		if (timer < 0) { IsFiring = false; }
	}

    public void Shoot() 
    {
        timer = duration;
        StartCoroutine(Fire());
        GameManager.Instance.AudioManager.Sfx.PlayOneShot(GameManager.Instance.AudioManager.GetShots(type));

        Packet packet = new Packet();
        packet.type = 1;
        packet.id = GameManager.Instance.ThisPlayer;
        packet.action = new ActionPacket();
        //Adding the type to 23 changes the action packet in the entity to play different sounds
        packet.action.data = (byte)(23 + type);

        NetworkManager.Instance.SendMessage(packet);


    }

	private IEnumerator Fire()
	{
		for (int i = 0; i < numsOfShots; i++)
		{
			GameManager.Instance.Shoot(shoot, type, GameManager.Instance.Entities[GameManager.Instance.ThisPlayer]);
			yield return new WaitForSeconds(delay);
		}
	}
}