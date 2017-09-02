using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class S_FRIEND_LIST : AbstractPacketHeuristic
    {
        private int elementLength = 2 + 2 + 2 + 2 + 2 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 1 + 8 + 4 + 4 + 4 + 4 + 4;
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count < 2+2+2+4+ elementLength) return;
            var count = Reader.ReadUInt16();
            if (message.Payload.Count < 2 + 2 + 2 + 4 + elementLength*count) return;

            var offset = Reader.ReadUInt16();
            var personalNoteOffset = Reader.ReadUInt16();
            if(Reader.BaseStream.Position != personalNoteOffset - 4) return;
            try { var personalNote = Reader.ReadTeraString(); }
            catch (Exception e) { return; }
            for (int i = 0; i < count; i++)
            {
                if (offset != Reader.BaseStream.Position + 4) return;
                var currentOffset = Reader.ReadUInt16();
                if (currentOffset != offset) return;
                var nextOffset = Reader.ReadUInt16();
                var nameOffset = Reader.ReadUInt16();
                var myNoteOffset = Reader.ReadUInt16();
                var theirNoteOffset = Reader.ReadUInt16();
                var playerId = Reader.ReadUInt32();
                var group = Reader.ReadUInt32();
                var level = Reader.ReadUInt32();
                if(level > 65) return;
                var race = Reader.ReadUInt32();
                var clas = Reader.ReadUInt32();
                var gender = Reader.ReadUInt32();
                try { var rcg = new RaceGenderClass((Race) race, (Gender) gender, (PlayerClass) clas); }
                catch (Exception e) { return; }
                var loc1 = Reader.ReadUInt32();
                var loc2 = Reader.ReadUInt32();
                var loc3 = Reader.ReadUInt32();
                var unk1 = Reader.ReadByte();
                var lastOnline = Reader.ReadUInt64();
                var unk2 = Reader.ReadUInt32();
                var bonds = Reader.ReadUInt32();
                if(bonds > 9999) return;
                if (Reader.BaseStream.Position + 4 != nameOffset) return;
                try { var name = Reader.ReadTeraString(); }
                catch (Exception e) { return; }
                if (Reader.BaseStream.Position + 4 != myNoteOffset) return;
                try { var myNote = Reader.ReadTeraString(); }
                catch (Exception e) { return; }
                if (Reader.BaseStream.Position + 4 != theirNoteOffset) return;
                try { var theirNote = Reader.ReadTeraString(); }
                catch (Exception e) { return; }
                offset = nextOffset;
            }
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
