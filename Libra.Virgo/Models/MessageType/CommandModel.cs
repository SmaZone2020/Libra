using Libra.Virgo.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Libra.Virgo.Models.MessageType
{
    public class CommandModel
    {
        public Guid TaskId { get; set; }
        public CommandType Type {  get; set; }
        public uint Flag { get; set; }
        public string[] Parameter { get; set; } = [];

    }
}
