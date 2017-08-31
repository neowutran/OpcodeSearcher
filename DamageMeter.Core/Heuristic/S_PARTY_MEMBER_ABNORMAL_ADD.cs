using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class S_PARTY_MEMBER_ABNORMAL_ADD : AbstractPacketHeuristic
    {
        public static S_PARTY_MEMBER_ABNORMAL_ADD Instance => _instance ?? (_instance = new S_PARTY_MEMBER_ABNORMAL_ADD());
        private static S_PARTY_MEMBER_ABNORMAL_ADD _instance;

        public S_PARTY_MEMBER_ABNORMAL_ADD() : base(OpcodeEnum.S_PARTY_MEMBER_ABNORMAL_ADD) { }

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                if (OpcodeFinder.Instance.GetOpcode(OPCODE) == message.OpCode) { Parse(); }
                return;
            }

            if(message.Payload.Count != 4+4+4+4+4+4) return;

            if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_PARTY_MEMBER_LIST)) return;                                              //check that we already are in party
            if (!OpcodeFinder.Instance.KnowledgeDatabase.ContainsKey(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList)) return;   //

            var serverId = Reader.ReadUInt32();
            var playerId = Reader.ReadUInt32();
            var abnormId = Reader.ReadUInt32(); //maybe it would be better to use a specific buff (minor battle solution?)
            var duration = Reader.ReadUInt32();
            var unk = Reader.ReadUInt32();
            var stacks = Reader.ReadUInt32();

            if(!DbUtils.IsPartyMember(playerId, serverId)) return;
            
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            DbUtils.AddPartyMemberAbnormal(playerId, serverId, abnormId);
        }

        private void Parse()
        {
            var serverId = Reader.ReadUInt32();
            var playerId = Reader.ReadUInt32();
            var abnormId = Reader.ReadUInt32();
            DbUtils.AddPartyMemberAbnormal(playerId, serverId, abnormId);

        }
    }
}
