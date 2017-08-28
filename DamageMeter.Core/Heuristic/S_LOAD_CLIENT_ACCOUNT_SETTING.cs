using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    public class S_LOAD_CLIENT_ACCOUNT_SETTING : AbstractPacketHeuristic
    {
        public static S_LOAD_CLIENT_ACCOUNT_SETTING Instance => _instance ?? (_instance = new S_LOAD_CLIENT_ACCOUNT_SETTING());
        private static S_LOAD_CLIENT_ACCOUNT_SETTING _instance;

        public bool Initialized = false;
        private S_LOAD_CLIENT_ACCOUNT_SETTING() : base(OpcodeEnum.S_LOAD_CLIENT_ACCOUNT_SETTING) { }
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_GET_USER_LIST)) return;
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }
            
            if (OpcodeFinder.Instance.PacketCount < 15 && message.Payload.Count > 1000)
            {
                OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            }

        }
    }
}
