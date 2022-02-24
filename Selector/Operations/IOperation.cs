using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Selector.Operations
{
    public interface IOperation
    {
        event EventHandler Success;
        Task Execute();
        Task Task { get; }
    }
}
