using System;
using System.Collections.Generic;
using System.Text;

namespace Libra.Virgo.Models.MessageType
{
    public class CommandResult
    {
        public Guid TaskId { get; set;}
        public object ?Result { get; set; } = null;
        public DateTime EndTime { get; set;}

    }
}
