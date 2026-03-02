using Libra.Virgo.Enum;
using Libra.Virgo.Models.MessageType;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Libra.Agent.Handle
{
    internal class MainHandle
    {
        public static async Task Handle(string dataJson, VirgoMessageType type)
        {
            switch (type)
            {
                case VirgoMessageType.Message:
                    break;

                case VirgoMessageType.Response:
                    break;

                case VirgoMessageType.Command:

                    await CommandHandle.Handle(dataJson);
                    break;

                default:
                    break;
            }
        }
    }
}
