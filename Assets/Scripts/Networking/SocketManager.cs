using Steamworks.Data;
using System;

namespace Pascal
{
    public class SocketManager : Steamworks.SocketManager
    {
        public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            NetworkManager.Instance.RelayMessageReceived(data, size, connection.Id);
        }
    }
}