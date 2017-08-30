using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class C_CHANGE_PARTY_MEMBER_AUTHORITY : AbstractPacketHeuristic
    {
        public static C_CHANGE_PARTY_MEMBER_AUTHORITY Instance => _instance ?? (_instance = new C_CHANGE_PARTY_MEMBER_AUTHORITY());
        private static C_CHANGE_PARTY_MEMBER_AUTHORITY _instance;

        public C_CHANGE_PARTY_MEMBER_AUTHORITY() : base(OpcodeEnum.C_CHANGE_PARTY_MEMBER_AUTHORITY) { }

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_PARTY_MEMBER_LIST)) return;
            if(message.Payload.Count != 4+4+1)return;
            Reader.Skip(8);
            var canInvite = Reader.ReadByte();
            if(canInvite != 0 && canInvite != 1) return;

            //TODO: add party info in DB and do more checks on that

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
