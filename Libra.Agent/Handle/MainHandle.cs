using Libra.Virgo.Enum;
using Libra.Virgo.Models.MessageType;
using Newtonsoft.Json;
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
            Console.WriteLine($"收到消息类型: {type}");
            
            Console.WriteLine($"数据长度: {dataJson.Length}");
            Console.WriteLine($"原始JSON: {dataJson}");

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
