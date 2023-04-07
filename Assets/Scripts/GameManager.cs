using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
	private bool isServer;
	private byte level;

	protected override void Awake()
	{
		base.Awake();

		isServer = NetworkManager.Instance.activeSocketServer;
		DontDestroyOnLoad(gameObject);
	}

	private void Update()
	{

	}

	public void SelectLevel(TMP_Dropdown change)
	{
		level = (byte)change.value;
	}

	public void Start()
	{
		Packet packet = new Packet();
		packet.type = 5;
		packet.id = level;

		NetworkManager.Instance.SendMessage(packet);
	}

	public void LoadLevel(byte level)
	{
		string scene;
		switch(level)
		{
			default:
			case 0: scene = "c1m1_Naptime"; break;
			case 1: scene = "c1m2_Library"; break;
			case 2: scene = "c1m3_Playground"; break;
			case 3: scene = "c1m4_Cellar"; break;
			case 4: scene = "c1m5_Corruption"; break;
		}

		SceneLoader.Instance.SetOnLoad(OnLevelLoad);
		SceneLoader.Instance.LoadScene(scene);
	}

	public void OnLevelLoad()
	{
		//TODO: spawn players, setup map
	}

	public void GameTrigger(byte trigger)
	{

	}

	public void ReceiveTransform(byte id, TransformPacket transform)
	{

	}

	public void Action(byte id, ActionPacket action)
	{

	}

	public void Health(byte id, ActionPacket health)
	{

	}

	public void Inventory(byte id, InventoryPacket inventory)
	{

	}

	public void Spawn(byte id, SpawnPacket spawn)
	{

	}
}
