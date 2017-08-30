using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class S_LEAVE_PARTY_MEMBER : AbstractPacketHeuristic
    {
        public static S_LEAVE_PARTY_MEMBER Instance => _instance ?? (_instance = new S_LEAVE_PARTY_MEMBER());
        private static S_LEAVE_PARTY_MEMBER _instance;

        public S_LEAVE_PARTY_MEMBER() : base(OpcodeEnum.S_LEAVE_PARTY_MEMBER) { }

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                if (OpcodeFinder.Instance.GetOpcode(OPCODE) == message.OpCode)
                {
                    Parse();
                }
                return;
            }

            if (message.Payload.Count < 2 + 4 + 4 + 4) return;
            var nameOffset = Reader.ReadUInt16();
            if (nameOffset != 14) return;

            var serverId = Reader.ReadUInt32();
            if (BasicTeraData.Instance.Servers.GetServer(serverId) == null) return;

            var playerId = Reader.ReadUInt32();
            var name = "";
            try
            {
                if (Reader.BaseStream.Position != nameOffset - 4) return;
                name = Reader.ReadTeraString();

            }
            catch (Exception e) { return; }
            if (OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList, out var res))
            {
                var list = (List<PartyMember>)res.Item2;
                if (!list.Any(x => x.PlayerId == playerId && x.ServerId == serverId && x.Name == name)) return;
            }
            else return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            RemovePartyMember(playerId, serverId, name);
        }

        private void Parse()
        {
            Reader.Skip(2);
            var serverId = Reader.ReadUInt32();
            var playerId = Reader.ReadUInt32();
            var name = Reader.ReadTeraString();
            RemovePartyMember(playerId, serverId, name);

        }
        private void RemovePartyMember(uint playerId, uint serverId, string name)
        {
            if (OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList, out var res))
            {
                var list = (List<PartyMember>)res.Item2;
                if (!list.Any(x => x.PlayerId == playerId && x.ServerId == serverId && x.Name == name)) return;
                list.Remove(new PartyMember(playerId, serverId, name));
                OpcodeFinder.Instance.KnowledgeDatabase.Remove(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList);
                OpcodeFinder.Instance.KnowledgeDatabase.Add(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList, new Tuple<Type, object>(typeof(List<PartyMember>), list));
            }
        }
    }
}
