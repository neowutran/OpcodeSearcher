using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class C_REQUEST_PARTY_INFO : AbstractPacketHeuristic
    {
        public static uint LastId;
        public static ushort PossibleOpcode;
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count != 4) return;
            LastId = Reader.ReadUInt32();
            PossibleOpcode = message.OpCode;
        }

        public static void Confirm()
        {
            OpcodeFinder.Instance.SetOpcode(PossibleOpcode, OpcodeEnum.C_REQUEST_PARTY_INFO);

        }
    }
}
