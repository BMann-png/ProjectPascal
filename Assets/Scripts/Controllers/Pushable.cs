using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Interactable), typeof(Entity))]
public class Pushable : MonoBehaviour
{
	[SerializeField] private int requiredPlayers;
	[SerializeField] private float pushTime;
	[SerializeField] private Vector3 endPosition;
	public UnityEvent onComplete;
	public UnityEvent onCompleteOther;

	private bool[] othersPushing = new bool[4];
	private Entity entity;
	private Interactable interactable;
	private Vector3 initialPosition;
	private int playerCount;
	private float pushTimer = 0.0f;
	private float timeInv;
	private bool complete = false;
	private bool pushing = false;

	private void Awake()
	{
		interactable = GetComponent<Interactable>();
		entity = GetComponent<Entity>();

		interactable.onInteract.RemoveAllListeners();
		interactable.onInteract.AddListener(Push);
		interactable.onStopInteract.RemoveAllListeners();
		interactable.onStopInteract.AddListener(StopPush);
		interactable.canInteract = false;
		interactable.hold = true;
		gameObject.isStatic = false;

		if (pushTime <= 0.0f) { pushTime = 0.01f; }
		timeInv = 1.0f / pushTime;
		initialPosition = transform.position;
	}

	//TODO: Push faster if extra players are pushing
	//TODO: Can't shoot or move? while pushing
	private void Update()
	{
		if (playerCount >= requiredPlayers || playerCount >= GameManager.Instance.AlivePlayers && !complete)
		{
			pushTimer += Time.deltaTime * timeInv;

			Vector3 init = transform.position;
			transform.position = Vector3.Lerp(initialPosition, endPosition, pushTimer);
			if (pushing) { GameManager.Instance.PushPlayer(transform.position - init); }

			if (pushTimer >= 1.0f)
			{
				complete = true;
				interactable.canInteract = false;
				onComplete.Invoke();
				onCompleteOther.Invoke();

				Packet packet = new Packet();
				packet.type = 1;
				packet.id = entity.id;
				packet.action = new ActionPacket(255);

				NetworkManager.Instance.SendMessage(packet);
			}
		}
	}

	public void Push()
	{
		if (!pushing)
		{
			pushing = true;
			++playerCount;

			Packet packet = new Packet();
			packet.type = 1;
			packet.id = GameManager.Instance.ThisPlayer;
			packet.action = new ActionPacket(2);

			NetworkManager.Instance.SendMessage(packet);

			Packet packet1 = new Packet();
			packet1.type = 1;
			packet1.id = entity.id;
			packet1.action = new ActionPacket((byte)GameManager.Instance.ThisPlayer);

			NetworkManager.Instance.SendMessage(packet1);
		}
	}

	public void StopPush()
	{
		if (pushing)
		{
			pushing = false;
			--playerCount;

			Packet packet = new Packet();
			packet.type = 1;
			packet.id = GameManager.Instance.ThisPlayer;
			packet.action = new ActionPacket(3);

			NetworkManager.Instance.SendMessage(packet);

			Packet packet1 = new Packet();
			packet1.type = 1;
			packet1.id = entity.id;
			packet1.action = new ActionPacket((byte)(100 + GameManager.Instance.ThisPlayer));

			NetworkManager.Instance.SendMessage(packet1);
		}
	}

	public void OtherComplete()
	{
		if(!complete)
		{
			complete = true;
			interactable.canInteract = false;
			transform.position = endPosition;
			onComplete.Invoke();
			onCompleteOther.Invoke();
		}
	}

	public void OtherPush(ushort id)
	{
		if (!othersPushing[id])
		{
			++playerCount;
			othersPushing[id] = true;
		}
	}

	public void OtherStop(ushort id)
	{
		if (othersPushing[id])
		{
			--playerCount;
			othersPushing[id] = false;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!complete && other.tag == "Player") { interactable.canInteract = true; }
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.tag == "Player") { interactable.canInteract = false; }
	}
}
