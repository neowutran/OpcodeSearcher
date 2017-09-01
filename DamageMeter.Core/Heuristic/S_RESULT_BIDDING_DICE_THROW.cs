using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class S_RESULT_BIDDING_DICE_THROW : AbstractPacketHeuristic
    {
        public static S_RESULT_BIDDING_DICE_THROW Instance => _instance ?? (_instance = new S_RESULT_BIDDING_DICE_THROW());
        private static S_RESULT_BIDDING_DICE_THROW _instance;

        public S_RESULT_BIDDING_DICE_THROW() : base(OpcodeEnum.S_RESULT_BIDDING_DICE_THROW) { }

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count != 8+4) return;
            if(!OpcodeFinder.Instance.KnowledgeDatabase.ContainsKey(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList))return;

            var cid = Reader.ReadUInt64();
            var roll = Reader.ReadUInt32();

            if(roll > 100 && roll != UInt32.MaxValue) return;
            if(!DbUtils.IsPartyMember(cid) && DbUtils.GetPlayercId() != cid) return;

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
