using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class S_PARTY_MEMBER_CHANGE_HP : AbstractPacketHeuristic
    {
        public static S_PARTY_MEMBER_CHANGE_HP Instance => _instance ?? (_instance = new S_PARTY_MEMBER_CHANGE_HP());
        private static S_PARTY_MEMBER_CHANGE_HP _instance;

        public S_PARTY_MEMBER_CHANGE_HP() : base(OpcodeEnum.S_PARTY_MEMBER_CHANGE_HP) { }

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_PARTY_MEMBER_LIST)) return;
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_PARTY_MEMBER_STAT_UPDATE)) return;
            if (message.Payload.Count != 4 + 4 + 4 + 4 ) return;
            if (!OpcodeFinder.Instance.KnowledgeDatabase.ContainsKey(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList)) return;

            var serverId = Reader.ReadUInt32();
            var playerId = Reader.ReadUInt32();
            var curHp = Reader.ReadUInt32();
            var maxHp = Reader.ReadUInt32();

            if(!DbUtils.IsPartyMember(playerId, serverId)) return;
            var m = DbUtils.GetPartyMember(playerId, serverId);
            if(m.MaxHp == 0 || m.MaxHp != maxHp) return;
            
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
