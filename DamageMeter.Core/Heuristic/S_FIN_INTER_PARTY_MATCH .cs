using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class S_FIN_INTER_PARTY_MATCH : AbstractPacketHeuristic
    {
        public static S_FIN_INTER_PARTY_MATCH Instance => _instance ?? (_instance = new S_FIN_INTER_PARTY_MATCH());
        private static S_FIN_INTER_PARTY_MATCH _instance;

        public S_FIN_INTER_PARTY_MATCH() : base(OpcodeEnum.S_FIN_INTER_PARTY_MATCH) { }

        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (message.Payload.Count != 4) return;
            var zoneId = Reader.ReadUInt32();
            if (zoneId != 9777 && zoneId != 777) return; // TODO CW, need to remove this hardcoded check once we have enough data to detect without it

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
