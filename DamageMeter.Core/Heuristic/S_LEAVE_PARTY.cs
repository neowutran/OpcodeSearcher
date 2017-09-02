using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class S_LEAVE_PARTY : AbstractPacketHeuristic
    {
        public static S_LEAVE_PARTY Instance => _instance ?? (_instance = new S_LEAVE_PARTY());
        private static S_LEAVE_PARTY _instance;

        public S_LEAVE_PARTY() : base(OpcodeEnum.S_LEAVE_PARTY) { }

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;

            if (message.Payload.Count != 0) return;
            if(!DbUtils.IsPartyFormed()) return;
            var msg = OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1);
            if (msg.Payload.Count != 0) return;
            if(msg.Direction != MessageDirection.ClientToServer) return;
            //TODO: need more checks?
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            OpcodeFinder.Instance.SetOpcode(msg.OpCode, OpcodeEnum.C_LEAVE_PARTY);
            OpcodeFinder.Instance.KnowledgeDatabase.TryRemove(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList, out var x);
        }
    }
}
