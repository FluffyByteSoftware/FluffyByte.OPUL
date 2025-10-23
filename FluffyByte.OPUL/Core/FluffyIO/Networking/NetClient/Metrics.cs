using System.Net;
using System.Net.Sockets;

namespace FluffyByte.OPUL.Core.FluffyIO.Networking.NetClient;

public class Metrics(FluffyClient parent)
{
    public int LastPing { get; private set; } = -20;
    public double AveragePing { get; private set; } = -20;
    public DateTime LastResponseTime { get; private set; } = DateTime.Now;
    public DateTime ConnectedAt { get; private set; } = DateTime.Now;

    private readonly FluffyClient _parent = parent;

    /// <summary>
    /// Updates the activity timestamp to the current date and time.
    /// </summary>
    /// <remarks>This method sets the <see cref="LastResponseTime"/> property to the current system time,
    /// indicating the most recent activity. It is typically used to track the last interaction time.</remarks>
    public void UpdateActivity()
    {
        LastResponseTime = DateTime.Now;
    }

    /// <summary>
    /// Updates the last recorded ping time and recalculates the average ping time.
    /// </summary>
    /// <remarks>The method updates the <see cref="LastPing"/> property with the provided ping time and
    /// recalculates the <see cref="AveragePing"/> as a simple average of the previous average and the new ping time.
    /// This method assumes that the ping time is a valid, non-negative value.</remarks>
    /// <param name="pingMs">The latest ping time in milliseconds. Must be a non-negative integer.</param>
    public void UpdatePing(int pingMs)
    {
        LastPing = pingMs;
        AveragePing = (AveragePing * pingMs) / 2.0;
    }

    public double IdleTimeSeconds
    {
        get
        {
            return (DateTime.Now - LastResponseTime).TotalSeconds;
        }
    }
}
