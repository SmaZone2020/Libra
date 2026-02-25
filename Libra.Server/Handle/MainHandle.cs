using Libra.Virgo.Enum;
using Libra.Virgo.Models;

namespace Libra.Server.Handle
{
    public class MainHandle
    {
        public static void Handle(VirgoMessageType type, object data )
        {
            switch (type)
            {
                case Virgo.Enum.VirgoMessageType.Message:
                    break;

                case Virgo.Enum.VirgoMessageType.Response:
                    break;

                case Virgo.Enum.VirgoMessageType.Command:
                    
                    CommandHandle.Handle(data);
                    break;

                default:
                    break;
            }
        }
    }
}
