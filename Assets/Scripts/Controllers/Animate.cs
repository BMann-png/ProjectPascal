using UnityEngine;

[RequireComponent(typeof(Interactable), typeof(Entity))]
public class Animate : MonoBehaviour
{
	[SerializeField] bool canInteract = true;
	[SerializeField] private new Animation animation;

	private Interactable interactable;
	private Entity entity;

	private void Awake()
	{
		interactable = GetComponent<Interactable>();
		entity = GetComponent<Entity>();

		interactable.canInteract = canInteract;
		interactable.onInteract.RemoveAllListeners();
		interactable.onInteract.AddListener(StartAnimation);
	}

	public void StartAnimation()
	{
		animation.Play();
		interactable.canInteract = false;

		Packet packet = new Packet();
		packet.type = 1;
		packet.id = entity.id;
		packet.action = new ActionPacket(255);
	}

	public void OtherPlay()
	{
		animation.Play();
		interactable.canInteract = false;
	}
}
