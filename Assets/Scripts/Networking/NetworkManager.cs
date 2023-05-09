using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

//TODO: Set owner
//TODO: Invite Friends
//TODO: Public vs private lobbies

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TransformPacket
{
	public TransformPacket(Transform t, float x)
	{
		xPos = t.position.x;
		yPos = t.position.y;
		zPos = t.position.z;

		xRot = x;
		yRot = t.eulerAngles.y;
	}

	public float xPos;
	public float yPos;
	public float zPos;
	public float xRot;
	public float yRot;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ActionPacket
{
	public ActionPacket(byte data)
	{
		this.data = data;
	}

	public byte data;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct HealthPacket
{
	public HealthPacket(byte health, byte trauma, byte down)
	{
		this.health = health;
		this.trauma = trauma;
		this.down = down;
	}

	public byte health;
	public byte trauma;
	public byte down;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct InventoryPacket
{
	public InventoryPacket(byte slot, byte data)
	{
		this.slot = slot;
		this.data = data;
	}

	public byte slot;
	public byte data;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpawnPacket
{
	public SpawnPacket(ushort spawn, byte type = 255)
	{
		this.spawn = spawn;
		this.type = type;
	}

	public ushort spawn;
	public byte type;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct OwnerPacket
{
	public OwnerPacket(ulong steamId)
	{
		this.steamId = steamId;
	}

	public ulong steamId;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct JoinPacket
{
	public JoinPacket(ulong steamId)
	{
		this.steamId = steamId;
		level = 255;
	}

	public ulong steamId;
	public byte level;
}

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct Packet
{
	[FieldOffset(0)] public byte type;
	[FieldOffset(1)] public ushort id;

	[FieldOffset(3)] public TransformPacket transform;
	[FieldOffset(3)] public ActionPacket action;
	[FieldOffset(3)] public HealthPacket health;
	[FieldOffset(3)] public InventoryPacket inventory;
	[FieldOffset(3)] public SpawnPacket spawn;
	[FieldOffset(3)] public OwnerPacket owner;
	[FieldOffset(3)] public JoinPacket join;
}

public class NetworkManager : Singleton<NetworkManager>
{
	public string PlayerName { get; private set; }
	public SteamId PlayerId { get; private set; }

	private Pascal.SocketManager socketManager;
	private Pascal.ConnectionManager connectionManager;
	public bool activeSocketServer { get; private set; }
	private bool activeSocketConnection;

	private List<Lobby> activeLobbies;
	public Lobby currentLobby;
	private Lobby hostedLobby;

	private IntPtr message;
	private bool cleanedUp = false;

	protected override void Awake()
	{
		base.Awake();

		try
		{
			SteamClient.Init(480, true);
			if (!SteamClient.IsValid)
			{
				throw new Exception("Client is not valid");
			}

			message = Marshal.AllocHGlobal(22);

			PlayerName = SteamClient.Name;
			PlayerId = SteamClient.SteamId;
			activeLobbies = new List<Lobby>();

			SteamNetworkingUtils.InitRelayNetworkAccess();

			Debug.Log("Steam initialized: " + PlayerName);
		}
		catch (Exception e)
		{
			Debug.Log(e.Message);
		}
	}

	private void Start()
	{
		SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeaveCallback;
		//SteamMatchmaking.OnLobbyEntered += OnLobbyEnteredCallback;
		SteamNetworkingSockets.OnConnectionStatusChanged += OnConnectionStatusChanged;
	}

	private void OnLobbyMemberLeaveCallback(Lobby lobby, Friend friend)
	{
		if (friend.IsMe) { return; } //ignore yourself leaving
		GameManager.Instance.PlayerLeft(friend);
	}

	private void OnLobbyEnteredCallback(Lobby obj)
	{
		
	}

	private void OnConnectionStatusChanged(Connection connection, ConnectionInfo info)
	{
		if(info.State == ConnectionState.Connected)
		{
			Packet packet = new Packet();
			packet.type = 9;
			packet.id = GameManager.INVALID_ID;
			packet.join = new JoinPacket(PlayerId);

			Instance.SendMessage(packet);
		}
	}

	void Update()
	{
		//SteamClient.RunCallbacks();
		try
		{
			if (activeSocketServer)
			{
				socketManager.Receive();
			}
			if (activeSocketConnection)
			{
				connectionManager.Receive();
			}
		}
		catch
		{
			Debug.Log("Error receiving data");
		}
	}

	public async Task<bool> CreateLobby(Dictionary<string, string> data, int maxPlayers)
	{
		try
		{
			var createLobbyOutput = await SteamMatchmaking.CreateLobbyAsync(maxPlayers);
			if (!createLobbyOutput.HasValue)
			{
				Debug.Log("Lobby created but not correctly instantiated");
				throw new Exception();
			}

			hostedLobby = createLobbyOutput.Value;
			hostedLobby.SetPublic();
			hostedLobby.SetJoinable(true);

			foreach (var item in data)
			{
				hostedLobby.SetData(item.Key, item.Value);
			}

			currentLobby = hostedLobby;

			CreateSocketServer();

			return true;
		}
		catch (Exception e)
		{
			Debug.Log("Failed to created lobby");
			Debug.Log(e.ToString());

			return false;
		}
	}

	public async Task<List<Lobby>> GetLobbies()
	{
		try
		{
			activeLobbies.Clear();
			activeLobbies = (await SteamMatchmaking.LobbyList.FilterDistanceFar().WithSlotsAvailable(1).WithMaxResults(100).RequestAsync()).ToList();
		}
		catch (Exception e)
		{
			Debug.Log("Error fetching multiplayer lobbies");
			Debug.Log(e.ToString());
		}

		return activeLobbies;
	}

	public void JoinLobby(Lobby lobby)
	{
		currentLobby = lobby;
		currentLobby.Join();
		JoinSocketServer(lobby);
	}

	public void LeaveLobby()
	{
		currentLobby.Leave();
		LeaveSocketServer();
	}

	public void CreateSocketServer()
	{
		socketManager = SteamNetworkingSockets.CreateRelaySocket<Pascal.SocketManager>(0);
		connectionManager = SteamNetworkingSockets.ConnectRelay<Pascal.ConnectionManager>(PlayerId);
		activeSocketServer = true;
		activeSocketConnection = true;
	}

	private void JoinSocketServer(Lobby lobby)
	{
		connectionManager = SteamNetworkingSockets.ConnectRelay<Pascal.ConnectionManager>(lobby.Owner.Id);
		activeSocketServer = false;
		activeSocketConnection = true;
	}

	private void LeaveSocketServer()
	{
		if (activeSocketConnection) { connectionManager.Close(); activeSocketConnection = false; }
		if (activeSocketServer) { socketManager.Close(); activeSocketServer = false; }
	}

	public void RelayMessageReceived(IntPtr message, int size, uint connectionId)
	{
		try
		{
			for (int i = 0; i < socketManager.Connected.Count; i++)
			{
				if (socketManager.Connected[i] != connectionId)
				{
					for (int k = 0; k < 5; k++)
					{
						Result result = socketManager.Connected[i].SendMessage(message, size);

						if (result == Result.OK)
						{
							break;
						}
					}
				}
			}
		}
		catch
		{
			Debug.Log("unable to relay server message");
		}
	}

	public bool SendMessage(Packet packet)
	{
		int size;
		switch (packet.type)
		{
			case 0: size = 23; break;   //Transform
			case 1: size = 4; break;    //Action
			case 2: size = 6; break;    //Health
			case 3: size = 6; break;    //Inventory
			case 4: size = 3; break;    //Game Trigger
			case 5: size = 3; break;    //Scene Load
			case 6: size = 6; break;    //Game Spawn
			case 7: size = 3; break;    //Game Despawn
			case 8: size = 11; break;   //Owner Change
			case 9: size = 12; break;   //Player Joined
			default: return false;
		}

		try
		{
			Marshal.StructureToPtr(packet, message, true);

			for (int k = 0; k < 5; k++)
			{
				Result result = connectionManager.Connection.SendMessage(message, size, SendType.Reliable);

				if (result == Result.OK) { return true; }
			}

			return false;
		}
		catch (Exception e)
		{
			Debug.Log(e.Message);
			Debug.Log("Unable to send message");
			return false;
		}
	}

	public void ProcessMessage(IntPtr message, int dataBlockSize)
	{
		try
		{
			Packet packet = Marshal.PtrToStructure<Packet>(message);

			switch (packet.type)
			{
				case 0: GameManager.Instance.ReceiveTransform(packet); break;	//Transform
				case 1: GameManager.Instance.Action(packet); break;				//Action
				case 2: GameManager.Instance.Health(packet); break;				//Health
				case 3: GameManager.Instance.Inventory(packet); break;			//Inventory
				case 4: GameManager.Instance.GameTrigger(packet); break;		//Game Trigger
				case 5: GameManager.Instance.LoadLevel(packet); break;			//Scene Load
				case 6: GameManager.Instance.Spawn(packet); break;				//Game Spawn
				case 7: GameManager.Instance.Despawn(packet); break;			//Game Despawn
				case 8: GameManager.Instance.OwnerChange(packet); break;		//Owner Change
				case 9: GameManager.Instance.PlayerJoined(packet); break;		//Player Joined
				default: break;
			}
		}
		catch
		{
			Debug.Log("Unable to process message from socket server");
		}
	}

	private void OnDisable()
	{
		GameClose();
	}

	private void OnDestroy()
	{
		GameClose();
	}

	private void OnApplicationQuit()
	{
		GameClose();
	}

	private void GameClose()
	{
		if (!cleanedUp)
		{
			cleanedUp = true;
			LeaveLobby();
			LeaveSocketServer();

			Marshal.FreeHGlobal(message);
		}
	}
}
