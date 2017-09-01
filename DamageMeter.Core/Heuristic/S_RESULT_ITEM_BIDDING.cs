using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class S_RESULT_ITEM_BIDDING : AbstractPacketHeuristic
    {
        public static S_RESULT_ITEM_BIDDING Instance => _instance ?? (_instance = new S_RESULT_ITEM_BIDDING());
        private static S_RESULT_ITEM_BIDDING _instance;

        public S_RESULT_ITEM_BIDDING() : base(OpcodeEnum.S_RESULT_ITEM_BIDDING) { }

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if(message.Payload.Count != 8) return;

            if(!OpcodeFinder.Instance.KnowledgeDatabase.ContainsKey(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList)) return;
            var playerId = Reader.ReadUInt64(); //this is uint32 in def, but doesen't match
            if(!DbUtils.IsPartyMember(playerId)) return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
