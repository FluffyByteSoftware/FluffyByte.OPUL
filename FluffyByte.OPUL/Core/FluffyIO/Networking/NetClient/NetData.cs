using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluffyByte.OPUL.Core.FluffyIO.Networking.NetClient;


public class NetData
{
    private readonly FluffyClient _parent;
    private readonly CancellationToken _shutdownToken;

    public NetData(FluffyClient parent, CancellationToken shutdownToken)
    {

    }
}
