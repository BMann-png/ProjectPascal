using Steamworks;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : Singleton<NetworkManager>
{
    public string PlayerName { get; private set; }
    public SteamId PlayerId { get; private set; }

    private string playerIdString;
    private List<Lobby> activeLobbies;
    private bool connectedToSteam;

    private Pascal.SocketManager socketManager;
    private Pascal.ConnectionManager connectionManager;
    private bool activeSocketServer;
    private bool activeSocketConnection;

    void Awake()
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
        SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreatedCallback;
        SteamMatchmaking.OnLobbyCreated += OnLobbyCreatedCallback;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEnteredCallback;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoinedCallback;
        SteamMatchmaking.OnChatMessage += OnChatMessageCallback;
        SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnectedCallback;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeaveCallback;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequestedCallback;
        SteamApps.OnDlcInstalled += OnDlcInstalledCallback;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public async Task<bool> CreateLobby(int lobbyParameters)
    {
        try
        {
            var createLobbyOutput = await SteamMatchmaking.CreateLobbyAsync(2);
            if (!createLobbyOutput.HasValue)
            {
                Debug.Log("Lobby created but not correctly instantiated");
                throw new Exception();
            }
            return true;
        }
        catch
        {

            return false;
        }
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        throw new NotImplementedException();
    }

    private void OnDlcInstalledCallback(AppId obj)
    {
        throw new NotImplementedException();
    }

    private void OnGameLobbyJoinRequestedCallback(Lobby arg1, SteamId arg2)
    {
        throw new NotImplementedException();
    }

    private void OnLobbyMemberLeaveCallback(Lobby arg1, Friend arg2)
    {
        throw new NotImplementedException();
    }

    private void OnLobbyMemberDisconnectedCallback(Lobby arg1, Friend arg2)
    {
        throw new NotImplementedException();
    }

    private void OnChatMessageCallback(Lobby arg1, Friend arg2, string arg3)
    {
        throw new NotImplementedException();
    }

    private void OnLobbyMemberJoinedCallback(Lobby arg1, Friend arg2)
    {
        throw new NotImplementedException();
    }

    private void OnLobbyEnteredCallback(Lobby obj)
    {
        throw new NotImplementedException();
    }

    private void OnLobbyCreatedCallback(Result arg1, Lobby arg2)
    {
        throw new NotImplementedException();
    }

    private void OnLobbyGameCreatedCallback(Lobby arg1, uint arg2, ushort arg3, SteamId arg4)
    {
        throw new NotImplementedException();
    }

    // Update is called once per frame
    void Update()
    {
        //SteamClient.RunCallbacks();
    }

    private void CreateSocketServer()
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
        //handle save and close
    }
}
