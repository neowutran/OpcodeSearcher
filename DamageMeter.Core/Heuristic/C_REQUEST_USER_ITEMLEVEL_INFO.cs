using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class C_REQUEST_USER_ITEMLEVEL_INFO : AbstractPacketHeuristic //    #4
    {
        public static C_REQUEST_USER_ITEMLEVEL_INFO Instance => _instance ?? (_instance = new C_REQUEST_USER_ITEMLEVEL_INFO());
        private static C_REQUEST_USER_ITEMLEVEL_INFO _instance;

        public C_REQUEST_USER_ITEMLEVEL_INFO() : base(OpcodeEnum.C_REQUEST_USER_ITEMLEVEL_INFO) { }

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if(message.Payload.Count != 0) return;

            if (OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1).OpCode != C_DUNGEON_CLEAR_COUNT_LIST.Instance.PossibleOpcode) return;
            if (OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 2).OpCode != C_NPCGUILD_LIST.Instance.PossibleOpcode) return;
            if (OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 3).OpCode != C_DUNGEON_COOL_TIME_LIST.Instance.PossibleOpcode) return;

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            C_DUNGEON_CLEAR_COUNT_LIST.Instance.Confirm();
            C_NPCGUILD_LIST.Instance.Confirm();
            C_DUNGEON_COOL_TIME_LIST.Instance.Confirm();
        }
    }
}
