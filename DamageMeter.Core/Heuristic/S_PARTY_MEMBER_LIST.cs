using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class S_PARTY_MEMBER_LIST : AbstractPacketHeuristic
    {
        public static S_PARTY_MEMBER_LIST Instance => _instance ?? (_instance = new S_PARTY_MEMBER_LIST());
        private static S_PARTY_MEMBER_LIST _instance;

        public S_PARTY_MEMBER_LIST() : base(OpcodeEnum.S_PARTY_MEMBER_LIST) { }
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }

            if (message.Payload.Count < 118) return;
            var count = Reader.ReadUInt16();
            var offset = Reader.ReadUInt16();

            Reader.Skip(1 + 1 + 4 + 4 + 2 + 2 + 4 + 4);
            var unk5 = Reader.ReadUInt32();
            if (unk5 != 1) return;
            var unk6 = Reader.ReadUInt32();
            if (unk6 != 1) return;
            var unk7 = Reader.ReadByte();
            if (unk7 != 0) return;
            var unk8 = Reader.ReadUInt32();
            if (unk8 != 1) return;
            var unk9 = Reader.ReadByte();
            if (unk9 != 0) return;
            var unk10 = Reader.ReadUInt32();
            if (unk10 != 1) return;
            var unk11 = Reader.ReadByte();
            if (unk11 != 0) return;

            for (int i = 0; i < count; i++)
            {
                Reader.BaseStream.Position = offset - 4;
                Reader.Skip(4);
                var nameOffset = Reader.ReadUInt16();
                Reader.Skip(4 + 4);
                var level = Reader.ReadUInt32();
                if (level > 65) return;
                var cl = Reader.ReadUInt32();
                if (cl > 12) return;
                Reader.Skip(1 + 8 + 4 + 1);
                var laurel = Reader.ReadUInt32();
                if (laurel > 5) return;
                Reader.BaseStream.Position = nameOffset - 4;

                try { Reader.ReadTeraString(); }
                catch (Exception e) { return; }
            }

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }

    }
}
