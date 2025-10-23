using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FluffyByte.OPUL.Core.FluffyIO.Networking.FluffyClients;

public interface IFluffyClient
{
    string Name { get; }
    int Id { get; }
    Guid Guid { get; }


    string Address { get; }
    IPAddress IPAddress { get; }
    string HostName { get; }
    bool IsConnected { get; }
    int LastPing { get; }
    double AveragePing { get; }

    void SetName(string name);

    Task<bool> TestConnectionAsync();
    

    DateTime LastResponseTime { get; }
    DateTime ConnectedAtTime { get; }
    DateTime LastActivityTime { get; }

    string? DisconnectReason { get; }
    Task DisconnectAsync();

    Task WriteLineAsync(string message, bool omitNewLine = false);
    Task<string> ReadLineAsync();
}
