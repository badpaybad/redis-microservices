using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisMicroservices.Core.Distributed
{
    public enum CommandStatus
    {
        None,
        Pushed,
        Pedding,
        Success,
        Error
    }
}
