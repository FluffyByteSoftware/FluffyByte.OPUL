using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluffyByte.OPUL.Core.FluffyIO;

public class FluffySystemOperator
{
    private static readonly Lazy<FluffySystemOperator> _instance = new(() => new());
    public static FluffySystemOperator Instance => _instance.Value;
    private FluffySystemOperator() { }



    private readonly List<IFluffyCore> _coreProcesses = [];

}
