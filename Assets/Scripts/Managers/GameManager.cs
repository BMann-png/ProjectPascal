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
		if(isServer && !inLobby)
		{
			//TODO: Run the game
		}
	}

	public void SelectLevel(TMP_Dropdown change)
	{
		if (isServer)
		{
			level = (byte)change.value;
		}
	}

	//TODO: only show level select and start game if owner
	public void StartGame()
	{
		if (isServer)
		{
			Packet packet = new Packet();
			packet.type = 5;
			packet.id = level;

			NetworkManager.Instance.SendMessage(packet);
			LoadLevel(level);
		}
	}

	public void LoadLevel(byte level)
	{
		for(int i = 0; i < tempPlayers.Length; ++i)
		{
			if (entities[i] != null)
			{
				tempPlayers[i].id = entities[i].id;
				tempPlayers[i].type = entities[i].type;
			}
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

		foreach (TempEntity e in tempPlayers)
		{
			if (e.id != 255)
			{
				if (e.id == thisPlayer)
				{
					entities[e.id] = Instantiate(prefabManager.Player, playerSpawnPoints[e.id].position, playerSpawnPoints[e.id].rotation).GetComponent<Entity>();
				}
				else
				{
					entities[e.id] = Instantiate(prefabManager.NetworkPlayer, playerSpawnPoints[e.id].position, playerSpawnPoints[e.id].rotation).GetComponent<Entity>();
				}

				entities[e.id].id = e.id;
				entities[e.id].type = e.type;
			}
		}
	}

	public async void CreateLobby(string lobbyName)
	{
		if (lobbyName.Length == 0) { return; }

		Dictionary<string, string> lobbyData = new Dictionary<string, string> {
			{ "Name", lobbyName },
			{ "Player0", "0" },
			{ "Player1", "0" },
			{ "Player2", "0" },
			{ "Player3", "0" },
		};

		bool result = await NetworkManager.Instance.CreateLobby(lobbyData, 4);

		if (result)
		{
			isServer = true;
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

			NetworkManager.Instance.currentLobby.SetData("Player0", NetworkManager.Instance.PlayerId.Value.ToString());
		}
		else
		{
			Lobby lobby = NetworkManager.Instance.currentLobby;
			IEnumerable<Friend> members = lobby.Members;

			for (byte i = 0; i < 4; ++i)
			{
				ulong steamId = ulong.Parse(lobby.GetData("Player" + i));

				if (steamId == 0)
				{
					if (thisPlayer == 255)
					{
						thisPlayer = i;

						entities[thisPlayer] = Instantiate(prefabManager.LobbyPlayer, playerSpawnPoints[thisPlayer].position, playerSpawnPoints[thisPlayer].rotation).GetComponent<Entity>();
						entities[thisPlayer].id = thisPlayer;
						entities[thisPlayer].type = thisPlayer;
					}

					continue;
				}

				entities[i] = Instantiate(prefabManager.LobbyPlayer, playerSpawnPoints[i].position, playerSpawnPoints[i].rotation).GetComponent<Entity>();
				entities[i].id = i;
				entities[i].type = i;
			}
		}
	}

	public void LeaveLobby()
	{
		isServer = false;
		thisPlayer = 255;
	}

	//Callbacks
	public void PlayerJoined(Friend player) //This is called before OnJoinLobby
	{
		//TODO: Send message of player joining

		Lobby lobby = NetworkManager.Instance.currentLobby;

		for (byte i = 0; i < 4; ++i)
		{
			ulong steamId = ulong.Parse(lobby.GetData("Player" + i));

			if (steamId == 0)
			{
				entities[i] = Instantiate(prefabManager.LobbyPlayer, playerSpawnPoints[i].position, playerSpawnPoints[i].rotation).GetComponent<Entity>();
				entities[i].id = i;
				entities[i].type = i;

				lobby.SetData("Player" + i, player.Id.Value.ToString());
				break;
			}
		}
	}

	public void PlayerLeft(Friend player)
	{
		//TODO: Send message of player leaving

		Lobby lobby = NetworkManager.Instance.currentLobby;

		for (int i = 0; i < 4; ++i)
		{
			ulong steamId = ulong.Parse(lobby.GetData("Player" + i));

			if (steamId == player.Id.Value)
			{
				Destroy(entities[i].gameObject);
				entities[i] = null;

				if (isServer) { lobby.SetData("Player" + i, "0"); }
				
				break;
			}
		}
	}

	public void GameTrigger(byte trigger)
	{

	}

	public void ReceiveTransform(byte id, TransformPacket transform)
	{
		if (id != thisPlayer)
		{
			entities[id].SetTransform(transform);
		}
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
