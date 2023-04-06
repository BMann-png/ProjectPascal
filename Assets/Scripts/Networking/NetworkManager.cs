using Steamworks;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.SceneManagement;

//TODO: Set owner
//TODO: Invite Friends
//TODO: Public vs private lobbies

public class NetworkManager : Singleton<NetworkManager>
{
	public string PlayerName { get; private set; }
	public SteamId PlayerId { get; private set; }

	private string playerIdString;
	private bool connectedToSteam;

	private Pascal.SocketManager socketManager;
	private Pascal.ConnectionManager connectionManager;
	private bool activeSocketServer;
	private bool activeSocketConnection;

	private List<Lobby> activeLobbies;
	public Lobby currentLobby;
	private Lobby hostedLobby;

	protected override void Awake()
	{
		try
		{
			SteamClient.Init(480, true);
			if (!SteamClient.IsValid)
			{
				throw new System.Exception("Client is not valid");
			}

			PlayerName = SteamClient.Name;
			PlayerId = SteamClient.SteamId;
			playerIdString = PlayerId.ToString();
			activeLobbies = new List<Lobby>();
			connectedToSteam = true;

			SteamNetworkingUtils.InitRelayNetworkAccess();

			Debug.Log("Steam initialized: " + PlayerName);
		}
		catch (System.Exception e)
		{
			Debug.Log(e.Message);
		}
	}

	void Start()
	{
		//SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreatedCallback;
		//SteamMatchmaking.OnLobbyCreated += OnLobbyCreatedCallback;
		//SteamMatchmaking.OnLobbyEntered += OnLobbyEnteredCallback;
		//SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoinedCallback;
		//SteamMatchmaking.OnChatMessage += OnChatMessageCallback;
		//SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnectedCallback;
		//SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeaveCallback;
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

	//private void OnLobbyMemberLeaveCallback(Lobby arg1, Friend arg2)
	//{
	//    throw new NotImplementedException();
	//}

	//private void OnLobbyMemberDisconnectedCallback(Lobby arg1, Friend arg2)
	//{
	//    throw new NotImplementedException();
	//}

	//private void OnChatMessageCallback(Lobby arg1, Friend arg2, string arg3)
	//{
	//    throw new NotImplementedException();
	//}

	//private void OnLobbyMemberJoinedCallback(Lobby arg1, Friend arg2)
	//{
	//    throw new NotImplementedException();
	//}

	//private void OnLobbyEnteredCallback(Lobby obj)
	//{
	//    throw new NotImplementedException();
	//}

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
			//hostedLobby.SetPrivate();
			hostedLobby.SetJoinable(true);

			foreach (var item in data)
			{
				hostedLobby.SetData(item.Key, item.Value);
			}

			currentLobby = hostedLobby;

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

	public void CreateSocketServer()
	{
		socketManager = SteamNetworkingSockets.CreateRelaySocket<Pascal.SocketManager>(0);
		connectionManager = SteamNetworkingSockets.ConnectRelay<Pascal.ConnectionManager>(PlayerId);
		activeSocketServer = true;
		activeSocketConnection = true;
	}

	private void JoinSocketServer()
	{
		connectionManager = SteamNetworkingSockets.ConnectRelay<Pascal.ConnectionManager>(PlayerId);
		activeSocketServer = false;
		activeSocketConnection = true;
	}

	private void LeaveSocketServer()
	{
		activeSocketServer = false;
		activeSocketConnection = false;

		try
		{
			connectionManager.Close();
			socketManager.Close();
		}
		catch
		{
			Debug.Log("Error closing managers");
		}
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

	public bool SendMessage(byte[] message)
	{
		try
		{
			int messageSize = message.Length;
			IntPtr intPtrMessage = System.Runtime.InteropServices.Marshal.AllocHGlobal(messageSize);
			System.Runtime.InteropServices.Marshal.Copy(message, 0, intPtrMessage, messageSize);

			for (int k = 0; k < 5; k++)
			{
				Result result = connectionManager.Connection.SendMessage(intPtrMessage, messageSize, SendType.Reliable);

				if (result == Result.OK)
				{
					System.Runtime.InteropServices.Marshal.FreeHGlobal(intPtrMessage);
					return true;
				}
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

	public void ProcessMessage(IntPtr messageIntPtr, int dataBlockSize)
	{
		try
		{
			byte[] message = new byte[dataBlockSize];
			System.Runtime.InteropServices.Marshal.Copy(messageIntPtr, message, 0, dataBlockSize);
			string messageString = System.Text.Encoding.UTF8.GetString(message);

			// Do something with received message

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
		LeaveSocketServer();
	}
}
