using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    public class S_UPDATE_CONTENTS_ON_OFF : AbstractPacketHeuristic
    {

        public static S_UPDATE_CONTENTS_ON_OFF Instance => _instance ?? (_instance = new S_UPDATE_CONTENTS_ON_OFF());
        private static S_UPDATE_CONTENTS_ON_OFF _instance;

        public bool Initialized = false;
        private S_UPDATE_CONTENTS_ON_OFF() : base(OpcodeEnum.S_UPDATE_CONTENTS_ON_OFF) { }
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }

            if (OpcodeFinder.Instance.PacketCount > 12 && OpcodeFinder.Instance.PacketCount < 20 && message.Payload.Count == 5)
            {
                OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            }

        }
    }
}
