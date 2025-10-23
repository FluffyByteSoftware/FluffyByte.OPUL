using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluffyByte.OPUL.Core.FluffyIO;

/// <summary>
/// Represents the various states of a fluffy process during its lifecycle.
/// </summary>
/// <remarks>This enumeration is used to track the current state of a fluffy process,  allowing for state-specific
/// handling and transitions. The states include: 
/// <list type="bullet"> 
/// <item>
/// <description>
/// <see cref="Stopped"/>: The process is not running.
/// </description>
/// </item> 
/// <item>
/// <description>
/// <see cref="Starting"/>: The process is in the process of starting up.
/// </description>
/// </item> 
/// <item>
/// <description>
/// <see cref="Running"/> : The process is currently
/// running.
/// </description>
/// </item> 
/// <item>
/// <description>
/// <see cref="Stopping"/>: The process is in the process of shutting
/// down.
/// </description>
/// </item> 
/// <item>
/// <description>
/// <see cref="Error"/>: The process has encountered an
/// error.
/// </description></item> </list></remarks>
public enum FluffyProcessState
{
    Stopped,
    Starting,
    Running,
    Stopping,
    Error
}
