using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class S_PARTY_MEMBER_CHANGE_STAMINA : AbstractPacketHeuristic
    {
        public static S_PARTY_MEMBER_CHANGE_STAMINA Instance => _instance ?? (_instance = new S_PARTY_MEMBER_CHANGE_STAMINA());
        private static S_PARTY_MEMBER_CHANGE_STAMINA _instance;

        public S_PARTY_MEMBER_CHANGE_STAMINA() : base(OpcodeEnum.S_PARTY_MEMBER_CHANGE_STAMINA) { }

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_PARTY_MEMBER_LIST)) return;
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_PARTY_MEMBER_STAT_UPDATE)) return;
            if (message.Payload.Count != 4 + 4 + 4 + 4 + 4) return;
            if (!OpcodeFinder.Instance.KnowledgeDatabase.ContainsKey(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList)) return;

            var serverId = Reader.ReadUInt32();
            var playerId = Reader.ReadUInt32();
            var curRe = Reader.ReadUInt32();
            var maxRe = Reader.ReadUInt32();
            var unk = Reader.ReadUInt32();

            if (!DbUtils.IsPartyMember(playerId, serverId)) return;
            var m = DbUtils.GetPartyMember(playerId, serverId);
            if (m.MaxRe != maxRe) return;

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
