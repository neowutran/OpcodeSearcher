using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class S_RETURN_TO_LOBBY : AbstractPacketHeuristic
    {
        private static bool waiting;
        public static ushort PossibleOpcode;
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count != 0) return;
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_PREPARE_RETURN_TO_LOBBY)) return;
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_CLEAR_ALL_HOLDED_ABNORMALITY)) return;
            if(!waiting) return;
            PossibleOpcode = message.OpCode;
        }

        public static void Confirm()
        {
            if(!waiting) return;
            OpcodeFinder.Instance.SetOpcode(PossibleOpcode, OpcodeEnum.S_RETURN_TO_LOBBY);
        }

        public static void Wait()
        {
            waiting = true;
        }
    }
}
