using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    public class S_LOADING_SCREEN_CONTROL_INFO : AbstractPacketHeuristic
    {
        public static S_LOADING_SCREEN_CONTROL_INFO Instance => _instance ?? (_instance = new S_LOADING_SCREEN_CONTROL_INFO());
        private static S_LOADING_SCREEN_CONTROL_INFO _instance;

        public bool Initialized = false;
        private S_LOADING_SCREEN_CONTROL_INFO() : base(OpcodeEnum.S_LOADING_SCREEN_CONTROL_INFO) { }
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }

            if (OpcodeFinder.Instance.PacketCount > 2 && OpcodeFinder.Instance.PacketCount < 10 && message.Payload.Count == 3)
            {
                OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            }

        }
    }
}
