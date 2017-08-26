using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    public class S_ACCOUNT_PACKAGE_LIST : AbstractPacketHeuristic
    {

        public static S_ACCOUNT_PACKAGE_LIST Instance => _instance ?? (_instance = new S_ACCOUNT_PACKAGE_LIST());
        private static S_ACCOUNT_PACKAGE_LIST _instance;

        public bool Initialized = false;
        private S_ACCOUNT_PACKAGE_LIST() : base(OpcodeEnum.S_ACCOUNT_PACKAGE_LIST) { }
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }

            if (OpcodeFinder.Instance.PacketCount > 10 && OpcodeFinder.Instance.PacketCount < 15 && message.Payload.Count == 38)
            {
                OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            }

        }
    }
}

