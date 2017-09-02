using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class S_LEAVE_PRIVATE_CHANNEL : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count != 4) return;
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_LOGIN)) return;
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_JOIN_PRIVATE_CHANNEL)) return;
            if(Reader.ReadUInt32() != S_JOIN_PRIVATE_CHANNEL.LastJoinedChannelId) return;
            //need check on client packet?
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
