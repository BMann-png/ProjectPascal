using Steamworks;
using Steamworks.Data;
using System;
using System.Collections;
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
	public HealthPacket(byte data)
	{
		this.data = data;
	}

	public byte data;
	//FOR PLAYERS:
	//0 - sprint
	//1 - stop sprint
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
	public SpawnPacket(byte spawn)
	{
		this.spawn = spawn;
	}

	public byte spawn;
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
		unused = 255;
	}

	public ulong steamId;
	public byte level;
	public byte unused; 
}

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct Packet
{
	[FieldOffset(0)] public byte type;
	[FieldOffset(1)] public byte id;

	[FieldOffset(2)] public TransformPacket transform;
	[FieldOffset(2)] public ActionPacket action;
	[FieldOffset(2)] public HealthPacket health;
	[FieldOffset(2)] public InventoryPacket inventory;
	[FieldOffset(2)] public SpawnPacket spawn;
	[FieldOffset(2)] public OwnerPacket owner;
	[FieldOffset(2)] public JoinPacket join;
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
		//SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoinedCallback;
		//SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreatedCallback;
		//SteamMatchmaking.OnLobbyCreated += OnLobbyCreatedCallback;
		SteamMatchmaking.OnLobbyEntered += OnLobbyEnteredCallback;
		//SteamMatchmaking.OnChatMessage += OnChatMessageCallback;
		//SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnectedCallback;
		//SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequestedCallback;
		//SteamApps.OnDlcInstalled += OnDlcInstalledCallback;
		//SceneManager.sceneLoaded += OnSceneLoaded;
	}

	#region Obsolete


	//private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
	//{
	//    throw new NotImplementedException();
	//}

	//private void OnDlcInstalledCallback(AppId obj)
	//{
	//    throw new NotImplementedException();
	//}

	///// <summary>
	///// 
	///// </summary>
	///// <param name="lobby"></param>
	///// <param name="id"></param>
	//[Obsolete("This is deprecated, please use JoinSocketServer instead.")]
	//async private void OnGameLobbyJoinRequestedCallback(Lobby lobby, SteamId id)
	//{
	//    RoomEnter joinedLobby = await lobby.Join();

	//    if (joinedLobby == RoomEnter.Success)
	//    {
	//        currentLobby = lobby;
	//        //AcceptP2P(OpponentSteamId);
	//        SceneManager.LoadScene("Scene to load");
	//    }
	//    else
	//    {
	//        Debug.Log("failed to join lobby");
	//    }
	//}

	private void OnLobbyMemberLeaveCallback(Lobby lobby, Friend friend)
	{
		if (friend.IsMe) { return; } //ignore yourself leaving
		GameManager.Instance.PlayerLeft(friend);
	}

	//private void OnLobbyMemberDisconnectedCallback(Lobby lobby, Friend friend)
	//{
	//	if (friend.IsMe) { return; } //ignore yourself disconnected
	//	FindFirstObjectByType<LobbyHandler>().PlayerLeft(friend);
	//}

	//private void OnChatMessageCallback(Lobby arg1, Friend arg2, string arg3)
	//{
	//    throw new NotImplementedException();
	//}

	//private void OnLobbyMemberJoinedCallback(Lobby lobby, Friend friend)
	//{
	//	if (friend.IsMe) { return; } //ignore yourself joining
	//	GameManager.Instance.PlayerJoined(friend);
	//}

	private void OnLobbyEnteredCallback(Lobby obj)
	{
		Packet packet = new Packet();
		packet.type = 9;
		packet.join = new JoinPacket(PlayerId);

		SendMessage(packet);
	}

	//private void OnLobbyCreatedCallback(Result arg1, Lobby arg2)
	//{
	//    throw new NotImplementedException();
	//}

	//private void OnLobbyGameCreatedCallback(Lobby arg1, uint arg2, ushort arg3, SteamId arg4)
	//{
	//    throw new NotImplementedException();
	//}
	#endregion

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
			activeLobbies = (await SteamMatchmaking.LobbyList.WithSlotsAvailable(1).WithMaxResults(100).RequestAsync()).ToList();
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
			case 0: size = 22; break;   //Transform
			case 1: size = 3; break;    //Action
			case 2: size = 3; break;    //Health
			case 3: size = 5; break;    //Inventory
			case 4: size = 2; break;    //Game Trigger
			case 5: size = 2; break;    //Scene Load
			case 6: size = 3; break;    //Game Spawn
			case 7: size = 2; break;    //Game Despawn
			case 8: size = 10; break;   //Owner Change
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
