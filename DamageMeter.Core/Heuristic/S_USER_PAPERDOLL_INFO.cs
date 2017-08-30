using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class S_USER_PAPERDOLL_INFO : AbstractPacketHeuristic
    {
        public static S_USER_PAPERDOLL_INFO Instance => _instance ?? (_instance = new S_USER_PAPERDOLL_INFO());
        private static S_USER_PAPERDOLL_INFO _instance;

        public S_USER_PAPERDOLL_INFO() : base(OpcodeEnum.S_USER_PAPERDOLL_INFO) { }

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if(C_REQUEST_USER_PAPERDOLL_INFO.Instance.PossibleOpcode == 0) return;

            if (message.Payload.Count < 4000) return;
            Reader.Skip(2 + 2 + 2 + 2 + 2 + 2);
            try
            {
                var nameOffset = Reader.ReadUInt16();
                Reader.BaseStream.Position = nameOffset - 4;
                var name = Reader.ReadTeraString();
                if (name != "" && name.Equals(C_REQUEST_USER_PAPERDOLL_INFO.Instance.Name, StringComparison.OrdinalIgnoreCase))
                {
                    OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
                    C_REQUEST_USER_PAPERDOLL_INFO.Instance.Confirm();
                }
            }
            catch (Exception e){return;}

        }
    }
}

