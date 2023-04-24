using Steamworks;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class GameManager : Singleton<GameManager>
{
	private static readonly int MAX_ENEMY_COUNT = 30;
	private static readonly int MAX_SPECIAL_COUNT = 2;

	public bool IsServer { get; private set; }
	private bool inLobby;
	private byte levelNum;

	private byte[] tempPlayers;
	private Entity[] entities;
	private Transform[] lobbySpawnpoints;
	private Stack<byte> enemyIndices = new Stack<byte>(30);
	private Stack<byte> objectiveIndices = new Stack<byte>(10);
	private Stack<byte> projectileIndices = new Stack<byte>(206);
	private int enemyCount = 0;
	private int specialCount = 0;

	private LevelManager level;

	private PrefabManager prefabManager;
	public PrefabManager PrefabManager { get => prefabManager; }

	public byte ThisPlayer { get; private set; } = 255;
	public byte PlayerCount { get; private set; } = 0; //TODO: Take into account player deaths
	public bool Network { get; private set; } = true;

	protected override void Awake()
	{
		base.Awake();

		prefabManager = FindFirstObjectByType<PrefabManager>();

		tempPlayers = new byte[4] { 255, 255, 255, 255 };

		entities = new Entity[256];

		for (byte i = 4; i < 34; ++i)
		{
			enemyIndices.Push(i);
		}

		for (byte i = 44; i < 49; ++i)
		{
			objectiveIndices.Push(i);
		}

		for (byte i = 49; i < 255; ++i)
		{
			projectileIndices.Push(i);
		}
	}

	private void FixedUpdate()
	{
		Network = !Network;
	}

	private void Update()
	{
		if (IsServer && !inLobby)
		{
			if(enemyCount < MAX_ENEMY_COUNT)
			{
				byte id = enemyIndices.Pop();
				byte spawn = level.RandomEnemySpawn();
				Transform transform = level.GetEnemySpawn(spawn);
				entities[id] = Instantiate(prefabManager.Enemy, transform.position, transform.rotation).GetComponent<Entity>();
				entities[id].id = id;
				entities[id].SetModel();

				Packet packet = new Packet();
				packet.id = id;
				packet.type = 6;
				packet.spawn = new SpawnPacket(spawn);

				NetworkManager.Instance.SendMessage(packet);

				++enemyCount;
			}

			if(specialCount < MAX_SPECIAL_COUNT)
			{
				byte spawn = level.RandomEnemySpawn();
				Transform transform = level.GetEnemySpawn(spawn);
				entities[34] = Instantiate(prefabManager.Enemy, transform.position, transform.rotation).GetComponent<Entity>();
				entities[34].id = 34;
				entities[34].SetModel();

				Packet packet = new Packet();
				packet.id = 34;
				packet.type = 6;
				packet.spawn = new SpawnPacket(spawn);

				NetworkManager.Instance.SendMessage(packet);

				++specialCount;
			}
		}
	}

	public void SelectLevel(TMP_Dropdown change)
	{
		if (IsServer)
		{
			levelNum = (byte)change.value;
		}
	}

	public void StartGame()
	{
		if (IsServer)
		{
			Packet packet = new Packet();
			packet.type = 5;
			packet.id = levelNum;

			NetworkManager.Instance.SendMessage(packet);
			LoadLevel(packet);
		}
	}

	public void ChangeLevel(byte level)
	{
		if (IsServer)
		{
			Packet packet = new Packet();
			packet.type = 5;
			packet.id = level;

			NetworkManager.Instance.SendMessage(packet);
			LoadLevel(packet);
		}
	}

	public void OnLevelLoad(LevelManager level)
	{
		this.level = level;
		inLobby = false;

		foreach (byte id in tempPlayers)
		{
			if (id != 255)
			{
				Transform transform = level.GetPlayerSpawn(id);

				if (id == ThisPlayer)
				{
					entities[id] = Instantiate(prefabManager.Player, transform.position, transform.rotation).GetComponent<Entity>();
					entities[id].id = id;
				}
				else
				{
					entities[id] = Instantiate(prefabManager.NetworkPlayer, transform.position, transform.rotation).GetComponent<Entity>();
					entities[id].id = id;
					entities[id].SetModel();
				}
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
			IsServer = true;
			SceneLoader.Instance.LoadScene("Lobby");
		}
		else
		{
			//TODO: Failure to create lobby message
		}
	}

	public void OnJoinLobby(Transform[] spawnPoints)
	{
		lobbySpawnpoints = spawnPoints;
		inLobby = true;
		PlayerCount = 1;

		if (IsServer)
		{
			entities[0] = Instantiate(prefabManager.LobbyPlayer, spawnPoints[0].position, spawnPoints[0].rotation).GetComponent<Entity>();
			entities[0].id = 0;
			entities[0].GetComponent<LobbyPlayer>().name.text = NetworkManager.Instance.PlayerName;
			entities[0].SetModel();

			ThisPlayer = 0;

			NetworkManager.Instance.currentLobby.SetData("Player0", NetworkManager.Instance.PlayerId.Value.ToString());
		}
		else
		{
			Lobby lobby = NetworkManager.Instance.currentLobby;
			IEnumerable<Friend> members = lobby.Members;

			for (byte i = 0; i < 4; ++i)
			{
				ulong steamId = ulong.Parse(lobby.GetData("Player" + i));

				if (steamId == 0 || steamId == NetworkManager.Instance.PlayerId.Value)
				{
					if (ThisPlayer == 255)
					{
						ThisPlayer = i;
						++PlayerCount;

						entities[ThisPlayer] = Instantiate(prefabManager.LobbyPlayer, spawnPoints[ThisPlayer].position, spawnPoints[ThisPlayer].rotation).GetComponent<Entity>();
						entities[ThisPlayer].id = ThisPlayer;
						entities[ThisPlayer].GetComponent<LobbyPlayer>().name.text = NetworkManager.Instance.PlayerName;
						entities[ThisPlayer].SetModel();
					}

					continue;
				}

				string steamName = "";
				foreach (Friend f in members)
				{
					if (f.Id.Value == steamId)
					{
						steamName = f.Name;
						break;
					}
				}

				entities[i] = Instantiate(prefabManager.LobbyPlayer, spawnPoints[i].position, spawnPoints[i].rotation).GetComponent<Entity>();
				entities[i].id = i;
				entities[i].GetComponent<LobbyPlayer>().name.text = steamName;
				entities[i].SetModel();
			}
		}
	}

	public void LeaveLobby()
	{
		Lobby lobby = NetworkManager.Instance.currentLobby;

		if (lobby.MemberCount > 1)
		{
			for (byte i = 0; i < 4; ++i)
			{
				ulong steamId = ulong.Parse(lobby.GetData("Player" + i));

				if (steamId != 0 && steamId != NetworkManager.Instance.PlayerId.Value)
				{
					Packet packet = new Packet();
					packet.type = 7;
					packet.owner = new OwnerPacket(steamId);

					NetworkManager.Instance.SendMessage(packet);
				}
			}
		}

		PlayerCount = 0;
		ThisPlayer = 255;
		IsServer = false;
		inLobby = false;
	}

	public void Shoot(byte type)
	{
		if (IsServer)
		{
			byte id = projectileIndices.Pop();

			Entity entity = entities[ThisPlayer];

			entities[id] = Instantiate(prefabManager.Projectile, entity.shoot.position, entity.shoot.rotation).GetComponent<Entity>();
			entities[id].id = id;
			entities[id].GetComponent<Projectile>().SetSpeed(100);

			Packet packet = new Packet();
			packet.id = id;
			packet.type = 6;
			packet.spawn = new SpawnPacket(ThisPlayer);

			NetworkManager.Instance.SendMessage(packet);
		}
		else
		{
			Packet packet = new Packet();
			packet.id = 255;
			packet.type = 6;
			packet.spawn = new SpawnPacket(ThisPlayer);

			NetworkManager.Instance.SendMessage(packet);
		}
	}

	public void Destroy(Entity obj)
	{
		if (obj.id > 3 && obj.id < 39)
		{
			enemyIndices.Push(obj.id);
		}
		else if (obj.id > 38 && obj.id < 49)
		{
			objectiveIndices.Push(obj.id);
		}
		else if (obj.id > 48 && obj.id < 255)
		{
			projectileIndices.Push(obj.id);
		}

		Packet packet = new Packet();
		packet.id = obj.id;
		packet.type = 7;

		NetworkManager.Instance.SendMessage(packet);

		Destroy(obj.gameObject);
	}

	//Callbacks
	public void PlayerJoined(Friend player) //This is called before OnJoinLobby
	{
		Lobby lobby = NetworkManager.Instance.currentLobby;

		for (byte i = 0; i < 4; ++i)
		{
			ulong steamId = ulong.Parse(lobby.GetData("Player" + i));

			if (steamId == 0)
			{
				Transform transform;
				if (inLobby) { transform = lobbySpawnpoints[i]; }
				else { transform = level.GetPlayerSpawn(i); }

				entities[i] = Instantiate(prefabManager.LobbyPlayer, transform.position, transform.rotation).GetComponent<Entity>();
				entities[i].id = i;
				entities[i].GetComponent<LobbyPlayer>().name.text = player.Name;
				entities[i].SetModel();
				++PlayerCount;

				lobby.SetData("Player" + i, player.Id.Value.ToString());
				break;
			}
		}
	}

	public void PlayerLeft(Friend player)
	{
		Lobby lobby = NetworkManager.Instance.currentLobby;

		for (int i = 0; i < 4; ++i)
		{
			ulong steamId = ulong.Parse(lobby.GetData("Player" + i));

			if (steamId == player.Id.Value)
			{
				Destroy(entities[i].gameObject);
				entities[i] = null;
				--PlayerCount;

				lobby.SetData("Player" + i, "0");

				break;
			}
		}
	}

	public void ReceiveTransform(Packet transform)
	{
		entities[transform.id].SetTransform(transform.transform);
	}

	public void Action(Packet action)
	{
		entities[action.id].DoAction(action.action);
	}

	public void Health(Packet health)
	{

	}

	public void Inventory(Packet inventory)
	{

	}

	public void GameTrigger(Packet packet)
	{

	}

	public void LoadLevel(Packet packet)
	{
		for (int i = 0; i < tempPlayers.Length; ++i)
		{
			if (entities[i] != null)
			{
				tempPlayers[i] = entities[i].id;
			}
		}

		string scene;
		switch (packet.id)
		{
			default:
			case 0: scene = "c1m1_NapRoom"; break;
			case 1: scene = "c1m2_Library"; break;
			case 2: scene = "c1m3_Playground"; break;
			case 3: scene = "c1m4_Cellar"; break;
			case 4: scene = "c1m5_Corruption"; break;
		}

		//TODO: Scene transition
		SceneLoader.Instance.LoadScene(scene);
	}

	public void Spawn(Packet packet)
	{
		if(packet.id < 4)
		{
			//player
		}
		else if(packet.id < 44)
		{
			Transform transform = level.GetEnemySpawn(packet.spawn.spawn);
			entities[packet.id] = Instantiate(prefabManager.Enemy, transform.position, transform.rotation).GetComponent<Entity>();
			entities[packet.id].id = packet.id;
			entities[packet.id].SetModel();
		}
		else if(packet.id < 49)
		{
			//objective
		}
		else
		{
			if (IsServer && packet.id == 255)
			{
				byte id = projectileIndices.Pop();

				Entity entity = entities[packet.spawn.spawn];

				entities[id] = Instantiate(prefabManager.Projectile, entity.shoot.position, entity.shoot.rotation).GetComponent<Entity>();
				entities[id].id = id;
				entities[id].GetComponent<Projectile>().SetSpeed(100);

				Packet newPacket = new Packet();
				newPacket.id = id;
				newPacket.type = 6;
				newPacket.spawn = packet.spawn;

				NetworkManager.Instance.SendMessage(newPacket);
			}
			else if (packet.id != 255)
			{
				Entity entity = entities[packet.spawn.spawn];

				entities[packet.id] = Instantiate(prefabManager.NetworkProjectile, entity.shoot.position, entity.shoot.rotation).GetComponent<Entity>();
				entities[packet.id].id = packet.id;
			}
		}
	}

	public void Despawn(Packet packet)
	{
		if (packet.id > 3 && packet.id < 39)
		{
			enemyIndices.Push(packet.id);
		}
		else if (packet.id > 38 && packet.id < 49)
		{
			objectiveIndices.Push(packet.id);
		}
		else if (packet.id > 48 && packet.id < 255)
		{
			projectileIndices.Push(packet.id);
		}

		Destroy(entities[packet.id].gameObject);
	}

	public void OwnerChange(Packet packet)
	{
		if (packet.owner.steamId == NetworkManager.Instance.PlayerId.Value)
		{
			IsServer = true;
		}
	}
}
