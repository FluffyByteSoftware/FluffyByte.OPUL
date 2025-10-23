using System.Net;
using System.Net.Sockets;
using System.Text;
using FluffyByte.OPUL.Core.FluffyIO.FluffyConsole;

namespace FluffyByte.OPUL.Core.FluffyIO.Networking.FluffyClients;

public class FluffyRawClient : IFluffyClient
{
    private static int _id = 0;
    public int Id { get; private set; }
    
    public string Name { get; private set; } = "RawClient";
    public Guid Guid { get; private set; }
    
    public string Address { get; private set; } = "0.0.0.1";
    public IPAddress IPAddress { get; private set; }
    public string HostName { get; private set; } = "unknown.com.unk";
    public bool IsConnected { get; private set; } = false;

    public int LastPing { get; private set; }
    public double AveragePing { get; private set; }

    public DateTime LastResponseTime { get; private set; }
    public DateTime ConnectedAtTime { get; private set; }
    public DateTime LastActivityTime { get; private set; }

    public string DisconnectReason { get; private set; } = "no reason";

    private bool _disconnecting = false;

    private readonly CancellationToken _shutdownToken = new();

    private readonly TcpClient _tcpClient;

    private readonly Sentinel _sentinelRef;
    private readonly NetworkStream _stream;
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;

    public FluffyRawClient(TcpClient tcpClient, Sentinel sentinel, CancellationToken ct)
    {
        _id++;
        Id = _id;
        Guid = new();

        IPAddress = IPAddress.Parse(Address);
        IsConnected = true;

        DateTime now = DateTime.Now;

        ConnectedAtTime = now;
        LastActivityTime = now;
        LastResponseTime = now;

        LastPing = 0;
        AveragePing = 0;

        _sentinelRef = sentinel;

        _tcpClient = tcpClient;
        _stream = tcpClient.GetStream();
        _reader = new(_stream, encoding: Encoding.UTF8);
        _writer = new(_stream, encoding: Encoding.UTF8) { AutoFlush = true };

        _shutdownToken = ct;
    }

    public void SetName(string name) { Name = name; }

    public async Task WriteLineAsync(string message, bool omitNewLine = false)
    {
        if (_disconnecting)
            return;


        try
        {
            await _writer.WriteLineAsync(message.ToCharArray(), _shutdownToken);
        }
        catch (IOException)
        {
            Scribe.Debug($"The client {Name} should be disconnecting now. An IOException has occurred, this is normal.");
        }
        catch(Exception ex)
        {
            Scribe.Error("Error in WriteLineAsync()", ex);
        }

    }

    public async Task<string> ReadLineAsync()
    {
        string? response = null;

        try
        {
            response = await _reader.ReadLineAsync(_shutdownToken);
        }
        catch (IOException)
        {
            Scribe.Debug($"{Name} is probably disconnecting.");
        }
        catch(Exception ex) when (ex is not IOException)
        {
            Scribe.Error("Error in ReadLineAsync()", ex);
        }

        return response ?? string.Empty;
    }

    public async Task<bool> TestConnectionAsync()
    {
        await Task.CompletedTask;

        return true;
    }

    public async Task DisconnectAsync()
    {
        if (_disconnecting)
            return;

        _disconnecting = true;

        HandleDisconnect();

        await Task.CompletedTask;
    }

    public void Disconnect()
    {
        if(_disconnecting)
        {
            return;
        }

        _disconnecting = true;

        HandleDisconnect();
    }


    private void HandleDisconnect()
    {
        if(_disconnecting)
        {
            return;
        }

        _tcpClient.Close();
        _tcpClient.Dispose();
    }
}
