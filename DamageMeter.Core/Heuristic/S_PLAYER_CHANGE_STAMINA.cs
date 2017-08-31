using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class S_PLAYER_CHANGE_STAMINA : AbstractPacketHeuristic
    {
        public static S_PLAYER_CHANGE_STAMINA Instance => _instance ?? (_instance = new S_PLAYER_CHANGE_STAMINA());
        private static S_PLAYER_CHANGE_STAMINA _instance;

        public S_PLAYER_CHANGE_STAMINA() : base(OpcodeEnum.S_PLAYER_CHANGE_STAMINA) { }

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }
            if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_LOGIN)) return;
            if (message.Payload.Count != 4+4+4+4+4) return;

            var curSt = Reader.ReadUInt32();
            var maxSt = Reader.ReadUInt32();
            var ch = (LoggedCharacter)OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter].Item2;
            if (ch.MaxSt == maxSt) { OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE); }
        }
    }
}
