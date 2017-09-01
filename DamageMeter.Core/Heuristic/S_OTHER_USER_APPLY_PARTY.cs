using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class S_OTHER_USER_APPLY_PARTY : AbstractPacketHeuristic
    {
        public static S_OTHER_USER_APPLY_PARTY Instance => _instance ?? (_instance = new S_OTHER_USER_APPLY_PARTY());
        private static S_OTHER_USER_APPLY_PARTY _instance;

        public S_OTHER_USER_APPLY_PARTY() : base(OpcodeEnum.S_OTHER_USER_APPLY_PARTY) { }
        public uint LastApply;
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                if (OpcodeFinder.Instance.GetOpcode(OPCODE) == message.OpCode) { Parse(); }
                return;
            }
            if(message.Payload.Count < 2+1+4+2+2+2+2+1+4) return;
            var nameOffset = Reader.ReadUInt16();
            var unk = Reader.ReadByte();
            var playerId = Reader.ReadUInt32();
            var clas = Reader.ReadUInt16();
            var race = Reader.ReadUInt16();
            var gender = Reader.ReadUInt16();
            var level = Reader.ReadUInt16();
            var unk2 = Reader.ReadByte();

            if(level > 65) return;
            try
            {
                var rgc = new RaceGenderClass((Race)race, (Gender)gender, (PlayerClass)clas);
            }
            catch (Exception e) { return; }

            try
            {
                if(nameOffset != Reader.BaseStream.Position + 4) return;
                var name = Reader.ReadTeraString();
            }
            catch (Exception e) { return; }

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            LastApply = playerId;
        }

        private void Parse()
        {
            Reader.Skip(3);
            LastApply = Reader.ReadUInt32();

        }
    }
}
