using Steamworks;
using Steamworks.Data;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
	private static readonly object joinLock = new object();

	public List<GameObject> playerLocations = new List<GameObject>();
	private static readonly int MAX_ENEMY_COUNT = 30;
	private static readonly int MAX_SPECIAL_COUNT = 2;
	public static readonly ushort INVALID_ID = 65535;

	public bool IsServer { get; private set; }
	public bool InLobby { get; private set; }
	private byte levelNum;

	private Queue<ushort> unspawnedPlayers = new Queue<ushort>();
	private Transform healthBarHolder;
	private List<GameObject> healthBars = new List<GameObject>();
	private Entity[] entities;
	private Transform[] lobbySpawnpoints;
	private Stack<ushort> enemyIndices = new Stack<ushort>(30);
	private Stack<ushort> interactableIndices = new Stack<ushort>(5);
	private Stack<ushort> projectileIndices = new Stack<ushort>(206);
	private bool[] specialsSpawned = new bool[10];
	private int enemyCount = 0;
	private int specialCount = 0;

	private LevelManager level;

	private PrefabManager prefabManager;
	public PrefabManager PrefabManager { get => prefabManager; }

	private SceneLoader sceneLoader;
	public SceneLoader SceneLoader { get => sceneLoader; }

	public bool Loading { get; private set; } = false;
	public bool Fading { get; private set; } = false;

	public ushort ThisPlayer { get; private set; } = INVALID_ID;
	public byte PlayerCount { get; private set; } = 0;
	public byte AlivePlayers { get; private set; } = 0;
	private byte loadedPlayers = 0;

	protected override void Awake()
	{
		base.Awake();

		healthBarHolder = GameObject.FindGameObjectWithTag("HealthBars").transform;
		prefabManager = FindFirstObjectByType<PrefabManager>();
		sceneLoader = FindFirstObjectByType<SceneLoader>();

		entities = new Entity[65536];

		for (ushort i = 4; i < 34; ++i) { enemyIndices.Push(i); }
		for (ushort i = 44; i < 10001; ++i) { interactableIndices.Push(i); }
		for (ushort i = 10001; i < INVALID_ID; ++i) { projectileIndices.Push(i); }
	}

	private void Update()
	{
		if (IsServer && !InLobby && playerLocations.Count == AlivePlayers)
		{
			if (enemyCount < MAX_ENEMY_COUNT)
			{
				ushort id = enemyIndices.Pop();
				ushort spawn = level.RandomEnemySpawn();
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

			if (specialCount < MAX_SPECIAL_COUNT)
			{
				ushort id = INVALID_ID;
				while (true)
				{
					id = (byte)Random.Range(0, 3);

					if (specialsSpawned[id] == false) { break; }
				}

				specialsSpawned[id] = true;
				id += 34;
				ushort spawn = level.RandomEnemySpawn();
				Transform transform = level.GetEnemySpawn(spawn);
				entities[id] = Instantiate(prefabManager.Enemy, transform.position, transform.rotation).GetComponent<Entity>();
				entities[id].id = id;
				entities[id].SetModel();

				Packet packet = new Packet();
				packet.id = id;
				packet.type = 6;
				packet.spawn = new SpawnPacket(spawn);

				NetworkManager.Instance.SendMessage(packet);

				++specialCount;
			}
		}
	}

	public void SelectLevel(TMP_Dropdown change)
	{
		if (IsServer && !Loading)
		{
			levelNum = (byte)change.value;
		}
	}

	public void StartGame()
	{
		if (IsServer && !Loading)
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
		InLobby = false;
		AlivePlayers = PlayerCount;

		if (++loadedPlayers == PlayerCount) { FinishLoading(); }

		Packet packet = new Packet();
		packet.type = 1;
		packet.id = ThisPlayer;
		packet.action = new ActionPacket(255);

		NetworkManager.Instance.SendMessage(packet);
	}

	private void FinishLoading()
	{
		if (IsServer)
		{
			for (ushort i = 0; i < level.InteractableSpawnCount(); ++i)
			{
				InteractableSpawner spawner = level.GetInteractableSpawn(i);

				if (spawner.guaranteeSpawn)
				{
					ushort id = interactableIndices.Pop();

					if (spawner.type == 255) { spawner.type = (byte)Random.Range(0, 5); }

					if (spawner.type < 100)
					{
						entities[id] = Instantiate(prefabManager.Pickups[spawner.type - 1], spawner.transform.position, spawner.transform.rotation).GetComponentInChildren<Entity>();
						entities[id].id = id;
						Interactable inter = entities[id].GetComponent<Interactable>();
						inter.SetEvents(spawner.onInteract, spawner.onStopInteract, spawner.onComplete);
						inter.id = spawner.id;
					}
					else if (spawner.type < 255)
					{
						entities[id] = Instantiate(prefabManager.Pushables[spawner.type - 100], spawner.transform.position, spawner.transform.rotation).GetComponentInChildren<Entity>();
						entities[id].id = id;
						Interactable inter = entities[id].GetComponent<Interactable>();
						inter.SetEvents(spawner.onInteract, spawner.onStopInteract, spawner.onComplete);
						inter.id = spawner.id;
					}

					Packet packet = new Packet();
					packet.type = 6;
					packet.id = id;
					packet.spawn = new SpawnPacket(i, spawner.type);

					NetworkManager.Instance.SendMessage(packet);
				}
				else
				{
					//TODO: spawn chance
				}
			}
		}

		byte healthBarId = 1;
		while (unspawnedPlayers.Count > 0)
		{
			ushort id = unspawnedPlayers.Dequeue();
			Transform transform = level.GetPlayerSpawn(id);

			if (id == ThisPlayer)
			{
				entities[id] = Instantiate(prefabManager.Player, transform.position, transform.rotation).GetComponent<Entity>();
				entities[id].id = id;
				playerLocations.Add(entities[id].gameObject);

				HealthBar bar = Instantiate(prefabManager.HealthBar, healthBarHolder).GetComponent<HealthBar>();
				healthBars.Add(bar.gameObject);

				entities[id].GetComponent<Health>().AttachHealthBar(bar);
				bar.SetImage(id);
				bar.gameObject.SetActive(true);
			}
			else
			{
				entities[id] = Instantiate(prefabManager.NetworkPlayer, transform.position, transform.rotation).GetComponent<Entity>();
				entities[id].id = id;
				entities[id].SetModel();
				playerLocations.Add(entities[id].gameObject);

				HealthBar bar = Instantiate(prefabManager.HealthBar, healthBarHolder).GetComponent<HealthBar>();
				healthBars.Add(bar.gameObject);

				entities[id].GetComponent<Health>().AttachHealthBar(bar);
				bar.SetImage(id);
				bar.gameObject.SetActive(true);

				++healthBarId;
			}
		}

		SceneLoader.SetLoadingScreen(false);
		SceneLoader.ResetScreen();
		Loading = false;
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
			SceneLoader.LoadScene("Lobby");
		}
		else
		{
			//TODO: Failure to create lobby message
		}
	}

	public void SetupLobby(Transform[] spawnPoints)
	{
		lobbySpawnpoints = spawnPoints;

		if (IsServer)
		{
			InLobby = true;
			entities[0] = Instantiate(prefabManager.LobbyPlayer, spawnPoints[0].position, spawnPoints[0].rotation).GetComponent<Entity>();
			entities[0].id = 0;
			entities[0].GetComponent<LobbyPlayer>().name.text = NetworkManager.Instance.PlayerName;
			entities[0].SetModel();

			ThisPlayer = 0;

			NetworkManager.Instance.currentLobby.SetData("Player0", NetworkManager.Instance.PlayerId.Value.ToString());

			PlayerCount = 1;
		}
	}

	public void OnJoinGame(byte data)
	{
		if (ThisPlayer == 255)
		{
			Debug.LogError("We joined without receiving an ID");
			NetworkManager.Instance.currentLobby.Leave();
			//TODO: Load back into the lobby browser
			return;
		}

		Lobby lobby = NetworkManager.Instance.currentLobby;
		PlayerCount = (byte)lobby.MemberCount;

		if (data == 255)
		{
			InLobby = true;
			IEnumerable<Friend> members = lobby.Members;

			for (ushort i = 0; i < 4; ++i)
			{
				ulong steamId = ulong.Parse(lobby.GetData("Player" + i));

				if (steamId != 0)
				{
					string steamName = "";
					foreach (Friend f in members)
					{
						if (f.Id.Value == steamId)
						{
							steamName = f.Name;
							break;
						}
					}

					entities[i] = Instantiate(prefabManager.LobbyPlayer, lobbySpawnpoints[i].position, lobbySpawnpoints[i].rotation).GetComponent<Entity>();
					entities[i].id = i;
					entities[i].GetComponent<LobbyPlayer>().name.text = steamName;
					entities[i].SetModel();
				}
				else if (i == ThisPlayer)
				{
					entities[i] = Instantiate(prefabManager.LobbyPlayer, lobbySpawnpoints[i].position, lobbySpawnpoints[i].rotation).GetComponent<Entity>();
					entities[i].id = i;
					entities[i].GetComponent<LobbyPlayer>().name.text = NetworkManager.Instance.PlayerName;
					entities[i].SetModel();
				}
			}
		}
		else if (data > 200)
		{
			StartLoad(data - 200);
		}
		else if (data > 100)
		{
			IEnumerable<Friend> members = lobby.Members;

			for (byte i = 0; i < 4; ++i)
			{
				ulong steamId = ulong.Parse(lobby.GetData("Player" + i));

				if (steamId != 0)
				{
					string steamName = "";
					foreach (Friend f in members)
					{
						if (f.Id.Value == steamId)
						{
							steamName = f.Name;
							break;
						}
					}

					entities[i] = Instantiate(prefabManager.LobbyPlayer, lobbySpawnpoints[i].position, lobbySpawnpoints[i].rotation).GetComponent<Entity>();
					entities[i].id = i;
					entities[i].GetComponent<LobbyPlayer>().name.text = steamName;
					entities[i].SetModel();
				}
				else if (i == ThisPlayer)
				{
					entities[i] = Instantiate(prefabManager.LobbyPlayer, lobbySpawnpoints[i].position, lobbySpawnpoints[i].rotation).GetComponent<Entity>();
					entities[i].id = i;
					entities[i].GetComponent<LobbyPlayer>().name.text = NetworkManager.Instance.PlayerName;
					entities[i].SetModel();
				}
			}

			StartLoad(data - 100);
		}
		else
		{
			//TODO: In game, load scene without caring if others are loaded
			//This could be complicated, we don't know which players are alive
			//We may just bar players from joining if we are in game
		}
	}

	public void AddPlayer(ushort id)
	{
		Transform transform;
		if (InLobby)
		{
			transform = lobbySpawnpoints[id];
			entities[id] = Instantiate(prefabManager.LobbyPlayer, transform.position, transform.rotation).GetComponent<Entity>();
			entities[id].id = id;
			entities[id].SetModel();

			Lobby lobby = NetworkManager.Instance.currentLobby;
			IEnumerable<Friend> members = lobby.Members;
			ulong steamId = ulong.Parse(lobby.GetData("Player" + id));

			string steamName = "";
			foreach (Friend f in members)
			{
				if (f.Id.Value == steamId)
				{
					steamName = f.Name;
					break;
				}
			}

			entities[id].GetComponent<LobbyPlayer>().name.text = steamName;

			if (Fading) { unspawnedPlayers.Enqueue(id); }
		}
		else if (Loading)
		{
			unspawnedPlayers.Enqueue(id);
		}
		else
		{
			transform = level.GetPlayerSpawn(id);
			entities[id] = Instantiate(prefabManager.NetworkPlayer, transform.position, transform.rotation).GetComponent<Entity>();
			entities[id].id = id;
			entities[id].SetModel();
		}
	}

	public void LeaveLobby()
	{
		Lobby lobby = NetworkManager.Instance.currentLobby;

		if (lobby.MemberCount > 1)
		{
			for (ushort i = 0; i < 4; ++i)
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
		ThisPlayer = INVALID_ID;
		IsServer = false;
		InLobby = false;
	}

	public void PickupItem(byte id)
	{
		//0 - sqirt gun
		//1 - bubble gun
		//2 - dart gun
		//3 - gun
		//4 - pacifier

		//TODO: Not sure which weapons go in which slot
		entities[ThisPlayer].GetComponent<Inventory>().SetWeapon(0, id);
	}

	public void PushPlayer(Vector3 dir)
	{
		entities[ThisPlayer].GetComponent<CharacterController>().Move(dir);
	}

	public void Shoot(byte type)
	{
		if (IsServer)
		{
			ushort id = projectileIndices.Pop();

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
			packet.id = INVALID_ID;
			packet.type = 6;
			packet.spawn = new SpawnPacket(ThisPlayer);

			NetworkManager.Instance.SendMessage(packet);
		}
	}

	public void Destroy(Entity obj)
	{
		if (obj.id < 4) //Player
		{
			return;
		}
		else if (obj.id < 34) //Common Enemy
		{
			--enemyCount;
			enemyIndices.Push(obj.id);
		}
		else if (obj.id < 44) //Special Enemy
		{
			--specialCount;
			specialsSpawned[obj.id - 34] = false;
		}
		else if (obj.id < 10001)
		{
			interactableIndices.Push(obj.id);
		}
		else if (obj.id < INVALID_ID)
		{
			projectileIndices.Push(obj.id);
		}

		Packet packet = new Packet();
		packet.id = obj.id;
		packet.type = 7;

		NetworkManager.Instance.SendMessage(packet);
	}

	//-----CALLBACKS-----

	public void PlayerJoined(Packet packet)
	{
		if (IsServer && packet.id == INVALID_ID)
		{
			lock (joinLock)
			{
				++PlayerCount;

				Lobby lobby = NetworkManager.Instance.currentLobby;

				for (ushort i = 0; i < 4; ++i)
				{
					ulong steamId = ulong.Parse(lobby.GetData("Player" + i));

					if (steamId == 0)
					{
						lobby.SetData("Player" + i, packet.join.steamId.ToString());

						packet.id = i;

						if (Fading) { packet.join.level = (byte)(levelNum + 100); }
						else if (Loading) { packet.join.level = (byte)(levelNum + 200); }
						else if (!(Loading || InLobby)) { packet.join.level = levelNum; }
						else { packet.join.level = 255; }

						NetworkManager.Instance.SendMessage(packet);
						AddPlayer(i);

						break;
					}
				}
			}
		}
		else if (packet.id != 255 && !IsServer)
		{
			if (ThisPlayer == 255)
			{
				if (packet.join.steamId == NetworkManager.Instance.PlayerId.Value)
				{
					ThisPlayer = packet.id;
					OnJoinGame(packet.join.level);
				}
			}
			else
			{
				++PlayerCount;
				AddPlayer(packet.id);
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
				if (entities[i] != null)
				{
					Destroy(entities[i].gameObject);
					entities[i] = null;
				}

				--PlayerCount;
				//TODO: decrement AlivePlayers

				lobby.SetData("Player" + i, "0");

				break;
			}
		}
	}

	public void ReceiveTransform(Packet transform)
	{
		if (!Loading) { entities[transform.id].SetTransform(transform.transform); }
	}

	public void Action(Packet action)
	{
		if (action.action.data == 255) //Loaded into level
		{
			if (++loadedPlayers == PlayerCount) { FinishLoading(); }
		}
		else { entities[action.id].DoAction(action.action); }
	}

	public void Health(Packet health)
	{
		Health h = entities[health.id].GetComponent<Health>();
		h.health = health.health.health;
		h.trauma = health.health.trauma;
		h.down = health.health.down;
	}

	public void Inventory(Packet inventory)
	{

	}

	public void GameTrigger(Packet packet)
	{

	}

	public void LoadLevel(Packet packet)
	{
		for (ushort i = 0; i < 4; ++i) { if (entities[i] != null) { unspawnedPlayers.Enqueue(i); } }

		Fading = true;
		StartCoroutine(SceneLoader.FadeToLoad(3.0f, packet.id, StartLoad));
	}

	public void StartLoad(int i)
	{
		Loading = true;
		Fading = false;
		InLobby = false;
		loadedPlayers = 0;

		foreach (GameObject healthBar in healthBars)
		{
			Destroy(healthBar);
		}

		healthBars.Clear();

		SceneLoader.SetLoadingScreen(true);
		SceneLoader.ResetScreen();
		playerLocations.Clear();

		string scene;
		switch (i)
		{
			default:
			case 0: scene = "c1m1_NapRoom"; break;
			case 1: scene = "c1m2_Library"; break;
			case 2: scene = "c1m3_Playground"; break;
			case 3: scene = "c1m4_Cellar"; break;
			case 4: scene = "c1m5_Corruption"; break;
		}

		SceneLoader.LoadScene(scene);
	}

	public void Spawn(Packet packet)
	{
		if (packet.id < 4)
		{

		}
		else if (packet.id < 44)
		{
			Transform transform = level.GetEnemySpawn(packet.spawn.spawn);
			if (IsServer) { entities[packet.id] = Instantiate(prefabManager.Enemy, transform.position, transform.rotation).GetComponent<Entity>(); }
			else { entities[packet.id] = Instantiate(prefabManager.NetworkEnemy, transform.position, transform.rotation).GetComponent<Entity>(); }
			entities[packet.id].id = packet.id;
			entities[packet.id].SetModel();
		}
		else if (packet.id < 10001)
		{
			InteractableSpawner spawner = level.GetInteractableSpawn(packet.spawn.spawn);

			if (packet.spawn.type < 100)
			{
				entities[packet.id] = Instantiate(prefabManager.Pickups[packet.spawn.type - 1], spawner.transform.position, spawner.transform.rotation).GetComponentInChildren<Entity>();
				entities[packet.id].id = packet.id;
				Interactable i = entities[packet.id].GetComponent<Interactable>();
				i.SetEvents(spawner.onInteract, spawner.onStopInteract, spawner.onComplete);
				i.id = spawner.id;
			}
			else if (packet.spawn.type < 255)
			{
				entities[packet.id] = Instantiate(prefabManager.Pushables[packet.spawn.type - 100], spawner.transform.position, spawner.transform.rotation).GetComponentInChildren<Entity>();
				entities[packet.id].id = packet.id;
				Interactable i = entities[packet.id].GetComponent<Interactable>();
				i.SetEvents(spawner.onInteract, spawner.onStopInteract, spawner.onComplete);
				i.id = spawner.id;
			}
		}
		else
		{
			if (IsServer && packet.id == INVALID_ID)
			{
				ushort id = projectileIndices.Pop();

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
			else if (packet.id != INVALID_ID)
			{
				Entity entity = entities[packet.spawn.spawn];

				entities[packet.id] = Instantiate(prefabManager.NetworkProjectile, entity.shoot.position, entity.shoot.rotation).GetComponent<Entity>();
				entities[packet.id].id = packet.id;
			}
		}
	}

	public void Despawn(Packet packet)
	{
		if (entities[packet.id] != null)
		{
			Destroy(entities[packet.id].gameObject);
		}
	}

	public void OwnerChange(Packet packet)
	{
		if (packet.owner.steamId == NetworkManager.Instance.PlayerId.Value)
		{
			IsServer = true;
		}
	}
}