using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class S_CREATURE_CHANGE_HP : AbstractPacketHeuristic
    {
        public static S_CREATURE_CHANGE_HP Instance => _instance ?? (_instance = new S_CREATURE_CHANGE_HP());
        private static S_CREATURE_CHANGE_HP _instance;

        public S_CREATURE_CHANGE_HP() : base(OpcodeEnum.S_CREATURE_CHANGE_HP) { }

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }

            if (message.Payload.Count != 4 + 4 + 4 + 4 + 8 + 8 + 1 + 4) return;

            var curHp = Reader.ReadUInt32();
            var maxHp = Reader.ReadUInt32();
            var diff = Reader.ReadUInt32();
            var type = Reader.ReadUInt32();
            var target = Reader.ReadUInt64();
            var source = Reader.ReadUInt64();
            var unk1 = Reader.ReadByte();
            if (unk1 != 0) return;
            var ch = (LoggedCharacter)OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter].Item2;
            if (ch.Cid != target) { return; } //the packet applies to any entity, but we use logged player for simplicity
            if (ch.MaxHp == maxHp) { OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE); }
        }
    }
}
