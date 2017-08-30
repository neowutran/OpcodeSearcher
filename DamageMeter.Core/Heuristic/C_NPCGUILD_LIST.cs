using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class C_NPCGUILD_LIST : AbstractPacketHeuristic //   #2
    {
        public static C_NPCGUILD_LIST Instance => _instance ?? (_instance = new C_NPCGUILD_LIST());
        private static C_NPCGUILD_LIST _instance;

        public C_NPCGUILD_LIST() : base(OpcodeEnum.C_NPCGUILD_LIST) { }
        public ushort PossibleOpcode;
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if(message.Payload.Count < 6) return;
            if (OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1).Payload.Count != 0) return;
            Reader.Skip(2);
            try
            {
                var name = Reader.ReadTeraString();
                if (OpcodeFinder.Instance.KnowledgeDatabase.ContainsKey(OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter))
                {
                    if(((LoggedCharacter)OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter].Item2).Name != name) return;
                }
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

