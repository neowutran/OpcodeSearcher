using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    public class C_GET_USER_LIST : AbstractPacketHeuristic
    {
        public static C_GET_USER_LIST Instance => _instance ?? (_instance = new C_GET_USER_LIST());
        private static C_GET_USER_LIST _instance;

        public bool Initialized = false;
        private C_GET_USER_LIST() : base(OpcodeEnum.C_GET_USER_LIST) { }
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }

            if (OpcodeFinder.Instance.PacketCount > 7 && OpcodeFinder.Instance.PacketCount < 13 && message.Payload.Count == 0)
            {
                OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            }

        }
    }
}
