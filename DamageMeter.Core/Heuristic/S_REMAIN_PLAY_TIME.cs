using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{

    public class S_REMAIN_PLAY_TIME : AbstractPacketHeuristic
    {
        public static S_REMAIN_PLAY_TIME Instance => _instance ?? (_instance = new S_REMAIN_PLAY_TIME());
        private static S_REMAIN_PLAY_TIME _instance;

        public bool Initialized = false;
        private S_REMAIN_PLAY_TIME() : base(OpcodeEnum.S_REMAIN_PLAY_TIME) { }
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }

            if (OpcodeFinder.Instance.PacketCount > 3 && OpcodeFinder.Instance.PacketCount < 10 && message.Payload.Count == 10)
            {
                OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            }

        }
    }
}
