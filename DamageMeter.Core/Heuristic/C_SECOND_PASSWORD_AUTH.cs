using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class C_SECOND_PASSWORD_AUTH : AbstractPacketHeuristic
    {
        public static C_SECOND_PASSWORD_AUTH Instance => _instance ?? (_instance = new C_SECOND_PASSWORD_AUTH());
        private static C_SECOND_PASSWORD_AUTH _instance;

        public C_SECOND_PASSWORD_AUTH() : base(OpcodeEnum.C_SECOND_PASSWORD_AUTH) { }
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode) || OpcodeFinder.Instance.PacketCount > 10) return;
            if (message.Payload.Count != 132) return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);

        }


    }
}
