﻿using System;

namespace Pascal
{
    public class ConnectionManager : Steamworks.ConnectionManager
    {
        public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            NetworkManager.Instance.ProcessMessage(data, size);
        }
    }
}