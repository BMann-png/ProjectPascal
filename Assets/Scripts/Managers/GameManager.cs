using Steamworks;
using Steamworks.Data;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : Singleton<GameManager>
{
	private static readonly object joinLock = new object();

	private static readonly int MAX_ENEMY_COUNT = 30;
	private static readonly int MAX_SPECIAL_COUNT = 2;
	public static readonly ushort INVALID_ID = 65535;

	public List<GameObject> playerLocations = new List<GameObject>();
	private List<GameObject> spectators = new List<GameObject>();
	private int spectateID = 0;
	private bool spectating = false;

	public bool IsServer { get; private set; }
	public bool InLobby { get; private set; }
	public bool InGame { get; private set; }
	private byte levelNum;

	private List<ushort> unspawnedPlayers = new List<ushort>();
	private Transform healthBarHolder;
	private List<GameObject> healthBars = new List<GameObject>();
	private Entity[] entities = new Entity[65536];
	public Entity[] Entities { get { return entities; } }
	private Transform[] lobbySpawnpoints;
	private Queue<ushort> enemyIndices = new Queue<ushort>(97);
	private Queue<ushort> interactableIndices = new Queue<ushort>(9890);
	private Queue<ushort> projectileIndices = new Queue<ushort>(40000);
	private bool[] specialsSpawned = new bool[10];
	private int enemyCount = 0;
	private int specialCount = 0;
	private float spawnTimer = 0.0f;

	private LevelManager level;
	private LobbyHandler lobby;

	private PrefabManager prefabManager;
    public PrefabManager PrefabManager { get => prefabManager; }

    private AudioManager audioManager;
	public AudioManager AudioManager { get => audioManager; }

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
		audioManager = FindFirstObjectByType<AudioManager>();
		sceneLoader = FindFirstObjectByType<SceneLoader>();

		for (ushort i = 4; i < 101; ++i) { enemyIndices.Enqueue(i); }
		for (ushort i = 111; i < 10001; ++i) { interactableIndices.Enqueue(i); }
		for (ushort i = 10001; i < INVALID_ID; ++i) { projectileIndices.Enqueue(i); }
	}

	private void Update()
	{
		spawnTimer -= Time.deltaTime;

		if (spawnTimer <= 0.0f && IsServer && !InLobby && AlivePlayers > 0)
		{
			if (enemyCount < MAX_ENEMY_COUNT)
			{
				ushort id = enemyIndices.Dequeue();
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
			else
			{
				spawnTimer = 5.0f;
			}

			if (specialCount < MAX_SPECIAL_COUNT)
			{
				ushort id;
				while (true)
				{
					id = (ushort)Random.Range(0, 3); //TODO: Widen range to later specials

					if (specialsSpawned[id] == false) { break; }
				}

				specialsSpawned[id] = true;
				id += 100;
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

		if(spectating)
		{
			if(Input.GetKeyDown(KeyCode.Mouse0))
			{
				if(++spectateID == spectators.Count) { spectateID = 0; }
			}
			else if(Input.GetKeyDown(KeyCode.Mouse1))
			{
				if(--spectateID == -1) { spectateID = spectators.Count - 1; }
			}
		}
	}

	public void SelectLevel(TMP_Dropdown change)
	{
		if (IsServer && !Loading)
		{
			levelNum = (byte)change.value;

			Packet packet = new Packet();
			packet.type = 5;
			packet.id = (byte)(levelNum + 100);

			NetworkManager.Instance.SendMessage(packet);
		}
	}

	public void StartGame()
	{
		if (IsServer && !Loading && !Fading)
		{
			Packet packet = new Packet();
			packet.type = 5;
			packet.id = levelNum;

			NetworkManager.Instance.SendMessage(packet);
			LoadLevel(levelNum);
		}
	}

	public void ChangeLevel(byte level)
	{
		if (IsServer && !Fading)
		{
			Packet packet = new Packet();
			packet.type = 5;
			packet.id = level;

			NetworkManager.Instance.SendMessage(packet);
			LoadLevel(level);
		}
	}

	public void OnLevelLoad(LevelManager level)
	{
		this.level = level;
		InLobby = false;
		AlivePlayers = 0;

		if (++loadedPlayers == PlayerCount) { FinishLoading(); }

		Packet packet = new Packet();
		packet.type = 1;
		packet.id = ThisPlayer;
		packet.action = new ActionPacket(255);

		NetworkManager.Instance.SendMessage(packet);
	}

	private void FinishLoading()
	{
		spectating = false;
		spectateID = 0;
		loadedPlayers = 0;
		InGame = true;

		if (IsServer)
		{
			for (ushort i = 0; i < level.InteractableSpawnCount(); ++i)
			{
				InteractableSpawner spawner = level.GetInteractableSpawn(i);

				if (spawner.guaranteeSpawn)
				{
					ushort id = interactableIndices.Dequeue();

					if (spawner.type == 255) { spawner.type = (byte)Random.Range(0, 5); }

					if (spawner.type < 100)
					{
						entities[id] = Instantiate(prefabManager.Pickups[spawner.type], spawner.transform.position, spawner.transform.rotation).GetComponentInChildren<Entity>();
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
		foreach(ushort id in unspawnedPlayers)
		{
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

				spectators.Add(entities[id].GetComponentInChildren<CameraTarget>().gameObject);

				++healthBarId;
			}

			++AlivePlayers;
		}

		unspawnedPlayers.Clear();

		foreach(GameObject go in spectators)
		{
			go.SetActive(false);
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
		lobby = FindFirstObjectByType<LobbyHandler>();
		lobbySpawnpoints = spawnPoints;

		if (IsServer)
		{
			InLobby = true;
			entities[0] = Instantiate(prefabManager.LobbyPlayer, spawnPoints[0].position, spawnPoints[0].rotation).GetComponent<Entity>();
			entities[0].id = 0;
			entities[0].destroyed = true;
			entities[0].GetComponent<LobbyPlayer>().name.text = NetworkManager.Instance.PlayerName;
			entities[0].SetModel();

			ThisPlayer = 0;

			NetworkManager.Instance.currentLobby.SetData("DaycareDescent", "true");
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

		if (data > 199)
		{
			InLobby = true;
			this.lobby = FindFirstObjectByType<LobbyHandler>();
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
					entities[i].destroyed = true;
					entities[i].GetComponent<LobbyPlayer>().name.text = steamName;
					entities[i].SetModel();
				}
				else if (i == ThisPlayer)
				{
					entities[i] = Instantiate(prefabManager.LobbyPlayer, lobbySpawnpoints[i].position, lobbySpawnpoints[i].rotation).GetComponent<Entity>();
					entities[i].id = i;
					entities[i].destroyed = true;
					entities[i].GetComponent<LobbyPlayer>().name.text = NetworkManager.Instance.PlayerName;
					entities[i].SetModel();
				}
			}

			FindFirstObjectByType<LobbyHandler>().SelectLevel((byte)(data - 200));
		}
		else if (data > 99)
		{
			FindFirstObjectByType<LobbyHandler>().SelectLevel((byte)(data - 100));
			StartLoad(data - 100);
		}
		else if (data > 49)
		{
			InLobby = true;
			this.lobby = FindFirstObjectByType<LobbyHandler>();
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
					entities[i].destroyed = true;
					entities[i].GetComponent<LobbyPlayer>().name.text = steamName;
					entities[i].SetModel();
				}
				else if (i == ThisPlayer)
				{
					entities[i] = Instantiate(prefabManager.LobbyPlayer, lobbySpawnpoints[i].position, lobbySpawnpoints[i].rotation).GetComponent<Entity>();
					entities[i].id = i;
					entities[i].destroyed = true;
					entities[i].GetComponent<LobbyPlayer>().name.text = NetworkManager.Instance.PlayerName;
					entities[i].SetModel();
				}
			}

			FindFirstObjectByType<LobbyHandler>().SelectLevel((byte)(data - 50));
			LoadLevel((byte)(data - 50));
		}
		else
		{
			//TODO: spawn players

			spectateID = 0;
			Spectate();
		}
	}

	public void AddPlayer(ushort id, ulong steamId)
	{
		Transform transform;
		if (InLobby)
		{
			transform = lobbySpawnpoints[id];
			entities[id] = Instantiate(prefabManager.LobbyPlayer, transform.position, transform.rotation).GetComponent<Entity>();
			entities[id].id = id;
			entities[id].destroyed = true;
			entities[id].SetModel();

			Lobby lobby = NetworkManager.Instance.currentLobby;
			IEnumerable<Friend> members = lobby.Members;

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

			if (Fading) { unspawnedPlayers.Add(id); }
		}
		else
		{
			unspawnedPlayers.Add(id);
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
					//TODO: new owner
					//NetworkManager.Instance.currentLobby.Owner = Friend

					Packet packet = new Packet();
					packet.type = 7;
					packet.owner = new OwnerPacket(steamId);

					NetworkManager.Instance.SendMessage(packet);
				}
			}
		}

		if(InGame)
		{
			InGame = false;

			loadedPlayers = 0;
			spectators.Clear();
			unspawnedPlayers.Clear();
			playerLocations.Clear();

			foreach (GameObject healthBar in healthBars)
			{
				Destroy(healthBar);
			}

			healthBars.Clear();
		}	

		PlayerCount = 0;
		ThisPlayer = INVALID_ID;
		IsServer = false;
		InLobby = false;

		FindFirstObjectByType<HUDManager>().HidePauseMenu();
	}

	public void PickupItem(byte id)
	{
		byte slot = 0;
		byte type = 0;

		switch(id)
		{
			case 0: slot = 0; type = 0; break;
			case 1: slot = 0; type = 1; break;
			case 2: slot = 1; type = 0; break;
			case 3: slot = 1; type = 1; break;
			case 4: slot = 2; type = 1; break;
			case 5: slot = 3; type = 1; break;
			default: break;
		}

		//0, 0 - dart gun
		//0, 1 - bubble gun
		//1, 0 - squirt gun
		//1, 1 - ball gun
		//2, 1 - pacifier
		//2, 1 - mission

		entities[ThisPlayer].GetComponent<Inventory>().SetWeapon(slot, type);

		Packet packet = new Packet();
		packet.type = 3;
		packet.id = ThisPlayer;
		packet.inventory = new InventoryPacket(slot, type);

		NetworkManager.Instance.SendMessage(packet);
	}

	public void PushPlayer(Vector3 dir)
	{
		entities[ThisPlayer].GetComponent<CharacterController>().Move(dir);
	}

	public void Shoot(Transform shoot, byte type, Entity owner)
	{
		if (IsServer)
		{
			ushort id = projectileIndices.Dequeue();

			Vector2 variation = Vector2.zero;
			
			switch (type)
			{
				case 0: break;
				case 1:
					variation.x = Random.Range(-5f, 5f);
					variation.y = Random.Range(-5f, 5f);
					break;
				case 2:
					variation.x = Random.Range(-3.5f, 3.5f);
					variation.y = Random.Range(-3.5f, 3.5f);
					break;
			}

			Quaternion rotation = shoot.rotation * Quaternion.Euler(variation.x, variation.y, 0.0f);

			GameObject go = Instantiate(prefabManager.Projectiles[type], shoot.position, rotation);

			go.GetComponent<Damage>().Owner = owner.gameObject;

            entities[id] = go.GetComponent<Entity>();
			entities[id].id = id;
			entities[id].GetComponent<Projectile>().SetSpeed();

			Packet packet = new Packet();
			packet.id = id;
			packet.type = 6;
			packet.spawn = new SpawnPacket(ThisPlayer, type);

			NetworkManager.Instance.SendMessage(packet);
		}
		else
		{
			Packet packet = new Packet();
			packet.id = INVALID_ID;
			packet.type = 6;
			packet.spawn = new SpawnPacket(ThisPlayer, type);

			NetworkManager.Instance.SendMessage(packet);
		}
	}

	public void Destroy(Entity obj)
	{
		if (obj.id < 4) //Player
		{
			--AlivePlayers;
			unspawnedPlayers.Add(obj.id);
		}
		else if (obj.id < 101) //Common Enemy
		{
			--enemyCount;
			enemyIndices.Enqueue(obj.id);
		}
		else if (obj.id < 111) //Special Enemy
		{
			--specialCount;
			specialsSpawned[obj.id - 100] = false;
		}
		else if (obj.id < 10001)
		{
			interactableIndices.Enqueue(obj.id);
		}
		else if (obj.id < INVALID_ID)
		{
			projectileIndices.Enqueue(obj.id);
		}

		Packet packet = new Packet();
		packet.id = obj.id;
		packet.type = 7;

		NetworkManager.Instance.SendMessage(packet);
	}

	public void Spectate()
	{
		Camera.main.gameObject.SetActive(false);
		spectators[spectateID].SetActive(true);
		spectating = true;
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

						if (Fading) { packet.join.level = (byte)(levelNum + 50); }
						else if (Loading) { packet.join.level = (byte)(levelNum + 100); }
						else if (!(Loading || InLobby)) { packet.join.level = levelNum; }
						else { packet.join.level = (byte)(levelNum + 200); }

						NetworkManager.Instance.SendMessage(packet);
						AddPlayer(i, packet.join.steamId);

						break;
					}
				}
			}
		}
		else if (packet.id != INVALID_ID && !IsServer)
		{
			if (ThisPlayer == INVALID_ID)
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
				AddPlayer(packet.id, packet.join.steamId);
			}
		}
	}

	public void PlayerLeft(Friend player)
	{
		Lobby lobby = NetworkManager.Instance.currentLobby;

		if(player.Id == lobby.Id)
		{
			//Kick to menu
		}
		else
		{
			for (int i = 0; i < 4; ++i)
			{
				ulong steamId = ulong.Parse(lobby.GetData("Player" + i));

				if (steamId == player.Id.Value)
				{
					foreach(ushort id in unspawnedPlayers)
					{
						unspawnedPlayers.Remove(id);
					}

					if (entities[i] != null)
					{
						Destroy(entities[i].gameObject);
						entities[i] = null;
					}

					--AlivePlayers;

					lobby.SetData("Player" + i, "0");

					break;
				}
			}
		}
	}

	public void ReceiveTransform(Packet transform)
	{
		if (!Loading) { entities[transform.id].SetTransform(transform.transform); }
	}

	public void Action(Packet action)
	{
		if (action.action.data == 255 && action.id < 4) //Loaded into level
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
		entities[inventory.id].GetComponent<Inventory>().EquipWeapon(inventory.inventory.slot, inventory.inventory.data);
    }

	public void GameTrigger(Packet packet)
	{

	}

	public void LoadLevel(byte level)
	{
		if(level > 99)
		{
			FindFirstObjectByType<LobbyHandler>().SelectLevel((byte)(level - 100));
		}
		else
		{
			loadedPlayers = 0;
			spectators.Clear();
			for (ushort i = 0; i < 4; ++i) { if (entities[i] != null) { entities[i].destroyed = true; unspawnedPlayers.Add(i); } }

			Fading = true;
			lobby.DisableUI();
			StartCoroutine(SceneLoader.FadeToLoad(3.0f, level, StartLoad));
		}
	}

	public void StartLoad(int i)
	{
		Loading = true;
		Fading = false;
		InLobby = false;

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
		else if (packet.id < 111)
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
				entities[packet.id] = Instantiate(prefabManager.Pickups[packet.spawn.type], spawner.transform.position, spawner.transform.rotation).GetComponentInChildren<Entity>();
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
				ushort id = projectileIndices.Dequeue();

				Entity entity = entities[packet.spawn.spawn];

				Vector2 variation = Vector2.zero;

				switch (packet.spawn.type)
				{
					case 0: break;
					case 1:
						variation.x = Random.Range(-5f, 5f);
						variation.y = Random.Range(-5f, 5f);
						break;
					case 2:
						variation.x = Random.Range(-3.5f, 3.5f);
						variation.y = Random.Range(-3.5f, 3.5f);
						break;
				}

				Quaternion rotation = entity.shoot.rotation * Quaternion.Euler(variation.x, variation.y, 0.0f);

				entities[id] = Instantiate(prefabManager.Projectiles[packet.spawn.type], entity.shoot.position, rotation).GetComponent<Entity>();
				entities[id].id = id;
				entities[id].GetComponent<Damage>().Owner = entities[packet.spawn.spawn].gameObject;
				entities[id].GetComponent<Projectile>().SetSpeed();

				Packet newPacket = new Packet();
				newPacket.id = id;
				newPacket.type = 6;
				newPacket.spawn = packet.spawn;

				NetworkManager.Instance.SendMessage(newPacket);
			}
			else if (packet.id != INVALID_ID)
			{
				Entity entity = entities[packet.spawn.spawn];

				entities[packet.id] = Instantiate(prefabManager.NetworkProjectiles[packet.spawn.type], entity.shoot.position, entity.shoot.rotation).GetComponent<Entity>();
				entities[packet.id].id = packet.id;
			}
		}
	}

	public void RotateShoot(Packet packet)
	{
		entities[packet.id].weapon.eulerAngles = new Vector3(packet.rotation.xRot, packet.rotation.yRot); 
	}

	public void Despawn(Packet packet)
	{
		if (entities[packet.id] != null)
		{
			if(packet.id < 4)
			{
				--AlivePlayers;
				unspawnedPlayers.Add(packet.id);
				playerLocations.Remove(entities[packet.id].gameObject);
				//TODO: Better spectator system
				//Entity e = entities[packet.id];
				//Spectate s = e.GetComponentInChildren<Spectate>();
				//GameObject g = s.gameObject;
				//int index = spectators.IndexOf(g);
				//spectators.RemoveAt(index);
				//
				//if(spectating && index == spectateID)
				//{
				//	spectateID = 0;
				//	spectators[spectateID].SetActive(true);
				//}
			}

			entities[packet.id].destroyed = true;
			Destroy(entities[packet.id].gameObject);
		}
	}

	public void OwnerChange(Packet packet)
	{
		if (packet.owner.steamId == NetworkManager.Instance.PlayerId.Value)
		{
			IsServer = true;
			lobby.SetOwner();
		}
	}
}