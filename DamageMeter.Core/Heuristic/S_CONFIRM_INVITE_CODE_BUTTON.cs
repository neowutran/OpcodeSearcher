using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{

    public class S_CONFIRM_INVITE_CODE_BUTTON : AbstractPacketHeuristic
    {

        public static S_CONFIRM_INVITE_CODE_BUTTON Instance => _instance ?? (_instance = new S_CONFIRM_INVITE_CODE_BUTTON());
        private static S_CONFIRM_INVITE_CODE_BUTTON _instance;

        public bool Initialized = false;
        private S_CONFIRM_INVITE_CODE_BUTTON() : base(OpcodeEnum.S_CONFIRM_INVITE_CODE_BUTTON) { }
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }

            if (OpcodeFinder.Instance.PacketCount > 11 && OpcodeFinder.Instance.PacketCount < 15 && message.Payload.Count == 15)
            {
                OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            }

        }
    }
}
