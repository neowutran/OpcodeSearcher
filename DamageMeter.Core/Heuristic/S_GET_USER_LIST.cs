using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    public class S_GET_USER_LIST : AbstractPacketHeuristic
    {
        public static S_GET_USER_LIST Instance => _instance ?? (_instance = new S_GET_USER_LIST());
        private static S_GET_USER_LIST _instance;

        public bool Initialized = false;
        private S_GET_USER_LIST() : base(OpcodeEnum.S_GET_USER_LIST) { }
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }

            if (OpcodeFinder.Instance.PacketCount == 10 && message.Payload.Count > 100)
            {
                OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            }

        }
    }
}
