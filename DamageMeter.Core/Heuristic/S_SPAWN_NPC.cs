using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class S_SPAWN_NPC : AbstractPacketHeuristic
    {
        public static S_SPAWN_NPC Instance => _instance ?? (_instance = new S_SPAWN_NPC());
        private static S_SPAWN_NPC _instance;

        public S_SPAWN_NPC() : base(OpcodeEnum.S_SPAWN_NPC) { }

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                if (OpcodeFinder.Instance.GetOpcode(OPCODE) == message.OpCode)
                {
                    Reader.Skip(10);
                    var id = Reader.ReadUInt64();
                    AddNpcToDatabase(id);
                }
                return;
            }
            if (message.Payload.Count < 100) return;

            Reader.Skip(10);
            var cid = Reader.ReadUInt64();
            Reader.Skip(8+4+4+4+2); //skipped pos, may be useful for more accuracy -- subtracted 2
            var unk1 = Reader.ReadInt32();
            if (unk1 != 0xC && unk1 != 0xA) return;
            var templateId = Reader.ReadUInt32();
            var zoneId = Reader.ReadUInt32(); //can check on current player location, need to add player location to db tho
            Reader.Skip(2);
            var unk5 = Reader.ReadUInt16();
            if (unk5 != 0 && unk5 != 4) return;
            Reader.Skip(2);
            var unk7 = Reader.ReadUInt32();
            if (unk7 != 0 && unk7 != 5) return;
            var unk8 = Reader.ReadByte();
            if (unk8 != 1) return;
            Reader.Skip(1);
            var unk10 = Reader.ReadUInt32();
            if (unk10 != 0 && unk10 != 1 && unk10 != 3 && unk10 != 4) return;
            Reader.Skip(8 + 2 + 2);
            var unk14 = Reader.ReadInt32();
            if (unk14 != 0) return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            AddNpcToDatabase(cid);
        }

        private void AddNpcToDatabase(ulong id)
        {
            List<ulong> list = new List<ulong>();
            if (OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.SpawnedNpcs, out Tuple<Type, object> result))
            {
                OpcodeFinder.Instance.KnowledgeDatabase.Remove(OpcodeFinder.KnowledgeDatabaseItem.SpawnedNpcs);
                list = (List<ulong>)result.Item2;
            }
            if (!list.Contains(id)) list.Add(id);
            OpcodeFinder.Instance.KnowledgeDatabase.Add(OpcodeFinder.KnowledgeDatabaseItem.SpawnedNpcs, new Tuple<Type, object>(typeof(List<ulong>), list));
        }
    }
}
