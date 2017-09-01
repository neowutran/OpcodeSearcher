using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class C_PARTY_APPLICATION_DENIED : AbstractPacketHeuristic
    {
        public static C_PARTY_APPLICATION_DENIED Instance => _instance ?? (_instance = new C_PARTY_APPLICATION_DENIED());
        private static C_PARTY_APPLICATION_DENIED _instance;

        public C_PARTY_APPLICATION_DENIED() : base(OpcodeEnum.C_PARTY_APPLICATION_DENIED) { }

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if(message.Payload.Count != 4) return;
            if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_OTHER_USER_APPLY_PARTY)) return;
            var pId = Reader.ReadUInt32();
            if(S_OTHER_USER_APPLY_PARTY.Instance.LastApply != pId) return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
