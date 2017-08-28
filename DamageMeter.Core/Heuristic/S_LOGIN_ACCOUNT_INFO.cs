using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{


    public class S_LOGIN_ACCOUNT_INFO : AbstractPacketHeuristic
    {
        public static S_LOGIN_ACCOUNT_INFO Instance => _instance ?? (_instance = new S_LOGIN_ACCOUNT_INFO());
        private static S_LOGIN_ACCOUNT_INFO _instance;

        public bool Initialized = false;
        private S_LOGIN_ACCOUNT_INFO() : base(OpcodeEnum.S_LOGIN_ACCOUNT_INFO) { }
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }

            if (OpcodeFinder.Instance.PacketCount > 6 && OpcodeFinder.Instance.PacketCount < 10 && message.Payload.Count > 18)
            {
                OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            }

        }
    }
}
