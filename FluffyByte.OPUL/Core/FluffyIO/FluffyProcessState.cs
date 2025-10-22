using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluffyByte.OPUL.Core.FluffyIO;

public enum FluffyProcessState
{
    Stopped,
    Starting,
    Running,
    Stopping,
    Error
}
