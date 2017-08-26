using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    public class S_LOGIN_ARBITER : AbstractPacketHeuristic
    {
        public static S_LOGIN_ARBITER Instance => _instance ?? (_instance = new S_LOGIN_ARBITER());
        private static S_LOGIN_ARBITER _instance;

        public bool Initialized = false;
        private S_LOGIN_ARBITER() : base(OpcodeEnum.S_LOGIN_ARBITER) { }
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }

            if (OpcodeFinder.Instance.PacketCount > 4 && OpcodeFinder.Instance.PacketCount < 10 && message.Payload.Count == 21)
            {
                OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            }

        }
    }
}
