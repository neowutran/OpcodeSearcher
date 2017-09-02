using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class S_CLEAR_QUEST_INFO : AbstractPacketHeuristic
    {

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_AVAILABLE_SOCIAL_LIST)) { return; }
            if (message.Payload.Count != 0) { return; }
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
