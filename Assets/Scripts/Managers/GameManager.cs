using Steamworks;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Experimental.GraphView.GraphView;

struct TempEntity
{
	public TempEntity(byte id, byte type)
	{
		this.id = id;
		this.type = type;
	}

	public byte id;
	public byte type;
}

//TODO: Associate a steamID with an entity

public class GameManager : Singleton<GameManager>
{
	private bool isServer;
	private bool inLobby;
	private byte level;

	private TempEntity[] tempPlayers;
	private Entity[] entities;
	private Transform[] playerSpawnPoints;
	private Transform[] enemySpawnPoints;

	private PrefabManager prefabManager;

	private byte thisPlayer = 255;

	protected override void Awake()
	{
		base.Awake();

		prefabManager = FindFirstObjectByType<PrefabManager>();

		isServer = NetworkManager.Instance.activeSocketServer;

		tempPlayers = new TempEntity[4] {
			new TempEntity(255, 255),
			new TempEntity(255, 255),
			new TempEntity(255, 255),
			new TempEntity(255, 255)
		};

		entities = new Entity[256];
	}

	private void Update()
	{

	}

	public void SelectLevel(TMP_Dropdown change)
	{
		level = (byte)change.value;
	}

	public void StartGame()
	{
		Packet packet = new Packet();
		packet.type = 5;
		packet.id = level;

		NetworkManager.Instance.SendMessage(packet);
		LoadLevel(level);
	}

	public void LoadLevel(byte level)
	{
		for(int i = 0; i < tempPlayers.Length; ++i)
		{
			tempPlayers[i].id = entities[i].id;
			tempPlayers[i].type = entities[i].type;
		}

		string scene;
		switch (level)
		{
			default:
			case 0: scene = "c1m1_Naptime"; break;
			case 1: scene = "c1m2_Library"; break;
			case 2: scene = "c1m3_Playground"; break;
			case 3: scene = "c1m4_Cellar"; break;
			case 4: scene = "c1m5_Corruption"; break;
		}

		SceneLoader.Instance.LoadScene(scene);
	}

	public void OnLevelLoad(Transform[] playerSpawnPoints, Transform[] enemySpawnPoints)
	{
		this.playerSpawnPoints = playerSpawnPoints;
		this.enemySpawnPoints = enemySpawnPoints;

		//TODO: spawn players using tempPlayers, setup map
	}

	public async void CreateLobby(string lobbyName)
	{
		if (lobbyName.Length == 0) { return; }

		Dictionary<string, string> lobbyData = new Dictionary<string, string> {
			{ "Name", lobbyName },
			{ "PlayerInfo0", "44" },
			{ "PlayerInfo1", "44" },
			{ "PlayerInfo2", "44" },
			{ "PlayerInfo3", "44" },
			{ "PlayerSteam0", "0" },
			{ "PlayerSteam1", "0" },
			{ "PlayerSteam2", "0" },
			{ "PlayerSteam3", "0" },
		};

		bool result = await NetworkManager.Instance.CreateLobby(lobbyData, 4);

		if (result)
		{
			SceneLoader.Instance.LoadScene("Lobby");
		}
		else
		{
			//TODO: Failure to join message
		}
	}

	public void OnJoinLobby(Transform[] spawnPoints)
	{
		playerSpawnPoints = spawnPoints;
		inLobby = true;

		if (isServer)
		{
			entities[0] = Instantiate(prefabManager.LobbyPlayer, playerSpawnPoints[0].position, playerSpawnPoints[0].rotation).GetComponent<Entity>();
			entities[0].id = 0;
			entities[0].type = 0;

			thisPlayer = 0;

			NetworkManager.Instance.currentLobby.SetData("PlayerInfo0", "00");
			NetworkManager.Instance.currentLobby.SetData("PlayerSteam0", NetworkManager.Instance.PlayerId.Value.ToString());
		}
		else
		{
			Lobby lobby = NetworkManager.Instance.currentLobby;

			bool[] taken = new bool[4];
			byte playerNum = 255;

			for (byte i = 0; i < 4; ++i)
			{
				string playerInfo = lobby.GetData("PlayerInfo" + i);

				byte id = (byte)(playerInfo[0] - '0');
				byte spawn = (byte)(playerInfo[1] - '0');

				if (id == 4 || spawn == 4) { if (playerNum == 255) { playerNum = i; } continue; }

				taken[i] = true;

				entities[id] = Instantiate(prefabManager.LobbyPlayer, playerSpawnPoints[spawn].position, playerSpawnPoints[spawn].rotation).GetComponent<Entity>();
				entities[id].id = id;
				entities[id].type = id;
			}

			for (byte i = 0; i < 4; ++i) { if (taken[i] == false) { thisPlayer = i; break; } }

			entities[thisPlayer] = Instantiate(prefabManager.LobbyPlayer, playerSpawnPoints[thisPlayer].position, playerSpawnPoints[thisPlayer].rotation).GetComponent<Entity>();
			entities[thisPlayer].id = thisPlayer;
			entities[thisPlayer].type = thisPlayer;

			lobby.SetData("PlayerInfo" + playerNum, "" + thisPlayer + thisPlayer);
			lobby.SetData("PlayerSteam" + playerNum, "" + NetworkManager.Instance.PlayerId.Value.ToString());

			Packet packet = new Packet();
			packet.type = 6;
			packet.id = thisPlayer;
			packet.spawn.spawn = thisPlayer;

			NetworkManager.Instance.SendMessage(packet);
		}
	}

	//Callbacks
	public void PlayerJoined(Friend player) //This is called before OnJoinLobby
	{

	}

	public void PlayerLeft(Friend player)
	{
		//TODO: figure out which player left with steamId
		//TODO: Despawn them
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
		if(inLobby)
		{
			//TODO: find new player info from lobby
			//TODO: Spawn player
		}
		else
		{

		}
	}
}
