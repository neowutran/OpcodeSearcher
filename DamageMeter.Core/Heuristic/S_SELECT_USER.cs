using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    public class S_SELECT_USER : AbstractPacketHeuristic
    {
        public static S_SELECT_USER Instance => _instance ?? (_instance = new S_SELECT_USER());
        private static S_SELECT_USER _instance;

        public bool Initialized = false;
        private S_SELECT_USER() : base(OpcodeEnum.S_SELECT_USER) { }
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.C_SELECT_USER)) return;
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }

            if (message.Payload.Count == 11)
            {
                OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            }

        }
    }
}
