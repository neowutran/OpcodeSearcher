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
        public static S_CLEAR_QUEST_INFO Instance => _instance ?? (_instance = new S_CLEAR_QUEST_INFO());
        private static S_CLEAR_QUEST_INFO _instance;

        public S_CLEAR_QUEST_INFO() : base(OpcodeEnum.S_CLEAR_QUEST_INFO) { }
        private bool first = true;
        private ushort PossibleOpcode;
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;

            //---all this to avoid conflict with S_PING (just checking that we don't get C_PONG after 1st 0-length packet)---//
            var previousPacket = OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1);
            if(previousPacket.OpCode == OpcodeFinder.Instance.GetOpcode(OpcodeEnum.C_PONG)) return;
            if (!first) OpcodeFinder.Instance.SetOpcode(PossibleOpcode, OPCODE);
            //-------//

            if (message.Payload.Count != 0) return;
            //this is most likely to be the 1st 0-length server packet after login
            first = false;
            PossibleOpcode = message.OpCode;
        }
    }
}
