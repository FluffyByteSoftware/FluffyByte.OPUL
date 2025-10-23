using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace FluffyByte.OPUL.Core.FluffyIO
{
    public interface IFluffyCoreProcess
    {
        /// <summary>
        /// Gets the current state of the process.
        /// </summary>
        FluffyProcessState State { get; }
        
        
        /// <summary>
        /// Gets the name of the process.
        /// </summary>
        string Name { get; }

        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync();
    }
}
