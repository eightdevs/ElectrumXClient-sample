﻿namespace ElectrumX.Request
{
    internal class ServerPingRequest : RequestBase<string>
    {
        internal ServerPingRequest()
        {
            Method = "server.ping";
            Parameters = new string[] { };
        }
    }
}
