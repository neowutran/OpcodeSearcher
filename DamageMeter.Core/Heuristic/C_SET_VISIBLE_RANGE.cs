using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{

    public class C_SET_VISIBLE_RANGE : AbstractPacketHeuristic
    {
        public static C_SET_VISIBLE_RANGE Instance => _instance ?? (_instance = new C_SET_VISIBLE_RANGE());
        private static C_SET_VISIBLE_RANGE _instance;

        public bool Initialized = false;
        private C_SET_VISIBLE_RANGE() : base(OpcodeEnum.C_SET_VISIBLE_RANGE) { }
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }

            if (OpcodeFinder.Instance.PacketCount > 6 && OpcodeFinder.Instance.PacketCount < 10 && message.Payload.Count == 4)
            {
                OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            }

        }
    }
}
