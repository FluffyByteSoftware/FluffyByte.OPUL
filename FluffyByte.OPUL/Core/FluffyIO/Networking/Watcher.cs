using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using FluffyByte.OPUL.Core.FluffyIO.FluffyConsole;
using FluffyByte.OPUL.Core.FluffyIO.Networking.NetClient;
using FluffyByte.OPUL.Tools.FluffyTypes;

namespace FluffyByte.OPUL.Core.FluffyIO.Networking;

/// <summary>
/// Manages and monitors connected Fluffy clients, providing thread-safe registration
/// and unregistration operations.
/// </summary>
/// <param name="sentinel">Reference to the Sentinel managing server operations</param>
public class Watcher(Sentinel sentinel)
{
    private readonly ThreadSafeList<FluffyClient> _clients = [];

    public readonly Sentinel SentinelReference = sentinel;

    /// <summary>
    /// Gets the number of currently connected clients.
    /// </summary>
    public int ConnectedClientCount => _clients.Count;

    /// <summary>
    /// Registers a newly connected client with the Watcher.
    /// </summary>
    /// <param name="client">The client to register</param>
    /// <returns>True if registration succeeded, false if client was null or already registered</returns>
    public bool RegisterClient(FluffyClient client)
    {
        if (client == null)
        {
            Scribe.Warning("[Watcher] Attempted to register null client");
            return false;
        }

        try
        {
            // Check if already registered to prevent duplicates
            if (_clients.Contains(client))
            {
                Scribe.Warning($"[Watcher] Client {client.Name} (ID: {client.Id}) is already registered");
                return false;
            }

            _clients.Add(client);
            Scribe.Info($"[Watcher] Client {client.Name} (ID: {client.Id}) registered. Total clients: {ConnectedClientCount}");
            return true;
        }
        catch (Exception ex)
        {
            Scribe.Error($"[Watcher] RegisterClient failed for {client.Name}", ex);
            return false;
        }
    }

    /// <summary>
    /// Unregisters a client from the Watcher.
    /// </summary>
    /// <param name="client">The client to unregister</param>
    /// <returns>True if the client was found and removed, false otherwise</returns>
    public bool UnregisterClient(FluffyClient client)
    {
        if (client == null)
        {
            Scribe.Warning("[Watcher] Attempted to unregister null client");
            return false;
        }

        try
        {
            bool removed = _clients.Remove(client);

            if (removed)
            {
                Scribe.Info($"[Watcher] Client {client.Name} (ID: {client.Id}) unregistered. Total clients: {ConnectedClientCount}");
            }
            else
            {
                Scribe.Warning($"[Watcher] Client {client.Name} (ID: {client.Id}) was not registered");
            }

            return removed;
        }
        catch (Exception ex)
        {
            Scribe.Error($"[Watcher] UnregisterClient failed for {client.Name}", ex);
            return false;
        }
    }

    /// <summary>
    /// Gets a snapshot of all currently connected clients.
    /// Safe for iteration without holding locks.
    /// </summary>
    /// <returns>A copy of the client list</returns>
    public List<FluffyClient> GetAllClients()
    {
        return _clients.Snapshot();
    }

    /// <summary>
    /// Finds a client by their ID.
    /// </summary>
    /// <param name="clientId">The client ID to search for</param>
    /// <returns>The matching client, or null if not found</returns>
    public FluffyClient? GetClientById(int clientId)
    {
        return _clients.Find(c => c.Id == clientId);
    }
}