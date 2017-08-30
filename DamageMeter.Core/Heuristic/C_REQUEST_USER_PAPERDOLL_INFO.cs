using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class C_REQUEST_USER_PAPERDOLL_INFO : AbstractPacketHeuristic
    {
        public static C_REQUEST_USER_PAPERDOLL_INFO Instance => _instance ?? (_instance = new C_REQUEST_USER_PAPERDOLL_INFO());
        private static C_REQUEST_USER_PAPERDOLL_INFO _instance;

        public C_REQUEST_USER_PAPERDOLL_INFO() : base(OpcodeEnum.C_REQUEST_USER_PAPERDOLL_INFO) { }
        public ushort PossibleOpcode;
        public string Name = "";
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;

            if (message.Payload.Count < 6) return;
            Reader.Skip(2);
            try
            {
                Name = Reader.ReadTeraString();
            }
            catch (Exception e) { return; }

            PossibleOpcode = message.OpCode;
        }

        public void Confirm()
        {
            OpcodeFinder.Instance.SetOpcode(PossibleOpcode, OPCODE);
        }
    }
}
