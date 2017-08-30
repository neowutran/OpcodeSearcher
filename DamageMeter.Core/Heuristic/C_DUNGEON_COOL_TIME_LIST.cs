using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class C_DUNGEON_COOL_TIME_LIST : AbstractPacketHeuristic // #1
    {
        public static C_DUNGEON_COOL_TIME_LIST Instance => _instance ?? (_instance = new C_DUNGEON_COOL_TIME_LIST());
        private static C_DUNGEON_COOL_TIME_LIST _instance;

        public C_DUNGEON_COOL_TIME_LIST() : base(OpcodeEnum.C_DUNGEON_COOL_TIME_LIST) { }

        public ushort PossibleOpcode;
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if(message.Payload.Count != 0) return;
            if (C_NPCGUILD_LIST.Instance.PossibleOpcode != 0 && C_DUNGEON_CLEAR_COUNT_LIST.Instance.PossibleOpcode != 0) return;
            PossibleOpcode = message.OpCode;
        }
        public void Confirm()
        {
            OpcodeFinder.Instance.SetOpcode(PossibleOpcode, OPCODE);
        }
    }
}
