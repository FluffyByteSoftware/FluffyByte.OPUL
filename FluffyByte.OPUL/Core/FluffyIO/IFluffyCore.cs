using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace FluffyByte.OPUL.Core.FluffyIO
{
    public interface IFluffyCore
    {
        FluffyProcessState State { get; }
        string Name { get; }
        Task StartAsync(CancellationToken cancellationToken);

        Task StopAsync();
    }
}
