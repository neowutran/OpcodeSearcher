using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class S_PLAYER_STAT_UPDATE : AbstractPacketHeuristic
    {
        public static S_PLAYER_STAT_UPDATE Instance => _instance ?? (_instance = new S_PLAYER_STAT_UPDATE());
        private static S_PLAYER_STAT_UPDATE _instance;

        public S_PLAYER_STAT_UPDATE() : base(OpcodeEnum.S_PLAYER_STAT_UPDATE) { }
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }
            if (message.Payload.Count != 231) return;

            if (!OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter, out Tuple<Type, object> result)) return;
            var ch = (LoggedCharacter)result.Item2;
            Reader.Skip(4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 2 + 2 + 2 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 2 + 2 + 2 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4);
            var lvl1 = Reader.ReadUInt16();
            if (lvl1 != ch.Level) return;
            var unk4 = Reader.ReadByte();
            if (unk4 != 0 && unk4 != 1) return;
            Reader.Skip(4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4);
            var unk7 = Reader.ReadUInt16();
            if (unk7 != 0) return;
            Reader.Skip(2);
            //var unk9 = Reader.ReadUInt32(); //not always true apparently
            //if (unk9 != 8000) return;
            Reader.Skip(4);
            var unk10 = Reader.ReadUInt32();
            if (unk10 != 3) return;
            var lvl2 = Reader.ReadUInt16();
            if (lvl2 != ch.Level) return;
            Reader.Skip(4);
            //var unk13 = Reader.ReadUInt32(); //not always true either
            //if (unk13 != 0) return;
            //var unk14 = Reader.ReadSingle(); //same for this
            //if (unk14 != 1) return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);

        }
    }
}
